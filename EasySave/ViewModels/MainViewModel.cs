using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;
using EasySave.Services;
using EasySave.View;

namespace EasySave.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Collection of jobs that automatically notifies the UI (DataGrid) on changes
        public ObservableCollection<BackupJob> BackupJobs { get; set; }
        private readonly BusinessSoftwareMonitor _businessMonitor;

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
        /// Logic to open the Create Job window and retrieve the result
        /// </summary>
        private void CreateNewJob()
        {
            // Instantiate the ViewModel for the dialog
            var createVm = new CreateJobViewModel();

            // Create the View and link it to the ViewModel via DataContext
            var dialog = new CreateJobDialog
            {
                Owner = Application.Current.MainWindow,
                DataContext = createVm
            };

            // ShowDialog returns true only if window.DialogResult is set to true
            if (dialog.ShowDialog() == true)
            {
                if (createVm.CreatedJob != null)
                {
                    // Adding to ObservableCollection automatically refreshes the DataGrid
                    BackupJobs.Add(createVm.CreatedJob);

                    MessageBox.Show(
                        $"Job '{createVm.CreatedJob.name}' created successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }

        private void RunSelectedBackups()
        {
            if (BackupJobs.Count == 0)
            {
                MessageBox.Show("No backup jobs to run.", "No Jobs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_businessMonitor.IsBusinessSoftwareRunning())
            {
                var settings = AppSettings.Instance;
                MessageBox.Show($"Cannot start: Business software '{settings.BusinessSoftware}' is running.", "Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Execution logic will be integrated soon.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
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