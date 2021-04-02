using DupFinderApp.Commands;
using DupFinderCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

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

        // todo: try to get this working
        private T Update<T>(T original, T newValue, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(original, newValue))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return newValue;
            }

            return original;
        }

        private ICommand? _chooseDirectoryCommand;
        public ICommand ChooseDirectoryCommand =>
            _chooseDirectoryCommand ??= new CommandHandler(() => ChooseDirectory(), () => true);

        private ICommand? _loadImagesIntoMemoryCommand;
        public ICommand LoadImagesIntoMemoryCommand =>
            _loadImagesIntoMemoryCommand ??= new CommandHandler(() => LoadImagesIntoMemory(), () => !string.IsNullOrWhiteSpace(SelectedPath) || Directory.Exists(SelectedPath));

        private ICommand? _findSimilarImagesCommand;
        public ICommand FindSimilarImagesCommand =>
            _findSimilarImagesCommand ??= new CommandHandler(() => FindSimilarImages(), () => LoadedImages > 0);

        private ICommand? _moveImagesCommand;
        public ICommand MoveImagesCommand =>
            _moveImagesCommand ??= new CommandHandler(() => SortImages(), () => SimilarImages > 0);


        public void ChooseDirectory()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() == true)
            {
                SelectedPath = folderDialog.SelectedPath;
            }
        }

        public async void LoadImagesIntoMemory()
            => LoadedImages = await _processor.AddTargets(new DirectoryInfo(SelectedPath));

        public async void FindSimilarImages()
            => SimilarImages = await _processor.Process();

        public void SortImages()
            => _processor.Prune();
    }
}
