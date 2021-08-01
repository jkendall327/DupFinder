using DupFinderApp;
using DupFinderApp.ViewModels;
using DupFinderApp.Views;
using System;
using System.Linq;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel mainWindow;
        private readonly OptionsViewModel optionsViewModel;

        public MainWindow(MainWindowViewModel mainWindow, OptionsViewModel optionsViewModel)
        {
            InitializeComponent();

            DataContext = mainWindow;

            mainWindow.ShowHelpWindowRequested += MainWindow_ShowHelpWindowRequested;
            mainWindow.ShowOptionsWindowRequested += MainWindow_ShowOptionsWindowRequested;
            mainWindow.ShowDirectoryDialogueRequested += MainWindow_ShowDirectoryDialogueRequested;
            this.mainWindow = mainWindow;
            this.optionsViewModel = optionsViewModel;
        }

        private void MainWindow_ShowDirectoryDialogueRequested(object? sender, EventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() != true) return;

            mainWindow.SelectedPath = folderDialog.SelectedPath;
        }

        private void MainWindow_ShowOptionsWindowRequested(object? sender, EventArgs e)
        {
            // don't open multiple windows
            if (Application.Current.Windows.OfType<OptionsView>().Any())
                return;

            // keeping the same viewmodel means user settings are preserved
            var window = new OptionsView(optionsViewModel)
            {
                DataContext = optionsViewModel
            };

            window.Show();
        }

        private void MainWindow_ShowHelpWindowRequested(object? sender, EventArgs e)
        {
            // don't open multiple windows
            if (Application.Current.Windows.OfType<HelpView>().Any())
                return;

            // keeping the same viewmodel means user settings are preserved
            var window = new HelpView()
            {
                DataContext = new HelpViewModel()
            };

            window.Show();
        }
    }
}
