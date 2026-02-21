using System.Windows;
using EasySave.Resources;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Handles language updates and UI refresh
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // Set English by default
            LanguageManager.Instance.ChangeLanguage("en-US");
            UpdateTexts();
            
            // Subscribe to window activation to refresh texts when Settings closes
            this.Activated += MainWindow_Activated;
        }

        /// <summary>
        /// Event handler called when window is activated (gains focus)
        /// Refreshes all texts in case language was changed in Settings
        /// </summary>
        private void MainWindow_Activated(object? sender, System.EventArgs e)
        {
            UpdateTexts();
        }

        private void BtnFR_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.Instance.ChangeLanguage("fr-FR");
            UpdateTexts();
        }

        private void BtnEN_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.Instance.ChangeLanguage("en-US");
            UpdateTexts();
        }

        /// <summary>
        /// Updates all UI text elements with current language
        /// </summary>
        private void UpdateTexts()
        {
            // Update title and subtitle
            TxtTitle.Text = LanguageManager.Instance["AppTitle"];
            TxtSubtitle.Text = LanguageManager.Instance["AppSubtitle"];
            TxtLanguage.Text = LanguageManager.Instance["Language"];
            
            // Update buttons
            BtnSettings.Content = LanguageManager.Instance["Settings"];
            BtnCreateJob.Content = LanguageManager.Instance["CreateNewJob"];
            BtnRunSelection.Content = LanguageManager.Instance["RunSelection"];
            
            // Update DataGrid headers
            ColJobName.Header = LanguageManager.Instance["JobName"];
            ColSourcePath.Header = LanguageManager.Instance["SourcePath"];
            ColDestinationPath.Header = LanguageManager.Instance["DestinationPath"];
            ColType.Header = LanguageManager.Instance["Type"];
            ColState.Header = LanguageManager.Instance["State"];
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
