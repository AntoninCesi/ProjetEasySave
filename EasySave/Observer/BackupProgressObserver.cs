using System;
using System.Collections.Generic;
using System.Threading;
using EasySave.Models;
using ModelFileInfo = EasySave.Models.FileInfo;  // ALIAS pour éviter le conflit

namespace EasySave.Strategies
{
    /// <summary>
    /// Observateur thread-safe pour suivre la progression des sauvegardes
    /// Calcule la progression en temps réel et notifie les classes intéressées
    /// </summary>
    public class BackupProgressObserver
    {
        // Lock pour garantir la thread-safety
        private readonly object _lock = new object();

        // Liste des fichiers à sauvegarder
        private readonly List<ModelFileInfo> _filesToBackup = new List<ModelFileInfo>();

        // Liste des fichiers déjà sauvegardés
        private readonly List<ModelFileInfo> _filesSaved = new List<ModelFileInfo>();

        // Statistiques thread-safe
        private long _totalSize = 0;
        private long _savedSize = 0;
        private int _totalFiles = 0;
        private int _savedFiles = 0;

        // Temps de sauvegarde
        private DateTime _backupStartTime;
        private TimeSpan _totalBackupTime = TimeSpan.Zero;

        // Délégué pour notifier les classes intéressées
        public delegate void ProgressChangedHandler(BackupProgressInfo progressInfo);
        public event ProgressChangedHandler OnProgressChanged;

        /// <summary>
        /// Constructeur
        /// </summary>
        public BackupProgressObserver()
        {
            _backupStartTime = DateTime.Now;
        }

        /// <summary>
        /// Ajoute un fichier à la liste des fichiers à sauvegarder
        /// Thread-safe
        /// </summary>
        public void AddFileToBackup(ModelFileInfo fileInfo)
        {
            if (fileInfo == null || fileInfo.isDirectory)
                return;

            lock (_lock)
            {
                _filesToBackup.Add(fileInfo);
                _totalFiles++;
                _totalSize += fileInfo.fileSize;
            }
        }

        /// <summary>
        /// Marque un fichier comme sauvegardé et met à jour la progression
        /// Thread-safe
        /// </summary>
        public void NotifyFileSaved(ModelFileInfo fileInfo)
        {
            if (fileInfo == null || fileInfo.isDirectory)
                return;

            lock (_lock)
            {
                _filesSaved.Add(fileInfo);
                _savedFiles++;
                _savedSize += fileInfo.fileSize;

                // Calculer la progression
                BackupProgressInfo progressInfo = CalculateProgress();

                // Notifier les observateurs en dehors du lock pour éviter les deadlocks
                ThreadPool.QueueUserWorkItem(_ => OnProgressChanged?.Invoke(progressInfo));
            }
        }

        /// <summary>
        /// Calcule la progression actuelle
        /// DOIT être appelé dans un lock
        /// </summary>
        private BackupProgressInfo CalculateProgress()
        {
            double progressPercentage = 0;

            if (_totalSize > 0)
            {
                progressPercentage = (_savedSize * 100.0) / _totalSize;
            }

            // Calculer le temps total écoulé
            TimeSpan elapsedTime = DateTime.Now - _backupStartTime;

            // Calculer le temps moyen par fichier
            TimeSpan averageFileTime = TimeSpan.Zero;
            if (_savedFiles > 0)
            {
                averageFileTime = TimeSpan.FromTicks(elapsedTime.Ticks / _savedFiles);
            }

            // Estimer le temps restant
            TimeSpan estimatedRemainingTime = TimeSpan.Zero;
            if (_savedFiles > 0 && _totalFiles > _savedFiles)
            {
                long remainingFiles = _totalFiles - _savedFiles;
                estimatedRemainingTime = TimeSpan.FromTicks(averageFileTime.Ticks * remainingFiles);
            }

            return new BackupProgressInfo
            {
                TotalFiles = _totalFiles,
                SavedFiles = _savedFiles,
                RemainingFiles = _totalFiles - _savedFiles,
                TotalSize = _totalSize,
                SavedSize = _savedSize,
                RemainingSize = _totalSize - _savedSize,
                ProgressPercentage = (int)Math.Round(progressPercentage),
                CurrentFile = _filesSaved.Count > 0 ? _filesSaved[_filesSaved.Count - 1] : null,
                TotalBackupTime = elapsedTime,
                AverageFileTime = averageFileTime,
                EstimatedRemainingTime = estimatedRemainingTime,
                BackupStartTime = _backupStartTime
            };
        }

