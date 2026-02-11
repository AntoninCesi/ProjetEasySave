using EasySave.Resources;
using EasySave.ViewModels;
using Microsoft.VisualBasic;
using System.Windows;

namespace EasySave.View
{
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
        }
    }
}