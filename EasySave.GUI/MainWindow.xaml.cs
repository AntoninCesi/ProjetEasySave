using System.Windows;
using EasySave.Resources;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            LanguageManager.Instance.ChangeLanguage("en-US");
            UpdateTexts();

            this.Activated += (s, e) => UpdateTexts();
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

        private void UpdateTexts()
        {
            var lang = LanguageManager.Instance;
            TxtTitle.Text             = lang["AppTitle"];
            TxtSubtitle.Text          = lang["AppSubtitle"];
            TxtLanguage.Text          = lang["Language"];
            BtnSettings.Content       = lang["Settings"];
            BtnDeleteJob.Content      = lang["DeleteJob"];
            BtnCreateJob.Content      = lang["CreateNewJob"];
            BtnRunSelection.Content   = lang["RunSelected"]; 
            ColSelectHeader.Text      = lang["Select"];        
            ColJobName.Header         = lang["JobName"];
            ColSourcePath.Header      = lang["SourcePath"];
            ColDestinationPath.Header = lang["DestinationPath"];
            ColType.Header            = lang["Type"];
            ColState.Header           = lang["State"];
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }
    }
}
