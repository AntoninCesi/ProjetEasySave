using System.Windows;
using EasySave.ViewModels;

namespace EasySave.GUI
{
   
    /// Interaction logic for ProgressWindow.xaml
    /// Minimal code-behind following MVVM pattern
  
    public partial class ProgressWindow : Window
    {
        /// Constructor that accepts a ViewModel (for integration with backend)
        
        /// <param name="viewModel">Pre-configured ProgressViewModel</param>
        public ProgressWindow(ProgressViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}