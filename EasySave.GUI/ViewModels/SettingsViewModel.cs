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
<<<<<<< HEAD
        private string _priorityExtensions;
        private string _maxParallelFileSizeKo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> LogFormats { get; }
        public ObservableCollection<string> Languages { get; }

=======

        public event PropertyChangedEventHandler? PropertyChanged;

        // Available options for dropdowns
        public ObservableCollection<string> LogFormats { get; }
        public ObservableCollection<string> Languages { get; }

        // Commands
>>>>>>> feature/commandeline
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

<<<<<<< HEAD
        public string SelectedLogFormat
        {
            get => _selectedLogFormat;
            set { _selectedLogFormat = value; OnPropertyChanged(nameof(SelectedLogFormat)); }
        }

        public string EncryptionExtensions
        {
            get => _encryptionExtensions;
            set { _encryptionExtensions = value; OnPropertyChanged(nameof(EncryptionExtensions)); }
        }

        public string BusinessSoftware
        {
            get => _businessSoftware;
            set { _businessSoftware = value; OnPropertyChanged(nameof(BusinessSoftware)); }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { _selectedLanguage = value; OnPropertyChanged(nameof(SelectedLanguage)); }
        }

        /// <summary>
        /// Comma-separated list of priority file extensions (e.g. .docx,.xlsx).
        /// Files with these extensions are transferred before all others.
        /// </summary>
        public string PriorityExtensions
        {
            get => _priorityExtensions;
            set { _priorityExtensions = value; OnPropertyChanged(nameof(PriorityExtensions)); }
        }

        /// <summary>
        /// Max file size in Ko above which only one parallel transfer is allowed at a time.
        /// Stored as string for TextBox binding, validated on save.
        /// </summary>
        public string MaxParallelFileSizeKo
        {
            get => _maxParallelFileSizeKo;
            set { _maxParallelFileSizeKo = value; OnPropertyChanged(nameof(MaxParallelFileSizeKo)); }
        }

=======
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
>>>>>>> feature/commandeline
        public SettingsViewModel()
        {
            _settings = AppSettings.Instance;

<<<<<<< HEAD
=======
            // Initialize available options
>>>>>>> feature/commandeline
            LogFormats = new ObservableCollection<string> { "JSON", "XML" };
            Languages = new ObservableCollection<string> { "en-US", "fr-FR" };

            // Load current settings
            _selectedLogFormat = _settings.LogFormat;
            _encryptionExtensions = _settings.EncryptionExtensions;
            _businessSoftware = _settings.BusinessSoftware;
            _selectedLanguage = _settings.Language;
<<<<<<< HEAD
            _priorityExtensions = _settings.PriorityExtensions;
            _maxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();

=======

            // Initialize commands
>>>>>>> feature/commandeline
            SaveCommand = new RelayCommand(_ => SaveSettings());
            CancelCommand = new RelayCommand(_ => CloseWindow());
            ResetCommand = new RelayCommand(_ => ResetToDefaults());
        }

<<<<<<< HEAD
=======
        /// <summary>
        /// Saves current settings and closes the window
        /// </summary>
>>>>>>> feature/commandeline
        private void SaveSettings()
        {
            try
            {
<<<<<<< HEAD
                if (!ValidateExtensions(EncryptionExtensions))
                {
                    MessageBox.Show(
                        "Invalid encryption extensions format. Use comma-separated extensions like: .docx,.xlsx,.pdf",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateExtensions(PriorityExtensions))
                {
                    MessageBox.Show(
                        "Invalid priority extensions format. Use comma-separated extensions like: .docx,.xlsx",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!long.TryParse(MaxParallelFileSizeKo, out long maxSizeKo) || maxSizeKo < 0)
                {
                    MessageBox.Show(
                        "Max parallel file size must be a positive number (in Ko). Use 0 to disable the limit.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

=======
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
>>>>>>> feature/commandeline
                _settings.LogFormat = SelectedLogFormat;
                _settings.EncryptionExtensions = EncryptionExtensions;
                _settings.BusinessSoftware = BusinessSoftware;
                _settings.Language = SelectedLanguage;
<<<<<<< HEAD
                _settings.PriorityExtensions = PriorityExtensions;
                _settings.MaxParallelFileSizeKo = maxSizeKo;

                _settings.Save();

                LanguageManager.Instance.ChangeLanguage(SelectedLanguage);

                CloseWindow();

                MessageBox.Show("Settings saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateExtensions(string extensions)
        {
            if (string.IsNullOrWhiteSpace(extensions))
                return true;

            return extensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .All(ext => ext.StartsWith("."));
        }

=======

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
>>>>>>> feature/commandeline
        private void ResetToDefaults()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to default values?",
<<<<<<< HEAD
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);
=======
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
>>>>>>> feature/commandeline

            if (result == MessageBoxResult.Yes)
            {
                _settings.ResetToDefaults();
<<<<<<< HEAD

=======
                
                // Update UI
>>>>>>> feature/commandeline
                SelectedLogFormat = _settings.LogFormat;
                EncryptionExtensions = _settings.EncryptionExtensions;
                BusinessSoftware = _settings.BusinessSoftware;
                SelectedLanguage = _settings.Language;
<<<<<<< HEAD
                PriorityExtensions = _settings.PriorityExtensions;
                MaxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();
            }
        }

        private void CloseWindow()
        {
=======
            }
        }

        /// <summary>
        /// Closes the Settings window
        /// </summary>
        private void CloseWindow()
        {
            // Find and close the window
>>>>>>> feature/commandeline
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

<<<<<<< HEAD
=======
        /// <summary>
        /// Raises PropertyChanged event for data binding
        /// </summary>
>>>>>>> feature/commandeline
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
