using System;
using System.IO;
using EasySave.Models;
using EasySave.Strategies;
using Tool.Utils;
using SysFileInfo = System.IO.FileInfo;
using SysDirInfo = System.IO.DirectoryInfo;

namespace EasySave.Strategies
{
    /// <summary>
    /// Stratégie de sauvegarde différentielle optimisée
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

            // Créer le répertoire de destination
            if (!Directory.Exists(job.destinationPath))
            {
                Directory.CreateDirectory(job.destinationPath);
            }

            // Initialiser l'observateur du job
            if (job.progressObserver == null)
            {
                job.progressObserver = new BackupProgressObserver();
            }

            job.progressObserver.Reset();

            try
            {
                // Marquer le job comme actif
                job.status.Status = BackupStateResum.ACTIVE;
                job.status.LastActionTimestamp = DateTime.Now;

                // Copie différentielle récursive
                CopyDirectoryDifferential(job.sourcePath, job.destinationPath, job);

                // Marquer comme terminé
                job.status.Status = BackupStateResum.FINISHED;
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;
                throw new Exception($"Erreur lors de la sauvegarde différentielle : {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Copie différentielle récursive (seulement fichiers nouveaux/modifiés)
        /// </summary>
        private void CopyDirectoryDifferential(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

            // Créer le répertoire de destination
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // Copier les fichiers nécessaires
            SysFileInfo[] files = sourceDir.GetFiles();
            foreach (SysFileInfo file in files)
            {
                try
                {
                    string destFilePath = Path.Combine(destinationPath, file.Name);

                    // Vérifier si le fichier nécessite une sauvegarde
                    if (NeedsBackup(file, destFilePath))
                    {
                        // Enregistrer le début
                        DateTime startTime = DateTime.Now;

                        // Copie physique
                        file.CopyTo(destFilePath, true);

                        // Calculer le temps
                        TimeSpan duration = DateTime.Now - startTime;

                        // Notifier l'observateur
                        job.progressObserver.NotifyFileSaved(file.Length, duration);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la copie de {file.FullName} : {ex.Message}");
                }
            }

            // Copier récursivement les sous-répertoires
            SysDirInfo[] subDirs = sourceDir.GetDirectories();
            foreach (SysDirInfo subDir in subDirs)
            {
                try
                {
                    string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                    CopyDirectoryDifferential(subDir.FullName, destSubDirPath, job);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la copie du répertoire {subDir.FullName} : {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Détermine si un fichier nécessite une sauvegarde
        /// </summary>
        private bool NeedsBackup(SysFileInfo sourceFile, string destFilePath)
        {
            try
            {
                // Si le fichier n'existe pas, il faut le sauvegarder
                if (!File.Exists(destFilePath))
                {
                    return true;
                }

                // Le fichier existe, vérifier s'il a été modifié
                SysFileInfo destFile = new SysFileInfo(destFilePath);

                // Comparer la date de dernière modification (tolérance de 1 seconde)
                TimeSpan timeDifference = sourceFile.LastWriteTime - destFile.LastWriteTime;
                if (Math.Abs(timeDifference.TotalSeconds) > 1)
                {
                    return true;
                }

                // Comparer la taille
                if (sourceFile.Length != destFile.Length)
                {
                    return true;
                }

                // Fichier identique, pas besoin de sauvegarder
                return false;
            }
            catch
            {
                // En cas d'erreur, sauvegarder par sécurité
                return true;
            }
        }
    }
}