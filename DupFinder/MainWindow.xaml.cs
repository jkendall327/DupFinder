using DupFinderCore;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IProcessor _processor;

        public MainWindow(IProcessor processor)
        {
            _processor = processor;

            _processor.AddTargets();

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() == true)
            {
                chosenFolder.Content = folderDialog.SelectedPath;
            }
        }

        private async void button_Click_1(object sender, RoutedEventArgs e)
        {
            label1.Content = await _processor.AddTargets();
        }
    }
}
