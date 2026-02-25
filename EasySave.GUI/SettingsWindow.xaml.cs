using System.Windows;
using EasySave.Resources;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
            UpdateTexts();
            LanguageManager.Instance.PropertyChanged += (s, e) => UpdateTexts();
        }

        private void UpdateTexts()
        {
            var lang = LanguageManager.Instance;
            TxtSettingsTitle.Text    = lang["SettingsTitle"];
            TxtSettingsSubtitle.Text = lang["SettingsSubtitle"];
            TxtLanguageLabel.Text    = lang["LanguageLabel"];
            TxtLanguageHint.Text     = lang["LanguageHint"];
            TxtLogFormatLabel.Text   = lang["LogFormatLabel"];
            TxtLogFormatHint.Text    = lang["LogFormatHint"];
            TxtEncryptionLabel.Text  = lang["EncryptionLabel"];
            TxtEncryptionHint.Text   = lang["EncryptionHint"];
            TxtLogsStorageLabel.Text = lang["LogsStorageLabel"];
            TxtLogsStorageHint.Text  = lang["LogsStorageHint"];
            RadioLocal.Content       = lang["LocalStorage"];
            RadioExternal.Content    = lang["ExternalStorage"];
            TxtBizLabel.Text         = lang["BusinessSoftwareLabel"];
            TxtBizHint.Text          = lang["BusinessSoftwareHint"];
            TxtPriorityLabel.Text    = lang["PriorityExtensionsLabel"];
            TxtPriorityHint.Text     = lang["PriorityExtensionsHint"];
            TxtMaxSizeLabel.Text     = lang["MaxFileSizeLabel"];
            TxtMaxSizeHint.Text      = lang["MaxFileSizeHint"];
            BtnReset.Content         = lang["ResetToDefaults"];
            BtnCancel.Content        = lang["Cancel"];
            BtnSave.Content          = lang["Save"];
        }
    }
}
