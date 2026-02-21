using System;
using System.Collections.Generic;
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
using Tool.Utils;
using EasySave.GUI;

namespace EasySave.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BackupJob> BackupJobs { get; set; }

        private readonly BusinessSoftwareMonitor _businessMonitor;
        private readonly BackupExecutionManager _backupManager;

        private BackupJob? _selectedJob;

        public LanguageManager Lang => LanguageManager.Instance;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ChangeLanguageCommand { get; }
        public ICommand CreateJobCommand { get; }
        public ICommand RunSelectionCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged(nameof(SelectedJob));
            }
        }

        public MainViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>();
            _businessMonitor = BusinessSoftwareMonitor.Instance;
            _backupManager = new BackupExecutionManager();

            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            CreateJobCommand = new RelayCommand(_ => CreateNewJob());
            RunSelectionCommand = new RelayCommand(_ => RunAllJobsParallel());
            DeleteJobCommand = new RelayCommand(_ => DeleteSelectedJob(), _ => SelectedJob != null);
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
        }

        private void ChangeLanguage(string? languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;
            LanguageManager.Instance.ChangeLanguage(languageCode);
            OnPropertyChanged(nameof(Lang));
        }

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

                    BackupJobs.Add(job);

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

        private void DeleteSelectedJob()
        {
            if (SelectedJob == null) return;

            var result = MessageBox.Show(
                $"Delete job '{SelectedJob.name}'?",
                "Delete Job",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                BackupJobs.Remove(SelectedJob);
                MessageBox.Show($"Job deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Runs ALL jobs in PARALLEL (simultaneously)
        /// Each job gets its own ProgressWindow
        /// </summary>
        private async void RunAllJobsParallel()
        {
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

            if (_businessMonitor.IsBusinessSoftwareRunning())
            {
                var settings = AppSettings.Instance;
                MessageBox.Show(
                    string.Format(Lang["BackupBlockedMessage"], settings.BusinessSoftware),
                    Lang["BackupBlocked"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Create ProgressWindow for each job
            var progressWindows = new List<Window>();
            var tasks = new List<Task>();

            for (int jobId = 0; jobId < BackupJobs.Count; jobId++)
            {
                var jobToRun = BackupJobs[jobId];
                var backendJob = _backupManager.getJobById(jobId);

                if (backendJob == null) continue;

                // Count total files
                int totalFiles = CountTotalFiles(jobToRun.sourcePath);

                // Create ProgressViewModel for this job
                var progressVM = new ProgressViewModel
                {
                    JobName = jobToRun.name,
                    TotalFiles = totalFiles,
                    ProcessedFiles = 0,
                    ProgressPercentage = 0,
                    CurrentFile = "Starting...",
                    TransferSpeed = "0 MB/s",
                    TimeRemaining = "Calculating..."
                };


                if (backendJob != null)
                {
                    // On s'abonne au changement de statut
                    backendJob.progressObserver.OnStatusChanged += (newStatus) =>
                    {
                        // IMPORTANT : On doit retourner sur le thread principal (UI Thread)
                        // pour modifier des objets liés à l'interface graphique.
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // On met à jour la propriété qui est bindée dans le XAML
                            // Le DataGrid détectera le changement automatiquement
                            jobToRun.status.Status = newStatus;
                        });
                    };
                }

                // Subscribe to progress updates
                backendJob.progressObserver.OnProgressChanged += (fileInfo) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressVM.ProcessedFiles = fileInfo.FilesSaved;
                        progressVM.TotalBytes = fileInfo.TotalSize;
                        progressVM.ProcessedBytes = fileInfo.TotalSize;

                        if (totalFiles > 0)
                        {
                            progressVM.ProgressPercentage = (fileInfo.FilesSaved / (double)totalFiles) * 100;
                        }

                        if (fileInfo.TotalBackupTime.TotalSeconds > 0)
                        {
                            double bytesPerSecond = fileInfo.TotalSize / fileInfo.TotalBackupTime.TotalSeconds;
                            progressVM.TransferSpeed = $"{bytesPerSecond / 1024 / 1024:F2} MB/s";

                            if (progressVM.ProgressPercentage > 0)
                            {
                                double totalTimeEstimate = fileInfo.TotalBackupTime.TotalSeconds / (progressVM.ProgressPercentage / 100);
                                double remaining = totalTimeEstimate - fileInfo.TotalBackupTime.TotalSeconds;
                                progressVM.TimeRemaining = TimeSpan.FromSeconds(remaining).ToString(@"mm\:ss");
                            }
                        }
                    });
                };

                // Create ProgressWindow for this job
                var progressWindow = new ProgressWindow(progressVM)
                {
                    // Position windows side by side
                    Left = 100 + (jobId * 50),
                    Top = 100 + (jobId * 50)
                };

                progressWindows.Add(progressWindow);

                // Capture jobId in a local variable for the lambda
                int currentJobId = jobId;

                // Create task for this job
                var jobTask = Task.Run(async () =>
                {
                    try
                    {
                        await _backupManager.ExecuteJob(currentJobId);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            progressWindow.Close();
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(
                                $"Backup '{jobToRun.name}' failed: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                            progressWindow.Close();
                        });
                    }
                });

                tasks.Add(jobTask);

                // Show the window (non-blocking)
                progressWindow.Show();
            }

            // Wait for ALL jobs to complete in parallel
            await Task.WhenAll(tasks);

            MessageBox.Show(
                "All backups completed!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private int CountTotalFiles(string path)
        {
            try
            {
                int count = Directory.GetFiles(path).Length;

                foreach (string dir in Directory.GetDirectories(path))
                {
                    count += CountTotalFiles(dir);
                }

                return count;
            }
            catch
            {
                return 0;
            }
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