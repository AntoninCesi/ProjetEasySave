using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EasySave.Commands;
using EasySave.Models;
using EasySave.Resources;
using EasySave.Services;

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
		private string _priorityExtensions;        // CSV string for TextBox
		private string _maxParallelFileSizeKo;     // string for TextBox binding

		public event PropertyChangedEventHandler? PropertyChanged;

		public ObservableCollection<string> LogFormats { get; }
		public ObservableCollection<string> Languages { get; }

		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand ResetCommand { get; }

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

		public SettingsViewModel()
		{
			_settings = AppSettings.Instance;

			LogFormats = new ObservableCollection<string> { "JSON", "XML" };
			Languages = new ObservableCollection<string> { "en-US", "fr-FR" };

			// Load current settings
			_selectedLogFormat = _settings.LogFormat;
			_encryptionExtensions = _settings.EncryptionExtensions;
			_businessSoftware = _settings.BusinessSoftware;
			_selectedLanguage = _settings.Language;

			// IMPORTANT: AppSettings.PriorityExtensions est une List<string> -> on passe par le bridge CSV
			_priorityExtensions = _settings.PriorityExtensionsCsv;

			_maxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();

			SaveCommand = new RelayCommand(_ => SaveSettings());
			CancelCommand = new RelayCommand(_ => CloseWindow());
			ResetCommand = new RelayCommand(_ => ResetToDefaults());
		}

		private void SaveSettings()
		{
			try
			{
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

				_settings.LogFormat = SelectedLogFormat;
				_settings.EncryptionExtensions = EncryptionExtensions;
				_settings.BusinessSoftware = BusinessSoftware;
				_settings.Language = SelectedLanguage;

				// IMPORTANT: écrire via CSV -> remplit la List<string> en interne
				_settings.PriorityExtensionsCsv = PriorityExtensions;

				_settings.MaxParallelFileSizeKo = maxSizeKo;

				_settings.Save();

				// Recharge le logger selon le nouveau LogFormat (n'impacte pas les features "pause")
				LogService.Instance.ReloadFromSettings();

				// Applique la langue
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

		private void ResetToDefaults()
		{
			var result = MessageBox.Show(
				"Are you sure you want to reset all settings to default values?",
				"Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				_settings.ResetToDefaults();

				SelectedLogFormat = _settings.LogFormat;
				EncryptionExtensions = _settings.EncryptionExtensions;
				BusinessSoftware = _settings.BusinessSoftware;
				SelectedLanguage = _settings.Language;

				// relire via CSV
				PriorityExtensions = _settings.PriorityExtensionsCsv;

				MaxParallelFileSizeKo = _settings.MaxParallelFileSizeKo.ToString();
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

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}