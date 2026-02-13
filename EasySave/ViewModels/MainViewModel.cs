using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EasySave.Commands;
using EasySave.ExecutionManagement;
using EasySave.Models;
using EasySave.Resources;
using EasySave.Services;
using EasySave.Strategies;
using EasySave.View;
using Tool.Utils;

namespace EasySave.ViewModels
{
    /// <summary>
    /// Main ViewModel with full backend integration
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // Collection of jobs that automatically notifies the UI (DataGrid) on changes
        public ObservableCollection<BackupJob> BackupJobs { get; set; }

        private readonly BusinessSoftwareMonitor _businessMonitor;
        private readonly BackupExecutionManager _backupManager;

        public LanguageManager Lang => LanguageManager.Instance;
        public event PropertyChangedEventHandler? PropertyChanged;

        // Commands for UI interactions
        public ICommand ChangeLanguageCommand { get; }
        public ICommand CreateJobCommand { get; }
        public ICommand RunSelectionCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public MainViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>();
            _businessMonitor = BusinessSoftwareMonitor.Instance;
            _backupManager = new BackupExecutionManager();

            // Initialize commands with their respective methods
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            CreateJobCommand = new RelayCommand(_ => CreateNewJob());
            RunSelectionCommand = new RelayCommand(_ => RunSelectedBackups());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        }

        private void ChangeLanguage(string? languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;
            LanguageManager.Instance.ChangeLanguage(languageCode);
            OnPropertyChanged(nameof(Lang));
        }

        /// <summary>
        /// Opens dialog to create a new backup job
        /// </summary>
        private void CreateNewJob()
        {
            var createVm = new CreateJobViewModel();
            var dialog = new CreateJobDialog
            {
                Owner = Application.Current.MainWindow,
                DataContext = createVm
            };

            if (dialog.ShowDialog() == true)
            {
                if (createVm.CreatedJob != null)
                {
                    var job = createVm.CreatedJob;

                    // Add to UI collection
                    BackupJobs.Add(job);

                    // Add to backend manager
                    _backupManager.createBackupJob(
                        job.name,
                        job.sourcePath,
                        job.destinationPath,
                        job.type
                    );

                    MessageBox.Show(
                        string.Format(Lang["JobCreatedSuccess"], job.name),
                        Lang["Success"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }

        /// <summary>
        /// Runs selected backup jobs with real backend integration
        /// </summary>
        private async void RunSelectedBackups()
        {
            // Check if there are any jobs
            if (BackupJobs.Count == 0)
            {
                MessageBox.Show(
                    Lang["NoJobsMessage"],
                    Lang["NoJobs"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            // Check if business software is running
            if (_businessMonitor.IsBusinessSoftwareRunning())
            {
                var settings = AppSettings.Instance;
                MessageBox.Show(
                    string.Format(Lang["BackupBlockedMessage"], settings.BusinessSoftware),
                    Lang["BackupBlocked"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                Console.WriteLine($"[{DateTime.Now}] Backup blocked: Business software '{settings.BusinessSoftware}' detected");
                return;
            }

            // Get the first job (or you can modify to get selected job from DataGrid)
            var jobToRun = BackupJobs[0];
            int jobId = 0; // Index in the manager's list

            // Create ProgressViewModel
            var progressVM = new ProgressViewModel
            {
                JobName = jobToRun.name,
                TotalFiles = 0,
                ProcessedFiles = 0,
                ProgressPercentage = 0,
                CurrentFile = "Initializing...",
                TransferSpeed = "0 MB/s",
                TimeRemaining = "Calculating..."
            };

            // Get the job from backend manager
            var backendJob = _backupManager.getJobById(jobId);
            if (backendJob == null)
            {
                MessageBox.Show("Job not found in backend!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Subscribe to progress updates
            backendJob.progressObserver.OnProgressChanged += (fileInfo) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    progressVM.ProcessedFiles = fileInfo.FilesSaved;
                    progressVM.TotalBytes = fileInfo.TotalSize;
                    progressVM.ProcessedBytes = fileInfo.TotalSize;

                    // Calculate percentage (basic - you can improve this)
                    if (progressVM.TotalFiles > 0)
                    {
                        progressVM.ProgressPercentage = (fileInfo.FilesSaved / (double)progressVM.TotalFiles) * 100;
                    }

                    // Estimate speed
                    if (fileInfo.TotalBackupTime.TotalSeconds > 0)
                    {
                        double bytesPerSecond = fileInfo.TotalSize / fileInfo.TotalBackupTime.TotalSeconds;
                        progressVM.TransferSpeed = $"{bytesPerSecond / 1024 / 1024:F2} MB/s";
                    }
                });
            };

            // Show progress window
            var progressWindow = new ProgressWindow(progressVM);

            // Execute backup in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _backupManager.ExecuteJob(jobId);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Backup '{jobToRun.name}' completed successfully!",
                            "Backup Complete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                        progressWindow.Close();
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Backup failed: {ex.Message}",
                            "Backup Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        progressWindow.Close();
                    });
                }
            });

            progressWindow.ShowDialog();
        }

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow { Owner = Application.Current.MainWindow };
            settingsWindow.ShowDialog();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}