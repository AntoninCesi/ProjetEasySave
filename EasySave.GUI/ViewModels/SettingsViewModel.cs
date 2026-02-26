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

    public class PriorityExtensionItem : INotifyPropertyChanged
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

        public PriorityExtensionItem(string extension, bool isChecked)
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
        private string _maxParallelFileSizeKo;

        public event PropertyChangedEventHandler? PropertyChanged;
        public LanguageManager Lang => LanguageManager.Instance;

        public ObservableCollection<string> LogFormats { get; }
        public ObservableCollection<string> Languages { get; }

        public ObservableCollection<EncryptionExtensionItem> EncryptionExtensionItems { get; }
        public ObservableCollection<PriorityExtensionItem> PriorityExtensionItems { get; }

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
                    if (value) // ← quand on coche Local, on décoche les autres
                    {
                        IsExternalStorageSelected = false;
                        IsBothStorageSelected = false;
                    }
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
                    if (value)
                    {
                        IsLocalStorageSelected = false;
                        IsBothStorageSelected = false;
                    }
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
                    if (value)
                    {
                        IsLocalStorageSelected = false;
                        IsExternalStorageSelected = false;
                    }
                }
            }
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

        public string MaxParallelFileSizeKo
        {
            get => _maxParallelFileSizeKo;
            set { _maxParallelFileSizeKo = value; OnPropertyChanged(nameof(MaxParallelFileSizeKo)); }
        }

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

            _isLocalStorageSelected = _settings.LogDestination == "Local";
            _isExternalStorageSelected = _settings.LogDestination == "Docker";
            _isBothStorageSelected = _settings.LogDestination == "Both";
            _selectedLogFormat = _settings.LogFormat;
            _businessSoftware = _settings.BusinessSoftware;
            _selectedLanguage = LanguageManager.Instance.CurrentLanguageCode;
            _maxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();

            // Checkboxes chiffrement
            var currentEncryption = (_settings.EncryptionExtensions ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.ToLowerInvariant())
                .ToHashSet();

            EncryptionExtensionItems = new ObservableCollection<EncryptionExtensionItem>(
                AvailableExtensions.Select(ext =>
                    new EncryptionExtensionItem(ext, currentEncryption.Contains(ext)))
            );

            // Checkboxes priorité
            var currentPriority = _settings.GetPriorityExtensionsArray()
                .Select(e => e.ToLowerInvariant())
                .ToHashSet();

            PriorityExtensionItems = new ObservableCollection<PriorityExtensionItem>(
                AvailableExtensions.Select(ext =>
                    new PriorityExtensionItem(ext, currentPriority.Contains(ext)))
            );

            SaveCommand = new RelayCommand(_ => SaveSettings());
            CancelCommand = new RelayCommand(_ => CloseWindow());
            ResetCommand = new RelayCommand(_ => ResetToDefaults());
        }

        private string GetEncryptionExtensionsCsv() =>
            string.Join(",", EncryptionExtensionItems.Where(i => i.IsChecked).Select(i => i.Extension));

        private string GetPriorityExtensionsCsv() =>
            string.Join(",", PriorityExtensionItems.Where(i => i.IsChecked).Select(i => i.Extension));

        private void SaveSettings()
        {
            try
            {
                if (!long.TryParse(MaxParallelFileSizeKo, out long maxSizeKo) || maxSizeKo < 0)
                {
                    MessageBox.Show(Lang["MaxSizeError"], Lang["ValidationError"], MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                _settings.LogDestination = IsExternalStorageSelected ? "Docker"
                         : IsBothStorageSelected ? "Both"
                         : "Local";
                _settings.LogFormat             = SelectedLogFormat;
                _settings.EncryptionExtensions  = GetEncryptionExtensionsCsv();
                _settings.BusinessSoftware      = BusinessSoftware;
                _settings.Language              = SelectedLanguage;
                _settings.MaxParallelFileSizeKo = maxSizeKo;

                _settings.Save();
                LogService.Instance.ReloadFromSettings();
                LanguageManager.Instance.ChangeLanguage(SelectedLanguage);

                CloseWindow();

                MessageBox.Show(Lang["SettingsSaved"], Lang["Success"], MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang["ErrorTitle"]}: {ex.Message}", Lang["ErrorTitle"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaults()
        {
            var result = MessageBox.Show(Lang["ConfirmResetMessage"], Lang["ConfirmReset"], MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _settings.ResetToDefaults();

                SelectedLogFormat = _settings.LogFormat;
                BusinessSoftware = _settings.BusinessSoftware;
                SelectedLanguage = _settings.Language;
                MaxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();

                var defaults = (_settings.EncryptionExtensions ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(e => e.ToLowerInvariant())
                    .ToHashSet();

                foreach (var item in EncryptionExtensionItems)
                    item.IsChecked = defaults.Contains(item.Extension);

                foreach (var item in PriorityExtensionItems)
                    item.IsChecked = false;
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