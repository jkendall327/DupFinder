using DupFinderApp.ViewModels;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel mainWindow)
        {
            InitializeComponent();
            DataContext = mainWindow;
        }
    }
}
