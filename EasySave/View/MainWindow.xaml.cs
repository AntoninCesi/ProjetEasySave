using System.Windows;
using EasySave.ViewModels;

namespace EasySave.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}