using DupFinderApp.ViewModels;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWindowViewModel _mainWindow;
        public MainWindow(MainWindowViewModel mainWindow)
        {
            _mainWindow = mainWindow;

            InitializeComponent();

            DataContext = _mainWindow;
        }
    }
}
