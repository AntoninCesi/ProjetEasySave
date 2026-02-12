using System;
using System.Threading;
using EasySave.Models;

namespace EasySave.Strategies
{
    /// <summary>
    /// Observateur ultra-simplifié pour suivre la progression des sauvegardes
    /// Stocke SEULEMENT : nombre de fichiers, taille totale, temps
    /// </summary>
    public class BackupProgressObserver
    {
        // Lock pour thread-safety
        private readonly object _lock = new object();

        // Compteurs simples
        private int _filesSaved = 0;
        private long _totalSize = 0;
        private DateTime _backupStartTime;

        // Événement pour notifier les changements
        public event Action<FileInfos> OnProgressChanged;

        public BackupProgressObserver()
        {
            _backupStartTime = DateTime.Now;
        }

        /// <summary>
        /// Notifie qu'un fichier a été sauvegardé
        /// </summary>
        public void NotifyFileSaved(long fileSize, TimeSpan backupDuration)
        {
            lock (_lock)
            {
                _filesSaved++;
                _totalSize += fileSize;

                // Créer les données de progression
                var FileInfos = new FileInfos
                {
                    FilesSaved = _filesSaved,
                    TotalSize = _totalSize,
                    TotalBackupTime = DateTime.Now - _backupStartTime,
                    LastFileDuration = backupDuration
                };

                // Notifier de manière asynchrone
                ThreadPool.QueueUserWorkItem(_ => OnProgressChanged?.Invoke(FileInfos));
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