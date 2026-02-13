using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Tool.Utils;

namespace EasySave.ViewModels
{
    public class CreateJobViewModel : INotifyPropertyChanged
    {
        private string _jobName = string.Empty;
        private string _sourcePath = string.Empty;
        private string _destinationPath = string.Empty;
        private string _selectedType;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Language support
        public LanguageManager Lang => LanguageManager.Instance;

        // Properties bound to the View
        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(nameof(JobName)); } }
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(nameof(SourcePath)); } }
        public string DestinationPath { get => _destinationPath; set { _destinationPath = value; OnPropertyChanged(nameof(DestinationPath)); } }
        public string SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(nameof(SelectedType)); } }

        public ObservableCollection<string> BackupTypeNames { get; }

        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseDestinationCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        // Property to hold the newly created job for the MainViewModel to pick up
        public BackupJob? CreatedJob { get; private set; }
        public bool IsConfirmed { get; private set; }

        public CreateJobViewModel()
        {
            // Initialize translated backup type names
            BackupTypeNames = new ObservableCollection<string>
            {
                Lang["Complete"],
                Lang["Differential"]
            };
            _selectedType = BackupTypeNames[0];

            BrowseSourceCommand = new RelayCommand(_ => BrowseSource());
            BrowseDestinationCommand = new RelayCommand(_ => BrowseDestination());
            CreateCommand = new RelayCommand(_ => CreateJob());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void BrowseSource()
        {
            var dialog = new OpenFolderDialog { Title = "Select Source Folder" };
            if (dialog.ShowDialog() == true) SourcePath = dialog.FolderName;
        }

        private void BrowseDestination()
        {
            var dialog = new OpenFolderDialog { Title = "Select Destination Folder" };
            if (dialog.ShowDialog() == true) DestinationPath = dialog.FolderName;
        }

        /// <summary>
        /// Validates input and prepares the BackupJob object
        /// </summary>
        private void CreateJob()
        {
            // Simple validation
            if (string.IsNullOrWhiteSpace(JobName) || string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(DestinationPath))
            {
                MessageBox.Show(Lang["PleaseEnterJobName"], Lang["ValidationError"], MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(SourcePath))
            {
                MessageBox.Show(Lang["SourceNotExist"], Lang["ValidationError"], MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Determine if Complete or Differential based on selected index
            bool isComplete = SelectedType == BackupTypeNames[0];

            // Create the model instance
            CreatedJob = new BackupJob
            {
                name = JobName,
                sourcePath = SourcePath,
                destinationPath = DestinationPath,
                type = isComplete ? BackupTypes.FULL : BackupTypes.DIFFERENTIAL
            };

            IsConfirmed = true;
            CloseDialog(true);
        }

        private void Cancel() => CloseDialog(false);

        /// <summary>
        /// Helper method to close the window from the ViewModel
        /// </summary>
        /// <param name="result">True if job was created, False otherwise</param>
        private void CloseDialog(bool result)
        {
            foreach (Window window in Application.Current.Windows)
            {
                // Find the window that is currently using this ViewModel instance
                if (window.DataContext == this)
                {
                    // Setting DialogResult to true or false automatically closes the window
                    try { window.DialogResult = result; }
                    catch { window.Close(); }
                    break;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}