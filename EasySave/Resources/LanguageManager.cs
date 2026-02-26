using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace EasySave.Resources
{
    /// <summary>
    /// Manages language switching for the application using resource files.
    /// Implements INotifyPropertyChanged to update UI when language changes.
    /// </summary>
    public class LanguageManager : INotifyPropertyChanged
    {
        private static LanguageManager? _instance;
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public static LanguageManager Instance => _instance ??= new LanguageManager();

        public event PropertyChangedEventHandler? PropertyChanged;

        private LanguageManager()
        {
            // Initialize with the resource manager pointing to your Strings.resx file
            _resourceManager = new ResourceManager("EasySave.Resources.Strings", typeof(LanguageManager).Assembly);

            // Start with ENGLISH by default
            _currentCulture = new CultureInfo("en-US");
        }

        /// <summary>
        /// Indexer to get localized strings by key
        /// </summary>
        public string this[string key]
        {
            get
            {
                string? value = _resourceManager.GetString(key, _currentCulture);
                return value ?? $"[Missing: {key}]";
            }
        }

        /// <summary>
        /// Changes the application language and notifies all bindings
        /// </summary>
        public void ChangeLanguage(string cultureCode)
        {
            _currentCulture = new CultureInfo(cultureCode);

            // Notify all bindings that use the indexer
            // This tells WPF to re-evaluate ALL Lang[xxx] bindings
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

            // Also change the thread culture for formatting
            CultureInfo.CurrentUICulture = _currentCulture;
            CultureInfo.CurrentCulture = _currentCulture;
        }

        public CultureInfo CurrentCulture => _currentCulture;

        public string CurrentLanguageCode => _currentCulture.Name;
    }
}