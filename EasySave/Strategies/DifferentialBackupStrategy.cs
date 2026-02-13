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

<<<<<<< HEAD
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
=======
            if (!Directory.Exists(job.destinationPath))
                Directory.CreateDirectory(job.destinationPath);

            if (job.progressObserver == null)
                job.progressObserver = new BackupProgressObserver();
>>>>>>> feature/backupstate

            job.progressObserver.Reset();

            try
            {
<<<<<<< HEAD
                // Marquer le job comme actif
=======
>>>>>>> feature/backupstate
                job.status.Status = BackupStateResum.ACTIVE;
                job.status.LastActionTimestamp = DateTime.Now;

                // Copie différentielle récursive
                CopyDirectoryDifferential(job.sourcePath, job.destinationPath, job);

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
                // Notifier l'observateur et mettre le statut ERROR
                job.status.Status = BackupStateResum.ERROR;
                job.status.LastActionTimestamp = DateTime.Now;

                // Notification simple pour signaler l'erreur
                job.progressObserver.NotifyFileSaved(0, TimeSpan.Zero);

                throw new Exception($"Erreur lors de la sauvegarde différentielle : {ex.Message}", ex);
            }
        }

        /// <summary>
<<<<<<< HEAD
        /// Copie différentielle récursive (seulement fichiers nouveaux/modifiés)
=======
        /// Copie différentielle récursive (arrêt immédiat en cas d'erreur)
>>>>>>> feature/backupstate
        /// </summary>
        private void CopyDirectoryDifferential(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

<<<<<<< HEAD
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
=======
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            // Copier les fichiers nécessaires
            foreach (SysFileInfo file in sourceDir.GetFiles())
            {
                if (NeedsBackup(file, Path.Combine(destinationPath, file.Name)))
                {
                    DateTime startTime = DateTime.Now;
                    file.CopyTo(Path.Combine(destinationPath, file.Name), true);
                    TimeSpan duration = DateTime.Now - startTime;
                    job.progressObserver.NotifyFileSaved(file.Length, duration);
>>>>>>> feature/backupstate
                }
            }

            // Copier récursivement les sous-répertoires
<<<<<<< HEAD
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
=======
            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryDifferential(subDir.FullName, destSubDirPath, job);
>>>>>>> feature/backupstate
            }
        }

        /// <summary>
        /// Détermine si un fichier nécessite une sauvegarde
        /// </summary>
        private bool NeedsBackup(SysFileInfo sourceFile, string destFilePath)
        {
            try
            {
<<<<<<< HEAD
                // Si le fichier n'existe pas, il faut le sauvegarder
                if (!File.Exists(destFilePath))
                {
=======
                if (!File.Exists(destFilePath))
>>>>>>> feature/backupstate
                    return true;

                SysFileInfo destFile = new SysFileInfo(destFilePath);
<<<<<<< HEAD

                // Comparer la date de dernière modification (tolérance de 1 seconde)
                TimeSpan timeDifference = sourceFile.LastWriteTime - destFile.LastWriteTime;
                if (Math.Abs(timeDifference.TotalSeconds) > 1)
                {
=======
                if (Math.Abs((sourceFile.LastWriteTime - destFile.LastWriteTime).TotalSeconds) > 1)
>>>>>>> feature/backupstate
                    return true;

<<<<<<< HEAD
                // Comparer la taille
=======
>>>>>>> feature/backupstate
                if (sourceFile.Length != destFile.Length)
                    return true;

<<<<<<< HEAD
                // Fichier identique, pas besoin de sauvegarder
=======
>>>>>>> feature/backupstate
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
