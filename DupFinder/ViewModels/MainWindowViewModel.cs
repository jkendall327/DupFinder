using DupFinderApp.Commands;
using DupFinderCore;
using System;
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
            => _processor = processor ?? throw new ArgumentNullException(nameof(processor));

        #region Properties

        private string selectedPath = string.Empty;

        public string SelectedPath
        {
            get { return selectedPath; }
            set => SetProperty(ref selectedPath, value, nameof(SelectedPath));
        }

        private int loadedImages;

        public int LoadedImages
        {
            get { return loadedImages; }
            set => SetProperty(ref loadedImages, value, nameof(LoadedImages));
        }

        private int similarImages;

        public int SimilarImages
        {
            get { return similarImages; }
            set => SetProperty(ref similarImages, value, nameof(SimilarImages));
        }

        protected void SetProperty<T>(ref T backingField, T value, string propertyName)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(backingField, value)) return;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Commands
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
        #endregion

        public void ChooseDirectory()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() != true) return;


            SelectedPath = folderDialog.SelectedPath;
        }

        public async void LoadImagesIntoMemory()
            => LoadedImages = await _processor.LoadImages(new DirectoryInfo(SelectedPath));

        public async void FindSimilarImages()
            => SimilarImages = await _processor.FindSimilarImages();

        public void SortImages()
            => _processor.FindBetterImages();
    }
}
