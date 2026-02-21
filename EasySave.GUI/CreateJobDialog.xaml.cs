using System.Windows;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    /// <summary>
    /// Interaction logic for CreateJobDialog.xaml
    /// Minimal code-behind following MVVM pattern
    /// </summary>
    public partial class CreateJobDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of CreateJobDialog
        /// </summary>
        public CreateJobDialog()
        {
            InitializeComponent();
            DataContext = new CreateJobViewModel();
        }
    }
}
