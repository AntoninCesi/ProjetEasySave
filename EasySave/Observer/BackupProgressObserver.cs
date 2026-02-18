using System;
using System.Threading;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.Strategies
{
    /// <summary>
    /// Observateur simplifié pour suivre la progression des sauvegardes
    /// Stocke : nombre de fichiers, taille totale, temps
    /// </summary>
    public class BackupProgressObserver
    {
        private readonly object _lock = new object();
        //public BackupStateResum status { get; set; } = BackupStateResum.OFF;
        private int _filesSaved;
        private long _totalSize;
        private DateTime _backupStartTime;

        // Événement de progression
        public event Action<FileInfos> OnProgressChanged;

        // Événement d’erreur
        public event Action<string> OnErrorOccurred;

        public BackupProgressObserver()
        {
            _backupStartTime = DateTime.Now;
        }

        // 1. Définition de l'événement (Action qui transporte le nouvel état)
        public event Action<BackupStateResum> OnStatusChanged;

        // 2. Variable pour stocker l'état actuel en interne (optionnel mais recommandé)
        private BackupStateResum _currentStatus = BackupStateResum.OFF;

        /// <summary>
        /// Met à jour le statut du job et notifie les abonnés (comme le MainViewModel)
        /// </summary>
        /// <param name="newStatus">Le nouvel état de la sauvegarde (ON, END, ERROR...)</param>
        public void UpdateStatus(BackupStateResum newStatus)
        {

            // 3. Notification asynchrone via le ThreadPool 
            // Cela permet au moteur de backup de continuer son travail sans attendre 
            // que l'interface graphique ait fini de se rafraîchir.
            ThreadPool.QueueUserWorkItem(_ =>
            {
                // Le ?.Invoke vérifie si quelqu'un est abonné avant de lancer l'événement
                OnStatusChanged?.Invoke(newStatus);
            });
        }


        public void NotifyFileSaved(long fileSize, TimeSpan fileDuration)
        {
            FileInfos progress;

            lock (_lock)
            {
                _filesSaved++;
                _totalSize += fileSize;

                progress = new FileInfos
                {
                    FilesSaved = _filesSaved,
                    TotalSize = _totalSize,
                    TotalBackupTime = DateTime.Now - _backupStartTime,
                    LastFileDuration = fileDuration
                };
            }

            // Notification asynchrone hors lock
            ThreadPool.QueueUserWorkItem(_ =>
                OnProgressChanged?.Invoke(progress)
            );
        }

        public void NotifyError(string message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
                OnErrorOccurred?.Invoke(message)
            );
        }
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
