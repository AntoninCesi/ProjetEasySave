using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

<<<<<<< HEAD
			state.TotalFiles = files.Length;
			state.TotalSizeBytes = files.Sum(f => new 

(f).Length);
=======
        private void ChangeLanguage(string? languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;
            LanguageManager.Instance.ChangeLanguage(languageCode);
            OnPropertyChanged(nameof(Lang));
        }
>>>>>>> feature/wpf-interface

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

<<<<<<< HEAD
			// 3) Copier + logs + update state temps réel
			foreach (var file in files)
			{
				var 

= new FileInfo(file);
=======
            // ShowDialog returns true only if window.DialogResult is set to true
            if (dialog.ShowDialog() == true)
            {
                if (createVm.CreatedJob != null)
                {
                    // Adding to ObservableCollection automatically refreshes the DataGrid
                    BackupJobs.Add(createVm.CreatedJob);
>>>>>>> feature/wpf-interface

                    MessageBox.Show(
                        string.Format(Lang["JobCreatedSuccess"], createVm.CreatedJob.name),
                        Lang["Success"],
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
                string content = "Hello World! This is a test file for CryptoSoft encryption.";

                // Write test content
                File.WriteAllText(testFile, content);

                MessageBox.Show(
                    string.Format(Lang["OriginalFileCreated"], testFile, content),
                    "CryptoSoft Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Encrypt
                var encryptResult = CryptoSoftService.EncryptFile(testFile, encryptedFile, "TestKey123");

                if (encryptResult.Success)
                {
                    string message = string.Format(Lang["EncryptionSuccess"], encryptedFile, encryptResult.TimeMs);
                    message += "\n\n" + Lang["CheckConsole"];

                    MessageBox.Show(
                        message,
                        Lang["Success"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Decrypt
                    var decryptResult = CryptoSoftService.DecryptFile(encryptedFile, decryptedFile, "TestKey123");

                    if (decryptResult.Success)
                    {
                        string decryptedContent = File.ReadAllText(decryptedFile);
                        string decryptMessage = string.Format(Lang["DecryptionSuccess"], decryptResult.TimeMs);
                        decryptMessage += $"\n\n{decryptedContent}";

                        MessageBox.Show(
                            decryptMessage,
                            Lang["Success"],
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        // Cleanup
                        File.Delete(testFile);
                        File.Delete(encryptedFile);
                        File.Delete(decryptedFile);
                    }
                }
                else
                {
                    MessageBox.Show(
                        string.Format(Lang["EncryptionFailed"], encryptResult.ErrorMessage),
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Lang["EncryptionFailed"], ex.Message),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
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