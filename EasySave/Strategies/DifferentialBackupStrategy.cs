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
    /// Differential backup strategy with encryption
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
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
                job.status.Status = BackupStateResum.ACTIVE;
                job.status.LastActionTimestamp = DateTime.Now;

                CopyDirectoryDifferential(job.sourcePath, job.destinationPath, job);

                job.status.Status = BackupStateResum.FINISHED;
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;
            }
            catch (OperationCanceledException)
            {
                // Logiciel métier détecté - laisser l'exception remonter
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;
                throw;  // Re-throw pour que BackupExecutionManager puisse la gérer
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                throw new Exception($"Erreur lors de la sauvegarde différentielle : {ex.Message}", ex);
            }
        }

        private void CopyDirectoryDifferential(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (SysFileInfo file in sourceDir.GetFiles())
            {
                string destFilePath = Path.Combine(destinationPath, file.Name);

                if (NeedsBackup(file, destFilePath))
                {
                    // ⚠️ VÉRIFICATION DU LOGICIEL MÉTIER PENDANT L'EXÉCUTION (v2.0 requirement)
                    if (BusinessSoftwareMonitor.Instance.IsBusinessSoftwareRunning())
                    {
                        Console.WriteLine($"⚠️  Logiciel métier détecté pendant la sauvegarde de {job.name}");
                        Console.WriteLine($"    Fin du transfert du fichier en cours puis arrêt...");

                        // On TERMINE le fichier en cours
                        DateTime startTime = DateTime.Now;

                        try
                        {
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
                            Console.WriteLine($"[ERROR] Failed to copy {file.Name}: {ex.Message}");
                            job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                        }

                        // Logger l'arrêt
                        LogService.Instance.LogBusinessSoftwareEvent(job.name,
                            "Backup stopped - Business software detected during execution");

                        // Arrêter la sauvegarde
                        throw new OperationCanceledException("Business software detected during backup");
                    }

                    // COPIE NORMALE du fichier
                    DateTime startTime2 = DateTime.Now;

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

                        TimeSpan duration = DateTime.Now - startTime2;
                        job.progressObserver.NotifyFileSaved(file.Length, duration);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to copy {file.Name}: {ex.Message}");
                        job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);
                    }
                }
            }

            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryDifferential(subDir.FullName, destSubDirPath, job);
            }
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
    }
}