using System;
using System.IO;
using System.Linq;
using System.Threading;
using EasySave.Models;
using EasySave.Services;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
	/// <summary>
	/// Differential backup strategy.
	/// When running in parallel, respects two rules:
	/// 1. Priority files across all jobs must complete before non-priority files transfer.
	/// 2. Only one large file (> MaxParallelFileSizeKo) can transfer at a time.
	/// Supports Pause/Play/Stop controls (interruptible copy by 80KB blocks).
	/// </summary>
	public class DifferentialBackupStrategy : IBackupStrategy
	{
		private readonly SemaphoreSlim? _largeFileSemaphore;
		private int _pendingPriorityFiles;
		private readonly bool _parallelMode;

		/// <summary>Constructor for sequential use (no parallel controls).</summary>
		public DifferentialBackupStrategy()
		{
			_parallelMode = false;
		}

		/// <summary>Constructor for parallel use — receives shared controls from BackupExecutionManager.</summary>
		public DifferentialBackupStrategy(SemaphoreSlim largeFileSemaphore, ref int pendingPriorityFiles)
		{
			_largeFileSemaphore = largeFileSemaphore;
			_pendingPriorityFiles = pendingPriorityFiles; // NOTE: copie de valeur (voir remarque plus bas)
			_parallelMode = true;
		}

		public void Execute(BackupJob job)
		{
			if (job == null) throw new ArgumentNullException(nameof(job));
			if (string.IsNullOrWhiteSpace(job.sourcePath)) throw new ArgumentException("Le chemin source ne peut pas être vide");
			if (string.IsNullOrWhiteSpace(job.destinationPath)) throw new ArgumentException("Le chemin de destination ne peut pas être vide");
			if (!Directory.Exists(job.sourcePath)) throw new DirectoryNotFoundException($"Le répertoire source n'existe pas : {job.sourcePath}");

			if (!Directory.Exists(job.destinationPath))
				Directory.CreateDirectory(job.destinationPath);

			if (job.progressObserver == null)
				job.progressObserver = new BackupProgressObserver();

			job.progressObserver.Reset();

			try
			{
				job.status.Status = BackupStateResum.ACTIVE;
				job.progressObserver.UpdateStatus(BackupStateResum.ACTIVE);
				job.status.LastActionTimestamp = DateTime.Now;

				// Pre-register priority files that actually need backup (differential check)
				if (_parallelMode)
					RegisterPendingPriorityFiles(job.sourcePath, job.destinationPath);

				CopyDirectoryDifferential(job.sourcePath, job.destinationPath, job);

				job.status.Status = BackupStateResum.FINISHED;
				job.progressObserver.UpdateStatus(BackupStateResum.FINISHED);
				job.status.Progress = 100;
				job.status.LastActionTimestamp = DateTime.Now;
			}
			catch (OperationCanceledException)
			{
				job.status.Status = BackupStateResum.ERROR;
				job.progressObserver.UpdateStatus(BackupStateResum.ERROR);
				job.status.LastActionTimestamp = DateTime.Now;
				throw;
			}
			catch (Exception ex)
			{
				job.status.Status = BackupStateResum.ERROR;
				job.progressObserver.UpdateStatus(BackupStateResum.ERROR);
				job.status.LastActionTimestamp = DateTime.Now;
				job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);

				LogService.Instance.LogBusinessSoftwareEvent(job.name, $"Differential backup error: {ex.Message}");
				throw new Exception($"Erreur lors de la sauvegarde différentielle : {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Counts only the priority files that actually need to be transferred (differential check)
		/// and increments the shared counter accordingly.
		/// </summary>
		private void RegisterPendingPriorityFiles(string sourcePath, string destinationPath)
		{
			foreach (var filePath in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
			{
				if (!IsPriorityFile(Path.GetExtension(filePath))) continue;

				string relativePath = Path.GetRelativePath(sourcePath, filePath);
				string destFilePath = Path.Combine(destinationPath, relativePath);

				if (NeedsBackup(new SysFileInfo(filePath), destFilePath))
					Interlocked.Increment(ref _pendingPriorityFiles);
			}
		}

		private void CopyDirectoryDifferential(string sourcePath, string destinationPath, BackupJob job)
		{
			SysDirInfo sourceDir = new SysDirInfo(sourcePath);

			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			// Priority files first, then normal files
			var files = sourceDir.GetFiles()
				.OrderByDescending(f => IsPriorityFile(f.Extension))
				.ToList();

			foreach (SysFileInfo file in files)
			{
				string destFilePath = Path.Combine(destinationPath, file.Name);

				// STOP demandé ?
				if (job.CancellationTokenSource.Token.IsCancellationRequested)
				{
					Console.WriteLine($"⏹️  Arrêt demandé pour {job.name}");
					throw new OperationCanceledException("Backup stopped by user");
				}

				// PAUSE demandée ?
				job.PauseEvent.Wait(job.CancellationTokenSource.Token);

				// Diff : skip si pas besoin
				if (!NeedsBackup(file, destFilePath))
					continue;

				// Logiciel métier : finir le fichier puis stop
				if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
				{
					CopyFileWithControls(file, destFilePath, job);

					LogService.Instance.LogBusinessSoftwareEvent(job.name,
						"Backup stopped - Business software detected during execution");

					throw new OperationCanceledException("Business software detected during backup");
				}

				CopyFileWithControls(file, destFilePath, job);
			}

			foreach (SysDirInfo subDir in sourceDir.GetDirectories())
			{
				CopyDirectoryDifferential(subDir.FullName, Path.Combine(destinationPath, subDir.Name), job);
			}
		}

		/// <summary>
		/// Copies one file while enforcing parallel rules:
		/// - Non-priority files wait until _pendingPriorityFiles == 0
		/// - Large files acquire the semaphore before transferring
		/// - Supports interruptible copy (Pause/Stop)
		/// </summary>
		private void CopyFileWithControls(SysFileInfo file, string destFilePath, BackupJob job)
		{
			bool isPriority = IsPriorityFile(file.Extension);
			bool isLargeFile = IsLargeFile(file.Length);
			bool semaphoreAcquired = false;

			// RULE 1: non-priority files wait for all priority files to complete
			if (_parallelMode && !isPriority)
			{
				while (Volatile.Read(ref _pendingPriorityFiles) > 0)
				{
					job.CancellationTokenSource.Token.ThrowIfCancellationRequested();
					job.PauseEvent.Wait(job.CancellationTokenSource.Token);
					Thread.Sleep(100);
				}
			}

			DateTime startTime = DateTime.Now;

			try
			{
				// RULE 2: large files acquire the semaphore (one at a time)
				if (_parallelMode && isLargeFile && _largeFileSemaphore != null)
				{
					_largeFileSemaphore.Wait(job.CancellationTokenSource.Token);
					semaphoreAcquired = true;
				}

				CopyAndEncryptFile(file, destFilePath, job);

				TimeSpan duration = DateTime.Now - startTime;

				// ✅ LOG fichier copié (ms)
				long transferMs = (long)duration.TotalMilliseconds;
				LogService.Instance.WriteLog(job.name, file.FullName, destFilePath, file.Length, transferMs);

				job.progressObserver.NotifyFileSaved(file.Length, duration);
			}
			catch (OperationCanceledException)
			{
				job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
				throw;
			}
			catch (Exception ex)
			{
				job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
				LogService.Instance.LogBusinessSoftwareEvent(job.name, $"Copy error: {file.FullName} - {ex.Message}");
			}
			finally
			{
				if (semaphoreAcquired && _largeFileSemaphore != null)
					_largeFileSemaphore.Release();

				// Decrement global counter once a priority file is done
				if (_parallelMode && isPriority)
					Interlocked.Decrement(ref _pendingPriorityFiles);
			}
		}

		private bool IsPriorityFile(string extension)
		{
			var priorities = AppSettings.Instance.GetPriorityExtensionsArray();
			return priorities.Length > 0 && priorities.Contains(extension.ToLower());
		}

		private bool IsLargeFile(long sizeBytes)
		{
			long maxKo = AppSettings.Instance.MaxParallelFileSizeKo;
			return maxKo > 0 && (sizeBytes / 1024) >= maxKo;
		}

		private bool NeedsBackup(SysFileInfo sourceFile, string destFilePath)
		{
			try
			{
				if (!File.Exists(destFilePath))
					return true;

				SysFileInfo destFile = new SysFileInfo(destFilePath);

				if (Math.Abs((sourceFile.LastWriteTime - destFile.LastWriteTime).TotalSeconds) > 1)
					return true;

				if (sourceFile.Length != destFile.Length)
					return true;

				return false;
			}
			catch
			{
				return true;
			}
		}

		/// <summary>
		/// Copy with optional encryption and Pause/Stop interruptible support.
		/// Encryption rule: based on AppSettings.EncryptionExtensions list.
		/// </summary>
		private void CopyAndEncryptFile(SysFileInfo file, string destFilePath, BackupJob job)
		{
			bool shouldEncrypt = AppSettings.Instance
				.EncryptionExtensions
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => s.ToLower())
				.Contains(file.Extension.ToLower());

			if (shouldEncrypt)
			{
				string tempEncrypted = destFilePath + ".temp";
				var encryptResult = CryptoSoftService.EncryptFile(file.FullName, tempEncrypted, "DefaultEncryptionKey2025");

				if (encryptResult.Success)
				{
					if (File.Exists(destFilePath))
						File.Delete(destFilePath);

					File.Move(tempEncrypted, destFilePath);
					return;
				}

				// fallback copie interruptible si cryptage échoue
				CopyFileInterruptible(file.FullName, destFilePath, job);
			}
			else
			{
				CopyFileInterruptible(file.FullName, destFilePath, job);
			}
		}

		/// <summary>
		/// Interruptible file copy (checks Stop/Pause each 80KB block).
		/// </summary>
		private void CopyFileInterruptible(string sourcePath, string destPath, BackupJob job)
		{
			const int bufferSize = 81920; // 80 KB
			byte[] buffer = new byte[bufferSize];

			try
			{
				using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
				using (FileStream destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
				{
					int bytesRead;
					while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						job.CancellationTokenSource.Token.ThrowIfCancellationRequested();
						job.PauseEvent.Wait(job.CancellationTokenSource.Token);

						destStream.Write(buffer, 0, bytesRead);
					}
				}
			}
			catch (OperationCanceledException)
			{
				// cleanup fichier partiel
				try
				{
					if (File.Exists(destPath))
						File.Delete(destPath);
				}
				catch { }

				throw;
			}
		}
	}
}