using System.Windows;
using EasySave.WPF.ViewModels;

namespace EasySave.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Assign the ViewModel as the data context
            DataContext = new MainViewModel();
        }
    }
}