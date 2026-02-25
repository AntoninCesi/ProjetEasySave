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
    /// Differential backup strategy with encryption support based on Settings extensions.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        private readonly SemaphoreSlim? _largeFileSemaphore;
        private int _pendingPriorityFiles;
        private readonly bool _parallelMode;

        public DifferentialBackupStrategy()
        {
            _parallelMode = false;
        }

        public DifferentialBackupStrategy(SemaphoreSlim largeFileSemaphore, ref int pendingPriorityFiles)
        {
            _largeFileSemaphore = largeFileSemaphore;
            _pendingPriorityFiles = pendingPriorityFiles;
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

        private void RegisterPendingPriorityFiles(string sourcePath, string destinationPath)
        {
            foreach (var filePath in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                if (!IsPriorityFile(Path.GetExtension(filePath))) continue;

                string relativePath = Path.GetRelativePath(sourcePath, filePath);
                string destFilePath = Path.Combine(destinationPath, relativePath);

                if (NeedsBackup(new SysFileInfo(filePath), destFilePath))
                {
                    Interlocked.Increment(ref _pendingPriorityFiles);
                }
            }
        }

        private void CopyDirectoryDifferential(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            var files = sourceDir.GetFiles()
                .OrderByDescending(f => IsPriorityFile(f.Extension))
                .ToList();

            foreach (SysFileInfo file in files)
            {
                string destFilePath = Path.Combine(destinationPath, file.Name);

                if (job.CancellationTokenSource.Token.IsCancellationRequested)
                    throw new OperationCanceledException("Backup stopped by user");

                job.PauseEvent.Wait(job.CancellationTokenSource.Token);

                if (!NeedsBackup(file, destFilePath))
                    continue;

                if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
                {
                    CopyFileWithControls(file, destFilePath, job);

                    LogService.Instance.LogBusinessSoftwareEvent(job.name,
                        "Backup stopped - Business software detected");

                    throw new OperationCanceledException("Business software detected");
                }

                CopyFileWithControls(file, destFilePath, job);
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                CopyDirectoryDifferential(subDir.FullName, Path.Combine(destinationPath, subDir.Name), job);
            }
        }

        private void CopyFileWithControls(SysFileInfo file, string destFilePath, BackupJob job)
        {
            bool isPriority = IsPriorityFile(file.Extension);
            bool isLargeFile = IsLargeFile(file.Length);
            bool semaphoreAcquired = false;

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
                if (_parallelMode && isLargeFile && _largeFileSemaphore != null)
                {
                    _largeFileSemaphore.Wait(job.CancellationTokenSource.Token);
                    semaphoreAcquired = true;
                }

                CopyAndEncryptFile(file, destFilePath, job);

                TimeSpan duration = DateTime.Now - startTime;

                LogService.Instance.WriteLog(
                    job.name,
                    file.FullName,
                    destFilePath,
                    file.Length,
                    (long)duration.TotalMilliseconds
                );

                job.progressObserver.NotifyFileSaved(file.Length, duration);
            }
            finally
            {
                if (semaphoreAcquired && _largeFileSemaphore != null)
                    _largeFileSemaphore.Release();

                if (_parallelMode && isPriority)
                    Interlocked.Decrement(ref _pendingPriorityFiles);
            }
        }

        private bool IsPriorityFile(string extension)
        {
            var priorities = AppSettings.Instance.GetPriorityExtensionsArray();
            return priorities.Contains(extension.ToLower());
        }

        private bool IsLargeFile(long sizeBytes)
        {
            long maxKo = AppSettings.Instance.MaxParallelFileSizeKo;
            return maxKo > 0 && (sizeBytes / 1024) >= maxKo;
        }

        private bool NeedsBackup(SysFileInfo sourceFile, string destFilePath)
        {
            if (!File.Exists(destFilePath))
                return true;

            SysFileInfo destFile = new SysFileInfo(destFilePath);

            return sourceFile.LastWriteTime != destFile.LastWriteTime
                || sourceFile.Length != destFile.Length;
        }

        /// <summary>
        /// ENCRYPT using Settings extensions
        /// </summary>
        private void CopyAndEncryptFile(SysFileInfo file, string destFilePath, BackupJob job)
        {
            try
            {
                string encryptionExtensions = AppSettings.Instance.EncryptionExtensions;

                bool shouldEncrypt = CryptoSoftService.ShouldEncrypt(
                    file.FullName,
                    encryptionExtensions
                );

                if (shouldEncrypt)
                {
                    var result = CryptoSoftService.EncryptFile(
                        file.FullName,
                        destFilePath
                    );

                    if (result.Success)
                    {
                        Console.WriteLine($"[ENCRYPTED] {file.Name}");
                        return;
                    }
                }

                CopyFileInterruptible(file.FullName, destFilePath, job);
            }
            catch
            {
                CopyFileInterruptible(file.FullName, destFilePath, job);
            }
        }

        private void CopyFileInterruptible(string sourcePath, string destPath, BackupJob job)
        {
            const int bufferSize = 81920;
            byte[] buffer = new byte[bufferSize];

            using FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            using FileStream destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);

            int bytesRead;
            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                job.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                job.PauseEvent.Wait(job.CancellationTokenSource.Token);

                destStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}