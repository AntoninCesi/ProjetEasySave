using System;
using System.Threading;
using EasySave.Models;

namespace EasySave.Strategies
{
    /// <summary>
    /// Observateur simplifié pour suivre la progression des sauvegardes
    /// Stocke : nombre de fichiers, taille totale, temps, notifications d'erreur
    /// </summary>
    public class BackupProgressObserver
    {
        private readonly object _lock = new object();

        private int _filesSaved = 0;
        private long _totalSize = 0;
        private DateTime _backupStartTime;

        // Événement pour notifier les changements
        public event Action<FileInfos> OnProgressChanged;

        // Événement pour notifier les erreurs
        public event Action<string> OnErrorOccurred;

        public BackupProgressObserver()
        {
            _backupStartTime = DateTime.Now;
        }

        /// <summary>
        /// Notifie qu'un fichier a été sauvegardé
        /// </summary>
        public void NotifyFileSaved(long fileSize, TimeSpan fileDuration)
        {
            lock (_lock)
            {
                _filesSaved++;
                _totalSize += fileSize;

                var progress = new FileInfos
                {
                    FilesSaved = _filesSaved,
                    TotalSize = _totalSize,
                    TotalBackupTime = DateTime.Now - _backupStartTime,
                    LastFileDuration = fileDuration
                };

                // Notification asynchrone
                ThreadPool.QueueUserWorkItem(_ => OnProgressChanged?.Invoke(progress));
            }
        }

        /// <summary>
        /// Notifie une erreur
        /// </summary>
        public void NotifyError(string message)
        {
            lock (_lock)
            {
                ThreadPool.QueueUserWorkItem(_ => OnErrorOccurred?.Invoke(message));
            }
        }

        /// <summary>
        /// Récupère les données de progression actuelles
        /// </summary>
        public FileInfos GetProgress()
        {
            lock (_lock)
            {
                return new FileInfos
                {
                    FilesSaved = _filesSaved,
                    TotalSize = _totalSize,
                    TotalBackupTime = DateTime.Now - _backupStartTime,
                    LastFileDuration = TimeSpan.Zero
                };
            }
        }

        /// <summary>
        /// Réinitialise l'observateur
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _filesSaved = 0;
                _totalSize = 0;
                _backupStartTime = DateTime.Now;
            }
        }
    }
}
