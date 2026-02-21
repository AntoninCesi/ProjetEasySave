using System.Windows;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// Code-behind kept minimal following MVVM pattern
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of SettingsWindow
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
