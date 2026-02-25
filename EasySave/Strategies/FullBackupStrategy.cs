using System;
using System.IO;
using EasySave.Models;
using EasySave.Services;
using EasySave.Strategies;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
    /// <summary>
    /// Full backup strategy with encryption based on checked extensions in Settings
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        private SemaphoreSlim largeFileSemaphore;
        private int pendingPriorityFiles;

        public FullBackupStrategy(SemaphoreSlim largeFileSemaphore, ref int pendingPriorityFiles)
        {
            this.largeFileSemaphore = largeFileSemaphore;
            this.pendingPriorityFiles = pendingPriorityFiles;
        }

        public FullBackupStrategy()
        {
        }

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
                    CopyAndEncryptFile(file, destFilePath);

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
                catch
                {
                    job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                }
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDir = Path.Combine(destinationPath, subDir.Name);

                CopyDirectoryRecursive(subDir.FullName, destSubDir, job);
            }
        }

        /// <summary>
        /// Encrypt only if extension is checked in Settings
        /// </summary>
        private void CopyAndEncryptFile(SysFileInfo file, string destFilePath)
        {
            try
            {
                // liste venant des cases cochées
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

                    if (!result.Success)
                    {
                        Console.WriteLine($"Encryption failed: {file.Name}");

                        file.CopyTo(destFilePath, true);
                    }
                    else
                    {
                        Console.WriteLine($"Encrypted: {file.Name}");
                    }
                }
                else
                {
                    file.CopyTo(destFilePath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption error: {ex.Message}");

                file.CopyTo(destFilePath, true);
            }
        }
    }
}