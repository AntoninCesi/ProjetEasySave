using System.Windows;
using EasySave.ViewModels;

namespace EasySave.GUI
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// Minimal code-behind following MVVM pattern
    /// </summary>
    public partial class ProgressWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of ProgressWindow
        /// </summary>
        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = new ProgressViewModel();
        }

        /// <summary>
        /// Constructor that accepts a ViewModel (for integration with backend)
        /// </summary>
        /// <param name="viewModel">Pre-configured ProgressViewModel</param>
        public ProgressWindow(ProgressViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
