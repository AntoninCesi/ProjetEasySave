using System;
using EasySave.Models;
using Tool.Utils;

namespace EasySave.StateManagement
{
    /// <summary>
    /// Gère l'état d'un BackupJob et notifie la progression à la UI
    /// </summary>
    public class BackupStateManager
    {
        private readonly BackupJob _job;

        // Propriétés internes
        public int FilesSaved { get; private set; }
        public long TotalSize { get; private set; }
        public TimeSpan Elapsed { get; private set; }
        public string JobName => _job.name; // Nom exposé à la UI

        // Événement que la UI peut écouter pour afficher la progression
        // Paramètres : FilesSaved, TotalSize, Elapsed, JobName
        public event Action<int, long, TimeSpan, string> ProgressChanged;

        public BackupStateManager(BackupJob job)
        {
            _job = job ?? throw new ArgumentNullException(nameof(job));

            // S'abonner à l'observer du job
            _job.progressObserver.OnProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(FileInfos info)
        {
            // Mettre à jour les propriétés internes
            FilesSaved = info.FilesSaved;
            TotalSize = info.TotalSize;
            Elapsed = info.TotalBackupTime;

            // Notifier la UI via un événement (sans exposer le modèle)
            ProgressChanged?.Invoke(FilesSaved, TotalSize, Elapsed, JobName);
        }
    }
}