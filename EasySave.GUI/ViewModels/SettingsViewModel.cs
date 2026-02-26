using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;
using EasySave.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tool.Utils;

namespace EasySave.ViewModels
{
    public class EncryptionExtensionItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        public string Extension { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public EncryptionExtensionItem(string extension, bool isChecked)
        {
            Extension = extension;
            _isChecked = isChecked;
        }
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings _settings;
        private string _selectedLogFormat;
        private string _businessSoftware;
        private string _selectedLanguage;
        private string _priorityExtensions;
        private string _maxParallelFileSizeKo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> LogFormats { get; }
        public ObservableCollection<string> Languages { get; }

        // Liste des extensions prédéfinies avec checkboxes
        public ObservableCollection<EncryptionExtensionItem> EncryptionExtensionItems { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        private bool _isLocalStorageSelected;
        private bool _isExternalStorageSelected;
        private bool _isBothStorageSelected;

        public bool IsLocalStorageSelected
        {
            get => _isLocalStorageSelected;
            set
            {
                if (_isLocalStorageSelected != value)
                {
                    _isLocalStorageSelected = value;
                    OnPropertyChanged(nameof(IsLocalStorageSelected));
                    if (value) ShowStoragePopup("Local Storage");
                }
            }
        }

        public bool IsExternalStorageSelected
        {
            get => _isExternalStorageSelected;
            set
            {
                if (_isExternalStorageSelected != value)
                {
                    _isExternalStorageSelected = value;
                    OnPropertyChanged(nameof(IsExternalStorageSelected));
                    if (value) ShowStoragePopup("External Storage");
                }
            }
        }

        public bool IsBothStorageSelected
        {
            get => _isBothStorageSelected;
            set
            {
                if (_isBothStorageSelected != value)
                {
                    _isBothStorageSelected = value;
                    OnPropertyChanged(nameof(IsBothStorageSelected));
                    if (value) ShowStoragePopup("Both Storage");
                }
            }
        }

        private void ShowStoragePopup(string storageType)
        {
            MessageBox.Show($"You selected: {storageType}", "Storage Selection",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public string SelectedLogFormat
        {
            get => _selectedLogFormat;
            set { _selectedLogFormat = value; OnPropertyChanged(nameof(SelectedLogFormat)); }
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

        public string PriorityExtensions
        {
            get => _priorityExtensions;
            set { _priorityExtensions = value; OnPropertyChanged(nameof(PriorityExtensions)); }
        }

        public string MaxParallelFileSizeKo
        {
            get => _maxParallelFileSizeKo;
            set { _maxParallelFileSizeKo = value; OnPropertyChanged(nameof(MaxParallelFileSizeKo)); }
        }

        // Extensions disponibles dans les checkboxes
        private static readonly string[] AvailableExtensions = new[]
        {
            ".txt", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt",
            ".pdf", ".csv", ".xml", ".json", ".zip", ".rar", ".7z",
            ".jpg", ".png", ".mp4", ".mp3"
        };

        public SettingsViewModel()
        {
            _settings = AppSettings.Instance;

            LogFormats = new ObservableCollection<string> { "JSON", "XML" };
            Languages = new ObservableCollection<string> { "en-US", "fr-FR" };

            _selectedLogFormat = _settings.LogFormat;
            _businessSoftware = _settings.BusinessSoftware;
            _selectedLanguage = _settings.Language;
            _priorityExtensions = _settings.PriorityExtensionsCsv;
            _maxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();

            // Initialise les checkboxes en cochant celles qui sont dans les settings actuels
            var currentEncryption = (_settings.EncryptionExtensions ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.ToLowerInvariant())
                .ToHashSet();

            EncryptionExtensionItems = new ObservableCollection<EncryptionExtensionItem>(
                AvailableExtensions.Select(ext =>
                    new EncryptionExtensionItem(ext, currentEncryption.Contains(ext)))
            );

            SaveCommand   = new RelayCommand(_ => SaveSettings());
            CancelCommand = new RelayCommand(_ => CloseWindow());
            ResetCommand  = new RelayCommand(_ => ResetToDefaults());
        }

        /// <summary>Construit la string CSV à partir des checkboxes cochées.</summary>
        private string GetEncryptionExtensionsCsv()
        {
            return string.Join(",", EncryptionExtensionItems
                .Where(i => i.IsChecked)
                .Select(i => i.Extension));
        }

        private void SaveSettings()
        {
            try
            {
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

                _settings.LogFormat             = SelectedLogFormat;
                _settings.EncryptionExtensions  = GetEncryptionExtensionsCsv();
                _settings.BusinessSoftware      = BusinessSoftware;
                _settings.Language              = SelectedLanguage;
                _settings.PriorityExtensionsCsv = PriorityExtensions;
                _settings.MaxParallelFileSizeKo = maxSizeKo;

                _settings.Save();
                LogService.Instance.ReloadFromSettings();
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
            if (string.IsNullOrWhiteSpace(extensions)) return true;
            return extensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .All(ext => ext.StartsWith("."));
        }

        private void ResetToDefaults()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to default values?",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _settings.ResetToDefaults();

                SelectedLogFormat     = _settings.LogFormat;
                BusinessSoftware      = _settings.BusinessSoftware;
                SelectedLanguage      = _settings.Language;
                PriorityExtensions    = _settings.PriorityExtensionsCsv;
                MaxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();

                // Recocher les extensions par défaut
                var defaults = (_settings.EncryptionExtensions ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(e => e.ToLowerInvariant())
                    .ToHashSet();

                foreach (var item in EncryptionExtensionItems)
                    item.IsChecked = defaults.Contains(item.Extension);
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
