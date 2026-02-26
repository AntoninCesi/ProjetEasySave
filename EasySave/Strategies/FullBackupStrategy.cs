using System;
using System.IO;
using System.Linq;
using System.Threading;
using EasySave.Models;
using EasySave.Services;
using EasySave.Strategies;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private SemaphoreSlim largeFileSemaphore = new SemaphoreSlim(1);
        private int pendingPriorityFiles;

        public FullBackupStrategy(SemaphoreSlim largeFileSemaphore, ref int pendingPriorityFiles)
        {
            this.largeFileSemaphore = largeFileSemaphore;
            this.pendingPriorityFiles = pendingPriorityFiles;
        }

        public FullBackupStrategy() { }

        public void Execute(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (string.IsNullOrWhiteSpace(job.sourcePath))
                throw new ArgumentException("Source path cannot be empty");

            if (string.IsNullOrWhiteSpace(job.destinationPath))
                throw new ArgumentException("Destination path cannot be empty");

            if (!Directory.Exists(job.sourcePath))
                throw new DirectoryNotFoundException($"Source directory not found: {job.sourcePath}");

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

        private void CopyDirectoryRecursive(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (SysFileInfo file in sourceDir.GetFiles())
            {
                // STOP
                job.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                // PAUSE
                job.PauseEvent.Wait(job.CancellationTokenSource.Token);

                // Business software check
                if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
                {
                    LogService.Instance.LogBusinessSoftwareEvent(
                        job.name,
                        "Backup stopped - Business software detected"
                    );
                    throw new OperationCanceledException("Business software detected");
                }

                DateTime startTime = DateTime.Now;
                string destFilePath = Path.Combine(destinationPath, file.Name);

                try
                {
                    CopyAndEncryptFile(file, destFilePath, job);

                    TimeSpan duration = DateTime.Now - startTime;
                    job.progressObserver.NotifyFileSaved(file.Length, duration);

                    LogService.Instance.WriteLog(
                        job.name,
                        file.FullName,
                        destFilePath,
                        file.Length,
                        (long)duration.TotalMilliseconds
                    );
                }
                catch (OperationCanceledException)
                {
                    job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                    throw;
                }
                catch
                {
                    job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                }
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                // STOP check between directories too
                job.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                job.PauseEvent.Wait(job.CancellationTokenSource.Token);

                CopyDirectoryRecursive(subDir.FullName, Path.Combine(destinationPath, subDir.Name), job);
            }
        }

        private void CopyAndEncryptFile(SysFileInfo file, string destFilePath, BackupJob job)
        {
            string encryptionExtensions = AppSettings.Instance.EncryptionExtensions;
            bool shouldEncrypt = CryptoSoftService.ShouldEncrypt(file.FullName, encryptionExtensions);

            if (shouldEncrypt)
            {
                var result = CryptoSoftService.EncryptFile(file.FullName, destFilePath);

                if (!result.Success)
                {
                    Console.WriteLine($"Encryption failed: {file.Name}");
                    CopyFileInterruptible(file.FullName, destFilePath, job);
                }
                else
                {
                    Console.WriteLine($"Encrypted: {file.Name}");
                }
            }
            else
            {
                CopyFileInterruptible(file.FullName, destFilePath, job);
            }
        }

        /// <summary>
        /// Interruptible copy — checks Pause/Stop every 80KB
        /// </summary>
        private void CopyFileInterruptible(string sourcePath, string destPath, BackupJob job)
        {
            const int bufferSize = 81920; // 80 KB
            byte[] buffer = new byte[bufferSize];

            try
            {
                using (var src = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                using (var dst = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                {
                    int bytesRead;
                    while ((bytesRead = src.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        job.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                        job.PauseEvent.Wait(job.CancellationTokenSource.Token);
                        dst.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                try { if (File.Exists(destPath)) File.Delete(destPath); } catch { }
                throw;
            }
        }
    }
}