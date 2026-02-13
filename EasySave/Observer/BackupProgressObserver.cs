using System;
using System.Threading;
<<<<<<< HEAD
=======
using EasySave.Models;
>>>>>>> feature/backupstate

namespace EasySave.Strategies
{
    /// <summary>
<<<<<<< HEAD
    /// Observateur ultra-simplifié pour suivre la progression des sauvegardes
    /// Stocke SEULEMENT : nombre de fichiers, taille totale, temps
    /// </summary>
    public class BackupProgressObserver
    {
        // Lock pour thread-safety
        private readonly object _lock = new object();

        // Compteurs simples
=======
    /// Observateur simplifié pour suivre la progression des sauvegardes
    /// Stocke : nombre de fichiers, taille totale, temps, notifications d'erreur
    /// </summary>
    public class BackupProgressObserver
    {
        private readonly object _lock = new object();

>>>>>>> feature/backupstate
        private int _filesSaved = 0;
        private long _totalSize = 0;
        private DateTime _backupStartTime;

        // Événement pour notifier les changements
<<<<<<< HEAD
        public event Action<ProgressData> OnProgressChanged;
=======
        public event Action<FileInfos> OnProgressChanged;

        // Événement pour notifier les erreurs
        public event Action<string> OnErrorOccurred;
>>>>>>> feature/backupstate

        public BackupProgressObserver()
        {
            _backupStartTime = DateTime.Now;
        }

        /// <summary>
        /// Notifie qu'un fichier a été sauvegardé
        /// </summary>
<<<<<<< HEAD
        public void NotifyFileSaved(long fileSize, TimeSpan backupDuration)
=======
        public void NotifyFileSaved(long fileSize, TimeSpan fileDuration)
>>>>>>> feature/backupstate
        {
            lock (_lock)
            {
                _filesSaved++;
                _totalSize += fileSize;

<<<<<<< HEAD
                // Créer les données de progression
                var progressData = new ProgressData
=======
                var progress = new FileInfos
>>>>>>> feature/backupstate
                {
                    FilesSaved = _filesSaved,
                    TotalSize = _totalSize,
                    TotalBackupTime = DateTime.Now - _backupStartTime,
<<<<<<< HEAD
                    LastFileDuration = backupDuration
                };

                // Notifier de manière asynchrone
                ThreadPool.QueueUserWorkItem(_ => OnProgressChanged?.Invoke(progressData));
=======
                    LastFileDuration = fileDuration
                };

                // Notification asynchrone
                ThreadPool.QueueUserWorkItem(_ => OnProgressChanged?.Invoke(progress));
>>>>>>> feature/backupstate
            }
        }

        /// <summary>
<<<<<<< HEAD
        /// Récupère les données de progression actuelles
        /// </summary>
        public ProgressData GetProgress()
        {
            lock (_lock)
            {
                return new ProgressData
                {
                    FilesSaved = _filesSaved,
                    TotalSize = _totalSize,
                    TotalBackupTime = DateTime.Now - _backupStartTime,
                    LastFileDuration = TimeSpan.Zero
                };
=======
        /// Notifie une erreur
        /// </summary>
        public void NotifyError(string message)
        {
            lock (_lock)
            {
                ThreadPool.QueueUserWorkItem(_ => OnErrorOccurred?.Invoke(message));
>>>>>>> feature/backupstate
            }
        }

        /// <summary>
<<<<<<< HEAD
=======
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
>>>>>>> feature/backupstate
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
<<<<<<< HEAD

    /// <summary>
    /// Données de progression minimalistes
    /// </summary>
    public class ProgressData
    {
        public int FilesSaved { get; set; }
        public long TotalSize { get; set; }
        public TimeSpan TotalBackupTime { get; set; }
        public TimeSpan LastFileDuration { get; set; }

        public override string ToString()
        {
            return $"Fichiers: {FilesSaved} - Taille: {TotalSize / 1024.0:F2} KB - Temps: {TotalBackupTime:hh\\:mm\\:ss}";
        }
    }
}
=======
}
>>>>>>> feature/backupstate
