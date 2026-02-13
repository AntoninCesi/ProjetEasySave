using System;
using System.ComponentModel;
using System.Windows.Input;
using EasySave.Commands;
using EasySave.Resources;

namespace EasySave.ViewModels
{
    /// <summary>
    /// ViewModel for the Progress window following MVVM pattern.
    /// This prepares the UI data structure; actual progress values will come 
    /// from BackupStateObservator (handled by the backend team).
    /// </summary>
    public class ProgressViewModel : INotifyPropertyChanged
    {
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

        /// <summary>
        /// Total number of files to backup
        /// </summary>
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
                _progressPercentage = value;
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

        // Commands (for future pause/cancel functionality)
        public ICommand PauseCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of ProgressViewModel
        /// </summary>
        public ProgressViewModel()
        {
            _jobName = "Backup Job";
            _totalFiles = 0;
            _processedFiles = 0;
            _progressPercentage = 0;
            _currentFile = "Initializing...";
            _totalBytes = 0;
            _processedBytes = 0;
            _transferSpeed = "0 MB/s";
            _timeRemaining = "Calculating...";
            _status = "Starting backup...";

            // Initialize commands (will be implemented in Version 3.0)
            PauseCommand = new RelayCommand(_ => PauseBackup(), _ => false);
            CancelCommand = new RelayCommand(_ => CancelBackup(), _ => false);
        }

        /// <summary>
        /// Pauses the backup (placeholder for V3.0)
        /// </summary>
        private void PauseBackup()
        {
            // TODO: Implement pause functionality in Version 3.0
        }

        /// <summary>
        /// Cancels the backup (placeholder for V3.0)
        /// </summary>
        private void CancelBackup()
        {
            // TODO: Implement cancel functionality in Version 3.0
        }

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
