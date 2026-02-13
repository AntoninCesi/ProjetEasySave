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
    /// Stratégie de sauvegarde complète optimisée
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

<<<<<<< HEAD
            // Créer le répertoire de destination
=======
>>>>>>> feature/backupstate
            if (!Directory.Exists(job.destinationPath))
            {
                Directory.CreateDirectory(job.destinationPath);
            }

<<<<<<< HEAD
            // Initialiser l'observateur du job
=======
>>>>>>> feature/backupstate
            if (job.progressObserver == null)
            {
                job.progressObserver = new BackupProgressObserver();
            }

            job.progressObserver.Reset();

            try
            {
<<<<<<< HEAD
                // Marquer le job comme actif
=======
>>>>>>> feature/backupstate
                job.status.Status = BackupStateResum.ACTIVE;
                job.status.LastActionTimestamp = DateTime.Now;

                // Copie récursive
                CopyDirectoryRecursive(job.sourcePath, job.destinationPath, job);

<<<<<<< HEAD
                // Marquer comme terminé
=======
                // Job terminé avec succès
>>>>>>> feature/backupstate
                job.status.Status = BackupStateResum.FINISHED;
                job.status.Progress = 100;
                job.status.LastActionTimestamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Notifier l'observateur de l'erreur
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;

                // On peut créer une notification spéciale dans l'observateur
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero); // peut être adapté pour signaler l'erreur

                throw new Exception($"Erreur lors de la sauvegarde complète : {ex.Message}", ex);
            }
        }

        /// <summary>
<<<<<<< HEAD
        /// Copie récursive des fichiers et dossiers
=======
        /// Copie récursive, arrête la sauvegarde en cas d'erreur
>>>>>>> feature/backupstate
        /// </summary>
        private void CopyDirectoryRecursive(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

<<<<<<< HEAD
            // Créer le répertoire de destination
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // Copier tous les fichiers
            SysFileInfo[] files = sourceDir.GetFiles();
            foreach (SysFileInfo file in files)
            {
                try
                {
                    string destFilePath = Path.Combine(destinationPath, file.Name);

                    // Enregistrer le début
                    DateTime startTime = DateTime.Now;

                    // Copie physique
                    file.CopyTo(destFilePath, true);

                    // Calculer le temps
                    TimeSpan duration = DateTime.Now - startTime;

                    // Notifier l'observateur
                    job.progressObserver.NotifyFileSaved(file.Length, duration);

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
                    CopyDirectoryRecursive(subDir.FullName, destSubDirPath, job);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la copie du répertoire {subDir.FullName} : {ex.Message}");
                }
=======
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            // Copier tous les fichiers
            foreach (SysFileInfo file in sourceDir.GetFiles())
            {
                DateTime startTime = DateTime.Now;
                string destFilePath = Path.Combine(destinationPath, file.Name);

                // Si une exception survient, elle remontera vers Execute
                file.CopyTo(destFilePath, true);

                TimeSpan duration = DateTime.Now - startTime;

                job.progressObserver.NotifyFileSaved(file.Length, duration);
            }

            // Copier récursivement les sous-répertoires
            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, destSubDirPath, job);
>>>>>>> feature/backupstate
            }
        }
    }
}
