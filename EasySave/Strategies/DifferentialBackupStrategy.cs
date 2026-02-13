using System;
using System.IO;
using EasySave.Models;
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

            if (!Directory.Exists(job.destinationPath))
                Directory.CreateDirectory(job.destinationPath);

            if (job.progressObserver == null)
                job.progressObserver = new BackupProgressObserver();

            job.progressObserver.Reset();

            try
            {
                job.status.Status = BackupStateResum.ACTIVE;
                job.status.LastActionTimestamp = DateTime.Now;

                // Copie différentielle récursive
                CopyDirectoryDifferential(job.sourcePath, job.destinationPath, job);

                // Job terminé avec succès
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
        /// Copie différentielle récursive (arrêt immédiat en cas d'erreur)
        /// </summary>
        private void CopyDirectoryDifferential(string sourcePath, string destinationPath, BackupJob job)
        {
            SysDirInfo sourceDir = new SysDirInfo(sourcePath);

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
                }
            }

            // Copier récursivement les sous-répertoires
            foreach (SysDirInfo subDir in sourceDir.GetDirectories())
            {
                string destSubDirPath = Path.Combine(destinationPath, subDir.Name);
                CopyDirectoryDifferential(subDir.FullName, destSubDirPath, job);
            }
        }

        /// <summary>
        /// Détermine si un fichier nécessite une sauvegarde
        /// </summary>
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
                // En cas d'erreur, sauvegarder par sécurité
                return true;
            }
        }
    }
}
