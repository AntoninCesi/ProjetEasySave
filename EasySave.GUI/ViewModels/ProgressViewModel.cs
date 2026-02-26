using System;
using System.ComponentModel;
using System.Windows.Input;
using EasySave.Commands;
using EasySave.Resources;
using EasySave.Models;
using EasySave.ExecutionManagement;
using Tool.Utils;

namespace EasySave.ViewModels
{

    /// ViewModel for the Progress window following MVVM pattern.
    /// This prepares the UI data structure; actual progress values will come from BackupStateObservator (handled by the backend team).

    public class ProgressViewModel : INotifyPropertyChanged
    {
        private readonly BackupJob _job;
        private readonly BackupExecutionManager _backupManager;
        private readonly int _jobId;
        private string _jobName;
        private int _totalFiles;
        private int _processedFiles;
        private double _progressPercentage;
        private string _currentFile;
        private long _totalBytes;
        private long _processedBytes;
        private string _transferSpeed;
        private string _timeRemaining;
        private string _status;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Language support
        public LanguageManager Lang => LanguageManager.Instance;

        /// <summary>
        /// Name of the backup job currently running
        /// </summary>
        public string JobName
        {
            get => _jobName;
            set
            {
                _jobName = value;
                OnPropertyChanged(nameof(JobName));
            }
        }

        
        /// Total number of files to backup
       
        public int TotalFiles
        {
            get => _totalFiles;
            set
            {
                _totalFiles = value;
                OnPropertyChanged(nameof(TotalFiles));
                OnPropertyChanged(nameof(FilesProgress));
            }
        }

        /// <summary>
        /// Number of files already processed
        /// </summary>
        public int ProcessedFiles
        {
            get => _processedFiles;
            set
            {
                _processedFiles = value;
                OnPropertyChanged(nameof(ProcessedFiles));
                OnPropertyChanged(nameof(FilesProgress));
            }
        }

        /// <summary>
        /// Progress percentage (0-100)
        /// Will be calculated by BackupStateObservator
        /// </summary>
        public double ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                _progressPercentage = Math.Min(100, Math.Max(0, value)); // Clamp entre 0 et 100
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        /// <summary>
        /// Current file being copied
        /// </summary>
        public string CurrentFile
        {
            get => _currentFile;
            set
            {
                _currentFile = value;
                OnPropertyChanged(nameof(CurrentFile));
            }
        }

        /// <summary>
        /// Total size in bytes
        /// </summary>
        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                _totalBytes = value;
                OnPropertyChanged(nameof(TotalBytes));
                OnPropertyChanged(nameof(TotalSizeFormatted));
            }
        }

        /// <summary>
        /// Processed size in bytes
        /// </summary>
        public long ProcessedBytes
        {
            get => _processedBytes;
            set
            {
                _processedBytes = value;
                OnPropertyChanged(nameof(ProcessedBytes));
                OnPropertyChanged(nameof(ProcessedSizeFormatted));
            }
        }

        /// <summary>
        /// Transfer speed (e.g., "5.2 MB/s")
        /// </summary>
        public string TransferSpeed
        {
            get => _transferSpeed;
            set
            {
                _transferSpeed = value;
                OnPropertyChanged(nameof(TransferSpeed));
            }
        }

        /// <summary>
        /// Estimated time remaining (e.g., "2 min 30 sec")
        /// </summary>
        public string TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged(nameof(TimeRemaining));
            }
        }

        /// <summary>
        /// Current status message
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        // Formatted display properties
        public string FilesProgress => $"{ProcessedFiles} / {TotalFiles}";
        public string TotalSizeFormatted => FormatBytes(TotalBytes);
        public string ProcessedSizeFormatted => FormatBytes(ProcessedBytes);

        // Commands
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }

        
        /// Initializes a new instance of ProgressViewModel
       
        public ProgressViewModel(BackupJob job, BackupExecutionManager backupManager, int jobId)
        {
            _job = job ?? throw new ArgumentNullException(nameof(job));
            _backupManager = backupManager ?? throw new ArgumentNullException(nameof(backupManager));
            _jobId = jobId;

            _jobName = job.name;
            _totalFiles = 0;
            _processedFiles = 0;
            _progressPercentage = 0;
            _currentFile = "Initializing...";
            _totalBytes = 0;
            _processedBytes = 0;
            _transferSpeed = "0 MB/s";
            _timeRemaining = "Calculating...";
            _status = "Starting backup...";

            // Initialize commands
            PauseCommand = new RelayCommand(_ => PauseBackup(), _ => CanPause());
            ResumeCommand = new RelayCommand(_ => ResumeBackup(), _ => CanResume());
            StopCommand = new RelayCommand(_ => StopBackup(), _ => CanStop());
        }

        /// Pauses the backup
        private void PauseBackup()
        {
            _backupManager.PauseJob(_jobId);
            Status = "⏸️ Backup paused";
        }

        /// Resumes the backup
        private void ResumeBackup()
        {
            _backupManager.ResumeJob(_jobId);
            Status = "▶️ Backup resumed";
        }

        /// Stops the backup
        private void StopBackup()
        {
            _backupManager.StopJob(_jobId);
            Status = "⏹️ Backup stopped";
        }

        private bool CanPause() => _job?.status.Status == BackupStateResum.ON;
        private bool CanResume() => _job?.status.Status == BackupStateResum.PAUSED;
        private bool CanStop() => _job?.status.Status == BackupStateResum.ON ||
                                  _job?.status.Status == BackupStateResum.PAUSED;

        /// <summary>
        /// Formats bytes to human-readable size
        /// </summary>
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Updates progress from BackupStateObservator
        /// This method will be called by the backend team's observer
        /// </summary>
        /// <param name="processed">Number of files processed</param>
        /// <param name="total">Total number of files</param>
        /// <param name="percentage">Progress percentage</param>
        /// <param name="currentFile">Current file being processed</param>
        public void UpdateProgress(int processed, int total, double percentage, string currentFile)
        {
            ProcessedFiles = processed;
            TotalFiles = total;
            ProgressPercentage = percentage;
            CurrentFile = currentFile;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
