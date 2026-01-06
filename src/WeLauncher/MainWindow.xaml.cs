using System.Windows;
using WeLauncher.ViewModels;

namespace WeLauncher
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

