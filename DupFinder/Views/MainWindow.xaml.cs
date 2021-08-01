using DupFinderApp;
using DupFinderApp.ViewModels;
using DupFinderApp.Views;
using Ookii.Dialogs.Wpf;
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

        public MainWindow(MainWindowViewModel mainWindow, Func<OptionsView> optionsViewCreator)
        {
            InitializeComponent();

            DataContext = mainWindow;

            this.mainWindow = mainWindow;

            mainWindow.ShowHelpWindowRequested += (_, _) => OpenWindow(new HelpView());
            mainWindow.ShowOptionsWindowRequested += (_, _) => OpenWindow(optionsViewCreator());
            mainWindow.ShowDirectoryDialogueRequested += MainWindow_ShowDirectoryDialogueRequested;
        }

        private void MainWindow_ShowDirectoryDialogueRequested(object? sender, EventArgs e)
        {
            var folderDialog = new VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() != true) return;

            mainWindow.SelectedPath = folderDialog.SelectedPath;
        }

        private static void OpenWindow<T>(T newInstance) where T : Window
        {
            if (Application.Current.Windows.OfType<T>().Any()) return;

            newInstance.Show();
        }
    }
}
