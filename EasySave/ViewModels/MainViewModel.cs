using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EasyLog;
using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;
using EasySave.Services;
using EasySave.View;

namespace EasySave.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application following MVVM pattern.
    /// Manages backup jobs and handles user interactions.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BackupJob> BackupJobs { get; set; }
        private readonly List<BackupState> _states = new();
        private readonly StateService _stateService = new();

        // Language support
        public LanguageManager Lang => LanguageManager.Instance;
        public event PropertyChangedEventHandler? PropertyChanged;

        // Commands for user interactions
        public ICommand ChangeLanguageCommand { get; }
        public ICommand CreateJobCommand { get; }
        public ICommand RunSelectionCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        /// <summary>
        /// Initializes a new instance of MainViewModel
        /// </summary>
        public MainViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>();

            // Initialize commands
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            CreateJobCommand = new RelayCommand(_ => CreateNewJob());
            RunSelectionCommand = new RelayCommand(_ => RunSelectedBackups());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // Load test data
            BackupJobs.Add(new BackupJob
            {
                name = "TestJob_Full",
                sourcePath = @"C:\Temp\SourceTest",
                destinationPath = @"C:\Temp\TargetTest"
            });
        }

        /// <summary>
        /// Changes the application language
        /// </summary>
        /// <param name="languageCode">Culture code (e.g., "en-US", "fr-FR")</param>
        private void ChangeLanguage(string? languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;
            LanguageManager.Instance.ChangeLanguage(languageCode);
            OnPropertyChanged(nameof(Lang));
        }

        /// <summary>
        /// Creates a new backup job
        /// TODO: Implement dialog for job creation
        /// </summary>
        private void CreateNewJob()
        {
            var newJob = new BackupJob
            {
                name = $"NewJob_{BackupJobs.Count + 1}",
                sourcePath = string.Empty,
                destinationPath = string.Empty
            };
            BackupJobs.Add(newJob);
        }

        /// <summary>
        /// Runs all selected backup jobs
        /// TODO: Implement job selection mechanism
        /// </summary>
        private void RunSelectedBackups()
        {
            // Currently runs all jobs
            for (int i = 0; i < BackupJobs.Count; i++)
            {
                ExecuteBackup(i);
            }
        }

        /// <summary>
        /// Opens the Settings window
        /// </summary>
        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// Executes a backup job by index
        /// </summary>
        /// <param name="jobIndex">Index of the job in BackupJobs collection</param>
        public void ExecuteBackup(int jobIndex)
        {
            if (jobIndex < 0 || jobIndex >= BackupJobs.Count) return;

            var job = BackupJobs[jobIndex];

            var state = _states.FirstOrDefault(s => s.JobName == job.name);
            if (state == null)
            {
                state = new BackupState { JobName = job.name };
                _states.Add(state);
            }

            state.Status = BackupStatus.ACTIF;
            state.LastActionTimestamp = DateTime.Now;
            state.CurrentSourceUNC = job.sourcePath;
            state.CurrentDestinationUNC = job.destinationPath;

            try
            {
                if (!Directory.Exists(job.sourcePath)) return;

                var files = Directory.GetFiles(job.sourcePath, "*.*", SearchOption.AllDirectories);

                state.TotalFiles = files.Length;
                state.TotalSizeBytes = files.Sum(f => new System.IO.FileInfo(f).Length);
                state.RemainingFiles = state.TotalFiles;
                state.RemainingSizeBytes = state.TotalSizeBytes;
                state.Progress = 0f;

                _stateService.SaveStates(_states);

                foreach (var file in files)
                {
                    var sysFileInfo = new System.IO.FileInfo(file);
                    string destFile = file.Replace(job.sourcePath, job.destinationPath);
                    string destDir = Path.GetDirectoryName(destFile)!;

                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                    state.CurrentSourceUNC = file;
                    state.CurrentDestinationUNC = destFile;

                    var stopWatch = Stopwatch.StartNew();
                    File.Copy(file, destFile, true);
                    stopWatch.Stop();

                    EasyLog.EasyLog.Instance.WriteLog(
                        DateTime.Now,
                        job.name,
                        file,
                        destFile,
                        sysFileInfo.Length,
                        stopWatch.ElapsedMilliseconds
                    );

                    UpdateStateProgress(state, sysFileInfo.Length);
                }

                state.Status = BackupStatus.TERMINE;
                _stateService.SaveStates(_states);
            }
            catch (Exception)
            {
                state.Status = BackupStatus.EN_ERREUR;
                _stateService.SaveStates(_states);
            }
        }

        /// <summary>
        /// Updates the progress of a backup state
        /// </summary>
        /// <param name="state">The backup state to update</param>
        /// <param name="fileSize">Size of the file just backed up</param>
        private void UpdateStateProgress(BackupState state, long fileSize)
        {
            state.RemainingFiles -= 1;
            state.RemainingSizeBytes -= fileSize;
            state.Progress = state.TotalFiles > 0
                ? (float)(state.TotalFiles - state.RemainingFiles) / state.TotalFiles
                : 0f;
            state.LastActionTimestamp = DateTime.Now;
            _stateService.SaveStates(_states);
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
