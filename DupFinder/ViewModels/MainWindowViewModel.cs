using DupFinderCore;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace DupFinderApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        readonly IProcessor _processor;

        public MainWindowViewModel(IProcessor processor)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        private string selectedPath = string.Empty;

        public string SelectedPath
        {
            get { return selectedPath; }
            set
            {
                if (selectedPath != value)
                {
                    selectedPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPath)));
                }
            }
        }

        private int loadedImages;

        public int LoadedImages
        {
            get { return loadedImages; }
            set
            {
                if (loadedImages != value)
                {
                    loadedImages = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadedImages)));
                }
            }
        }

        private int similarImages;

        public int SimilarImages
        {
            get { return similarImages; }
            set
            {
                if (similarImages != value)
                {
                    similarImages = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SimilarImages)));
                }
            }
        }

        public void ChooseDirectory()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() == true)
            {
                SelectedPath = folderDialog.SelectedPath;
            }
        }

        public async void LoadTargets()
        {
            var directory = SelectedPath;

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                MessageBox.Show("Please choose a valid folder."); return;
            }

            var info = new DirectoryInfo(directory);

            LoadedImages = await _processor.AddTargets(info);
        }

        public async void FindSimilarImages()
        {
            SimilarImages = await _processor.Process();
        }

        public void SortImages(object sender, RoutedEventArgs e)
        {
            _processor.Prune();
        }
    }
}
