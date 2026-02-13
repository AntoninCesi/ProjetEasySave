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
    /// Full backup strategy with encryption (no console output)
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (string.IsNullOrWhiteSpace(job.sourcePath))
                throw new ArgumentException("Le chemin source ne peut pas être vide");

            if (string.IsNullOrWhiteSpace(job.destinationPath))
                throw new ArgumentException("Le chemin de destination ne peut pas être vide");

            if (!Directory.Exists(job.sourcePath))
                throw new DirectoryNotFoundException($"Le répertoire source n'existe pas : {job.sourcePath}");

            if (!Directory.Exists(job.destinationPath))
            {
                Directory.CreateDirectory(job.destinationPath);
            }

            if (job.progressObserver == null)
            {
                job.progressObserver = new BackupProgressObserver();
            }

            job.progressObserver.Reset();

            try
            {
                job.status.Status = BackupStateResum.ACTIVE;
                job.status.LastActionTimestamp = DateTime.Now;

                CopyDirectoryRecursive(job.sourcePath, job.destinationPath, job);

                job.status.Status = BackupStateResum.FINISHED;
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                throw new Exception($"Erreur lors de la sauvegarde complète : {ex.Message}", ex);
            }
        }

        private void CopyDirectoryRecursive(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (SysFileInfo file in sourceDir.GetFiles())
            {
                DateTime startTime = DateTime.Now;
                string destFilePath = Path.Combine(destinationPath, file.Name);

                try
                {
                    // Force encryption for .txt files
                    bool shouldEncrypt = file.Extension.ToLower() == ".txt";

                    if (shouldEncrypt)
                    {
                        string tempEncrypted = destFilePath + ".temp";
                        var encryptResult = CryptoSoftService.EncryptFile(
                            file.FullName,
                            tempEncrypted,
                            "DefaultEncryptionKey2025"
                        );

                        if (encryptResult.Success)
                        {
                            if (File.Exists(destFilePath))
                                File.Delete(destFilePath);

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

                    TimeSpan duration = DateTime.Now - startTime;
                    job.progressObserver.NotifyFileSaved(file.Length, duration);
                }
                catch (Exception ex)
                {
                    // Silent error handling - notify observer but continue
                    job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                }
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, destSubDirPath, job);
            }
        }
    }
}