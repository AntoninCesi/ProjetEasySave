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
    /// <summary>
    /// Main ViewModel for the application following MVVM pattern.
    /// Manages backup jobs display and user interactions.
    /// Business logic for backup execution is handled by BackupController (from Console team).
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BackupJob> BackupJobs { get; set; }
        private readonly BusinessSoftwareMonitor _businessMonitor;

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
            _businessMonitor = BusinessSoftwareMonitor.Instance;

            // Initialize commands
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
            CreateJobCommand = new RelayCommand(_ => CreateNewJob());
            RunSelectionCommand = new RelayCommand(_ => RunSelectedBackups());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // TODO: Load backup jobs from configuration file
            // This will be integrated when Console team's job management is ready
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
        /// Opens dialog to create a new backup job
        /// TODO: Implement job creation dialog
        /// </summary>
        private void CreateNewJob()
        {
            MessageBox.Show(
                "Job creation dialog will be implemented here.\n" +
                "This will allow users to specify:\n" +
                "- Job name\n" +
                "- Source path\n" +
                "- Destination path\n" +
                "- Backup type (Full/Differential)",
                "Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Runs selected backup jobs
        /// Checks for business software before starting
        /// </summary>
        private void RunSelectedBackups()
        {
            // Check if business software is running
            if (_businessMonitor.IsBusinessSoftwareRunning())
            {
                var settings = AppSettings.Instance;
                MessageBox.Show(
                    $"Cannot start backup: Business software '{settings.BusinessSoftware}' is running.\n\n" +
                    "Please close the business software and try again.",
                    "Backup Blocked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                Console.WriteLine($"[{DateTime.Now}] Backup blocked: Business software '{settings.BusinessSoftware}' detected");
                return;
            }

            // TODO: Integrate with Console team's BackupController
            // This will call their backup execution logic
            MessageBox.Show(
                "Backup execution will be integrated here.\n" +
                "This will use the BackupController from the Console team.",
                "Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Opens the Settings window
        /// </summary>
        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = Application.Current.MainWindow
            };
            settingsWindow.ShowDialog();
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