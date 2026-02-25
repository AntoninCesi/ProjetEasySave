using System.Windows;
using EasySave.Resources;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow(ProgressViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            UpdateTexts();
            LanguageManager.Instance.PropertyChanged += (s, e) => UpdateTexts();
        }

        private void UpdateTexts()
        {
            var lang = LanguageManager.Instance;
            TxtBackupTitle.Text      = lang["BackupInProgressTitle"];
            TxtFilesLabel.Text       = lang["FilesLabel"];
            TxtDataLabel.Text        = lang["DataLabel"];
            TxtSpeedLabel.Text       = lang["SpeedLabel"];
            TxtRemainingLabel.Text   = lang["RemainingLabel"];
            TxtCurrentFileLabel.Text = lang["CurrentFileLabel"];
            BtnPause.Content         = lang["Pause"];
            BtnResume.Content        = lang["Resume"];
            BtnStop.Content          = lang["Stop"];
        }

        private void Button_Click(object sender, RoutedEventArgs e) { }
    }
}
