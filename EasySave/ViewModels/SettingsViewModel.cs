using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;

namespace EasySave.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings window following MVVM pattern.
    /// Manages application settings and provides commands for user interactions.
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings _settings;
        private string _selectedLogFormat;
        private string _encryptionExtensions;
        private string _businessSoftware;
        private string _selectedLanguage;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Available options for dropdowns
        public ObservableCollection<string> LogFormats { get; }
        public ObservableCollection<string> Languages { get; }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        /// <summary>
        /// Gets or sets the selected log format (JSON or XML)
        /// </summary>
        public string SelectedLogFormat
        {
            get => _selectedLogFormat;
            set
            {
                _selectedLogFormat = value;
                OnPropertyChanged(nameof(SelectedLogFormat));
            }
        }

        /// <summary>
        /// Gets or sets the comma-separated list of file extensions to encrypt
        /// </summary>
        public string EncryptionExtensions
        {
            get => _encryptionExtensions;
            set
            {
                _encryptionExtensions = value;
                OnPropertyChanged(nameof(EncryptionExtensions));
            }
        }

        /// <summary>
        /// Gets or sets the business software process name to monitor
        /// </summary>
        public string BusinessSoftware
        {
            get => _businessSoftware;
            set
            {
                _businessSoftware = value;
                OnPropertyChanged(nameof(BusinessSoftware));
            }
        }

        /// <summary>
        /// Gets or sets the selected application language
        /// </summary>
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }

        /// <summary>
        /// Initializes a new instance of SettingsViewModel
        /// </summary>
        public SettingsViewModel()
        {
            _settings = AppSettings.Instance;

            // Initialize available options
            LogFormats = new ObservableCollection<string> { "JSON", "XML" };
            Languages = new ObservableCollection<string> { "en-US", "fr-FR" };

            // Load current settings
            _selectedLogFormat = _settings.LogFormat;
            _encryptionExtensions = _settings.EncryptionExtensions;
            _businessSoftware = _settings.BusinessSoftware;
            _selectedLanguage = _settings.Language;

            // Initialize commands
            SaveCommand = new RelayCommand(_ => SaveSettings());
            CancelCommand = new RelayCommand(_ => CloseWindow());
            ResetCommand = new RelayCommand(_ => ResetToDefaults());
        }

        /// <summary>
        /// Saves current settings and closes the window
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // Validate extensions format
                if (!ValidateExtensions(EncryptionExtensions))
                {
                    MessageBox.Show(
                        "Invalid extension format. Use comma-separated extensions like: .docx,.xlsx,.pdf",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Update settings
                _settings.LogFormat = SelectedLogFormat;
                _settings.EncryptionExtensions = EncryptionExtensions;
                _settings.BusinessSoftware = BusinessSoftware;
                _settings.Language = SelectedLanguage;

                // Save to file
                _settings.Save();

                // Apply language change immediately
                LanguageManager.Instance.ChangeLanguage(SelectedLanguage);

                // Close window
                CloseWindow();

                MessageBox.Show(
                    "Settings saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving settings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Validates the format of encryption extensions
        /// </summary>
        /// <param name="extensions">Comma-separated extension string</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateExtensions(string extensions)
        {
            if (string.IsNullOrWhiteSpace(extensions))
                return true; // Empty is valid (no encryption)

            var parts = extensions.Split(',');
            return parts.All(ext => ext.Trim().StartsWith("."));
        }

        /// <summary>
        /// Resets all settings to default values
        /// </summary>
        private void ResetToDefaults()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to default values?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _settings.ResetToDefaults();
                
                // Update UI
                SelectedLogFormat = _settings.LogFormat;
                EncryptionExtensions = _settings.EncryptionExtensions;
                BusinessSoftware = _settings.BusinessSoftware;
                SelectedLanguage = _settings.Language;
            }
        }

        /// <summary>
        /// Closes the Settings window
        /// </summary>
        private void CloseWindow()
        {
            // Find and close the window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        /// <summary>
        /// Raises PropertyChanged event for data binding
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
