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
    /// Full backup strategy.
    /// When running in parallel, respects two rules:
    /// 1. Priority files across all jobs must complete before non-priority files transfer.
    /// 2. Only one large file (> MaxParallelFileSizeKo) can transfer at a time.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        private readonly SemaphoreSlim? _largeFileSemaphore;
        private int _pendingPriorityFiles;
        private readonly bool _parallelMode;

        /// <summary>Constructor for sequential use (no parallel controls).</summary>
        public FullBackupStrategy()
        {
            _parallelMode = false;
        }

        /// <summary>Constructor for parallel use — receives shared controls from BackupExecutionManager.</summary>
        public FullBackupStrategy(SemaphoreSlim largeFileSemaphore, ref int pendingPriorityFiles)
        {
            _largeFileSemaphore = largeFileSemaphore;
            _pendingPriorityFiles = pendingPriorityFiles;
            _parallelMode = true;
        }

        public void Execute(BackupJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            if (string.IsNullOrWhiteSpace(job.sourcePath)) throw new ArgumentException("Source path cannot be empty");
            if (string.IsNullOrWhiteSpace(job.destinationPath)) throw new ArgumentException("Destination path cannot be empty");
            if (!Directory.Exists(job.sourcePath)) throw new DirectoryNotFoundException($"Source directory not found: {job.sourcePath}");

            if (!Directory.Exists(job.destinationPath))
                Directory.CreateDirectory(job.destinationPath);

            if (job.progressObserver == null)
                job.progressObserver = new BackupProgressObserver();

            job.progressObserver.Reset();

            try
            {
                job.status.Status = BackupStateResum.ON;
                job.progressObserver.UpdateStatus(BackupStateResum.ON);
                job.status.LastActionTimestamp = DateTime.Now;

                // Pre-register all priority files of this job in the shared counter
                if (_parallelMode)
                    RegisterPendingPriorityFiles(job.sourcePath);

                CopyDirectoryRecursive(job.sourcePath, job.destinationPath, job);

                job.status.Status = BackupStateResum.END;
                job.progressObserver.UpdateStatus(BackupStateResum.END);
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;
            }
            catch (OperationCanceledException)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;
                throw;
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.progressObserver.UpdateStatus(BackupStateResum.ERROR);
                job.status.LastActionTimestamp = DateTime.Now;
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                throw new Exception($"Full backup error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Counts all priority files in the source directory and increments
        /// the shared counter so other parallel jobs know to wait.
        /// </summary>
        private void RegisterPendingPriorityFiles(string sourcePath)
        {
            foreach (var file in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                if (IsPriorityFile(Path.GetExtension(file)))
                    Interlocked.Increment(ref _pendingPriorityFiles);
            }
        }

        private void CopyDirectoryRecursive(string sourcePath, string destinationPath, BackupJob job)
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
                if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
                {
                    CopyFileWithControls(file, Path.Combine(destinationPath, file.Name), job);
                    LogService.Instance.LogBusinessSoftwareEvent(job.name,
                        "Backup stopped - Business software detected during execution");
                    throw new OperationCanceledException("Business software detected during backup");
                }

                CopyFileWithControls(file, Path.Combine(destinationPath, file.Name), job);
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
                CopyDirectoryRecursive(subDir.FullName, Path.Combine(destinationPath, subDir.Name), job);
        }

        /// <summary>
        /// Copies one file while enforcing the two parallel rules:
        /// - Non-priority files wait until _pendingPriorityFiles == 0
        /// - Large files acquire the semaphore before transferring
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
                    Console.WriteLine($"[WAIT] {file.Name} waiting for priority files...");
                    Thread.Sleep(100);
                }
            }

            DateTime startTime = DateTime.Now;

            try
            {
                // RULE 2: large files acquire the semaphore (one at a time)
                if (_parallelMode && isLargeFile && _largeFileSemaphore != null)
                {
                    Console.WriteLine($"[WAIT] {file.Name} ({file.Length / 1024} Ko) waiting for large file slot...");
                    _largeFileSemaphore.Wait();
                    semaphoreAcquired = true;
                    Console.WriteLine($"[LOCK] Large file slot acquired for: {file.Name}");
                }

                CopyAndEncryptFile(file, destFilePath);

                TimeSpan duration = DateTime.Now - startTime;
                job.progressObserver.NotifyFileSaved(file.Length, duration);
                LogService.Instance.WriteLog(job.name, file.FullName, destFilePath, file.Length, (long)duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to copy {file.Name}: {ex.Message}");
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
            }
            finally
            {
                if (semaphoreAcquired && _largeFileSemaphore != null)
                {
                    _largeFileSemaphore.Release();
                    Console.WriteLine($"[UNLOCK] Large file slot released after: {file.Name}");
                }

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

        private void CopyAndEncryptFile(SysFileInfo file, string destFilePath)
        {
            bool shouldEncrypt = AppSettings.Instance
                .EncryptionExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(file.Extension.ToLower());

            if (shouldEncrypt)
            {
                string tempEncrypted = destFilePath + ".temp";
                var encryptResult = CryptoSoftService.EncryptFile(file.FullName, tempEncrypted, "DefaultEncryptionKey2025");

                if (encryptResult.Success)
                {
                    if (File.Exists(destFilePath)) File.Delete(destFilePath);
                    File.Move(tempEncrypted, destFilePath);
                }
                else
                {
                    file.CopyTo(destFilePath, true);
                }
            }
            else
            {
                file.CopyTo(destFilePath, true);
            }
        }
    }
}
