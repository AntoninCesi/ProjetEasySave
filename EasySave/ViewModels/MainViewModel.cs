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
using System.IO;

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

            // === TEST CRYPTOSOFT ===
            TestCryptoSoft();
            return;
            // === FIN TEST ===

            // Check if there are any jobs
            if (BackupJobs.Count == 0)
            {
                MessageBox.Show(
                    "No backup jobs to run.\n\nPlease create a job first by clicking 'Create New Job'.",
                    "No Jobs",
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
                    $"Cannot start backup: Business software '{settings.BusinessSoftware}' is running.\n\n" +
                    "Please close the business software and try again.",
                    "Backup Blocked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                Console.WriteLine($"[{DateTime.Now}] Backup blocked: Business software '{settings.BusinessSoftware}' detected");
                return;
            }

            // TEMPORARY: Show progress window for demo
            var progressVM = new ProgressViewModel
            {
                JobName = "Backup Test",
                TotalFiles = 150,
                ProcessedFiles = 67,
                ProgressPercentage = 44.7,
                CurrentFile = @"C:\Users\Documents\Photos\Vacances2024.jpg",
                TotalBytes = 1024L * 1024L * 750,
                ProcessedBytes = 1024L * 1024L * 335,
                TransferSpeed = "12.5 MB/s",
                TimeRemaining = "2 min 15 sec"
            };

            var progressWindow = new ProgressWindow(progressVM);
            progressWindow.ShowDialog();

            // TODO: When colleague finishes BackupStateObservator, replace above with:
            // - Create ProgressViewModel
            // - Pass it to BackupStateObservator
            // - Start the actual backup with progress updates
        }

        private void TestCryptoSoft()
        {
            try
            {
                // Create a test file
                string testFile = Path.Combine(Path.GetTempPath(), "test_crypto.txt");
                string encryptedFile = testFile + ".encrypted";
                string decryptedFile = Path.Combine(Path.GetTempPath(), "test_crypto_decrypted.txt");

                // Write test content
                File.WriteAllText(testFile, "Hello World! This is a test file for CryptoSoft encryption.");

                MessageBox.Show($"Original file created:\n{testFile}\n\nContent: Hello World! This is a test...");

                // Encrypt
                bool encryptSuccess = CryptoSoftService.EncryptFile(testFile, encryptedFile, "TestKey123");

                if (encryptSuccess)
                {
                    MessageBox.Show($"✅ Encryption successful!\n\nEncrypted file:\n{encryptedFile}\n\nCheck the console for multi-threading logs!");

                    // Decrypt
                    bool decryptSuccess = CryptoSoftService.DecryptFile(encryptedFile, decryptedFile, "TestKey123");

                    if (decryptSuccess)
                    {
                        string decryptedContent = File.ReadAllText(decryptedFile);
                        MessageBox.Show($"✅ Decryption successful!\n\nDecrypted content:\n{decryptedContent}");

                        // Cleanup
                        File.Delete(testFile);
                        File.Delete(encryptedFile);
                        File.Delete(decryptedFile);
                    }
                }
                else
                {
                    MessageBox.Show("❌ Encryption failed!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}");
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