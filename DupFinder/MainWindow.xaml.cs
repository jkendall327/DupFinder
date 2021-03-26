using DupFinderCore;
using System.IO;
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
            if (!Directory.Exists(chosenFolder.Content.ToString()))
            {
                MessageBox.Show("Please choose a valid folder.");
                return;
            }

            var info = new DirectoryInfo(chosenFolder.Content?.ToString());

            label1.Content = await _processor.AddTargets(info);
        }
    }
}
