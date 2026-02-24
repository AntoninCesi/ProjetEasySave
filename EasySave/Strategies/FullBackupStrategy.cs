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
    /// Full backup strategy with encryption and integrated logging
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
                // VÉRIFIER SI ANNULATION DEMANDÉE (STOP)
                if (job.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine($"⏹️  Arrêt demandé pour {job.name}");
                    throw new OperationCanceledException("Backup stopped by user");
                }

                //  VÉRIFIER SI PAUSE DEMANDÉE (PAUSE)
                job.PauseEvent.Wait(job.CancellationTokenSource.Token);

                // Vérification du logiciel métier
                if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
                {
                    DateTime startTime = DateTime.Now;
                    string destFilePath = Path.Combine(destinationPath, file.Name);

                    try
                    {
                        CopyAndEncryptFile(file, destFilePath, job);
                        TimeSpan duration = DateTime.Now - startTime;
                        job.progressObserver.NotifyFileSaved(file.Length, duration);

                        // Log événement logiciel métier
                        LogService.Instance.LogBusinessSoftwareEvent(job.name,
                            "Backup stopped - Business software detected during execution");
                    }
                    catch
                    {
                        job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                    }

                    throw new OperationCanceledException("Business software detected during backup");
                }

                // Copie normale du fichier
                DateTime startTime2 = DateTime.Now;
                string destFilePath2 = Path.Combine(destinationPath, file.Name);

                try
                {
                    CopyAndEncryptFile(file, destFilePath2, job);
                    TimeSpan duration = DateTime.Now - startTime2;
                    job.progressObserver.NotifyFileSaved(file.Length, duration);

                    // Logger chaque fichier copié
                    LogService.Instance.WriteLog(job.name, file.FullName, destFilePath2, file.Length, (long)duration.TotalMilliseconds);
                }
                catch
                {
                    job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                }
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, destSubDirPath, job);
            }
        }

        private void CopyAndEncryptFile(SysFileInfo file, string destFilePath, BackupJob job)
        {
            bool shouldEncrypt = file.Extension.ToLower() == ".txt";

            if (shouldEncrypt)
            {
                string tempEncrypted = destFilePath + ".temp";
                var encryptResult = CryptoSoftService.EncryptFile(file.FullName, tempEncrypted, "DefaultEncryptionKey2025");

                if (encryptResult.Success)
                {
                    if (File.Exists(destFilePath))
                        File.Delete(destFilePath);

                    File.Move(tempEncrypted, destFilePath);
                }
                else
                {
                    CopyFileInterruptible(file.FullName, destFilePath, job);
                }
            }
            else
            {
                CopyFileInterruptible(file.FullName, destFilePath, job);
            }
        }


        /// Nouvelle classe pour Copier un fichier de manière interruptible (pause/stop)

        private void CopyFileInterruptible(string sourcePath, string destPath, BackupJob job)
        {
            const int bufferSize = 81920; // 80 KB buffer
            byte[] buffer = new byte[bufferSize];

            try
            {
                using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                using (FileStream destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                {
                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        //  Vérifier annulation à chaque bloc
                        job.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        // Vérifier pause à chaque bloc
                        job.PauseEvent.Wait(job.CancellationTokenSource.Token);

                        destStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //  Nettoyer le fichier incomplet en cas d'annulation
                try
                {
                    if (File.Exists(destPath))
                    {
                        File.Delete(destPath);
                    }
                }
                catch
                {
                    // Si on ne peut pas supprimer, au moins on a essayé
                }

                // Relancer l'exception pour que le job sache qu'il a été annulé
                throw;
            }
        }
    }
}
