using EasySave.Commands;
using EasySave.Models;
using Microsoft.Win32;
using System;
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
        private string _selectedType = "Complete";

        public event PropertyChangedEventHandler? PropertyChanged;

        // Properties bound to the View
        public string JobName { get => _jobName; set { _jobName = value; OnPropertyChanged(nameof(JobName)); } }
        public string SourcePath { get => _sourcePath; set { _sourcePath = value; OnPropertyChanged(nameof(SourcePath)); } }
        public string DestinationPath { get => _destinationPath; set { _destinationPath = value; OnPropertyChanged(nameof(DestinationPath)); } }
        public string SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(nameof(SelectedType)); } }

        public string[] BackupTypeNames { get; } = new[] { "Complete", "Differential" };

        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseDestinationCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        // Property to hold the newly created job for the MainViewModel to pick up
        public BackupJob? CreatedJob { get; private set; }
        public bool IsConfirmed { get; private set; }

        public CreateJobViewModel()
        {
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
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(SourcePath))
            {
                MessageBox.Show("Source folder does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create the model instance
            CreatedJob = new BackupJob
            {
                name = JobName,
                sourcePath = SourcePath,
                destinationPath = DestinationPath,
                type = SelectedType == BackupTypeNames[0] ? BackupTypes.COMPLET : BackupTypes.DIFFERENTIAL
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