        /// <summary>
        /// Récupère la progression actuelle de manière thread-safe
        /// </summary>
        public BackupProgressInfo GetProgress()
        {
            lock (_lock)
            {
                return CalculateProgress();
            }
        }

        /// <summary>
        /// Récupère la liste des fichiers en cours de sauvegarde (thread-safe)
        /// </summary>
        public List<ModelFileInfo> GetFilesToBackup()
        {
            lock (_lock)
            {
                return new List<ModelFileInfo>(_filesToBackup);
            }
        }

        /// <summary>
        /// Récupère la liste des fichiers déjà sauvegardés (thread-safe)
        /// </summary>
        public List<ModelFileInfo> GetSavedFiles()
        {
            lock (_lock)
            {
                return new List<ModelFileInfo>(_filesSaved);
            }
        }

        /// <summary>
        /// Récupère les statistiques de temps détaillées par fichier (thread-safe)
        /// </summary>
        public List<FileBackupTimeInfo> GetFileTimeStatistics()
        {
            lock (_lock)
            {
                var stats = new List<FileBackupTimeInfo>();

                foreach (var file in _filesSaved)
                {
                    stats.Add(new FileBackupTimeInfo
                    {
                        FileName = file.fileName,
                        FilePath = file.filePath,
                        FileSize = file.fileSize,
                        BackupDuration = file.backupDuration,
                        TransferRate = file.fileSize > 0 && file.backupDuration.TotalSeconds > 0
                            ? (long)(file.fileSize / file.backupDuration.TotalSeconds)
                            : 0,
                        Status = file.backupStatus
                    });
                }

                return stats;
            }
        }

        /// <summary>
        /// Récupère le fichier le plus lent et le plus rapide
        /// </summary>
        public (ModelFileInfo Slowest, ModelFileInfo Fastest) GetExtremeFiles()
        {
            lock (_lock)
            {
                if (_filesSaved.Count == 0)
                    return (null, null);

                ModelFileInfo slowest = null;
                ModelFileInfo fastest = null;

                foreach (var file in _filesSaved)
                {
                    if (file.backupStatus != FileBackupStatus.Completed)
                        continue;

                    if (slowest == null || file.backupDuration > slowest.backupDuration)
                        slowest = file;

                    if (fastest == null || file.backupDuration < fastest.backupDuration)
                        fastest = file;
                }

                return (slowest, fastest);
            }
        }

        /// <summary>
        /// Réinitialise l'observateur pour une nouvelle sauvegarde
        /// Thread-safe
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _filesToBackup.Clear();
                _filesSaved.Clear();
                _totalSize = 0;
                _savedSize = 0;
                _totalFiles = 0;
                _savedFiles = 0;
                _backupStartTime = DateTime.Now;
                _totalBackupTime = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Informations de temps de sauvegarde pour un fichier
    /// </summary>
    public class FileBackupTimeInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public TimeSpan BackupDuration { get; set; }
        public long TransferRate { get; set; } // octets par seconde
        public FileBackupStatus Status { get; set; }

        public override string ToString()
        {
            return $"{FileName}: {BackupDuration.TotalMilliseconds:F2}ms " +
                   $"({TransferRate / 1024.0:F2} KB/s)";
        }
    }

    /// <summary>
    /// Informations de progression de la sauvegarde
    /// </summary>
    public class BackupProgressInfo
    {
        public int TotalFiles { get; set; }
        public int SavedFiles { get; set; }
        public int RemainingFiles { get; set; }
        public long TotalSize { get; set; }
        public long SavedSize { get; set; }
        public long RemainingSize { get; set; }
        public int ProgressPercentage { get; set; }
        public ModelFileInfo CurrentFile { get; set; }

        // Statistiques de temps
        public TimeSpan TotalBackupTime { get; set; }
        public TimeSpan AverageFileTime { get; set; }
        public TimeSpan EstimatedRemainingTime { get; set; }
        public DateTime BackupStartTime { get; set; }

        public override string ToString()
        {
            return $"Progress: {ProgressPercentage}% - Files: {SavedFiles}/{TotalFiles} - " +
                   $"Size: {SavedSize}/{TotalSize} bytes - " +
                   $"Time: {TotalBackupTime:hh\\:mm\\:ss} - " +
                   $"Remaining: ~{EstimatedRemainingTime:hh\\:mm\\:ss}";
        }
    }
}