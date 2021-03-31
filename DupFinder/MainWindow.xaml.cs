using BenchmarkDotNet.Attributes;
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

        private void DirectoryPicker_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() == true)
            {
                chosenFolder.Content = folderDialog.SelectedPath;
            }
        }

        [Benchmark]
        private async void TargetAdder_Click(object sender, RoutedEventArgs e)
        {
            var directory = chosenFolder.Content.ToString();

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                MessageBox.Show("Please choose a valid folder.");
                return;
            }

            var info = new DirectoryInfo(directory);
            // 'Possible null reference for argument 'path' in 'DirectoryInfo.DirectoryInfo(string path)'

            label_ChosenFolder.Content = await _processor.AddTargets(info);
        }

        private async void button_ProcessImages_Click(object sender, RoutedEventArgs e)
        {
            label_processedImages.Content = await _processor.Process();
        }

        private void button_MoveImages_Click(object sender, RoutedEventArgs e)
        {
            _processor.Prune();
        }
    }
}
