using DupFinderApp.Commands;
using DupFinderCore;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DupFinderApp.ViewModels
{
    public class MainWindowViewModel : VMBase
    {
        private readonly IProcessor _processor;
        private OptionsViewModel _optionsViewModel;
        public OptionsViewModel OptionsWindow { get => _optionsViewModel; set => SetProperty(ref _optionsViewModel, value); }

        public MainWindowViewModel(IProcessor processor, OptionsViewModel optionsViewModel)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _optionsViewModel = optionsViewModel ?? throw new ArgumentNullException(nameof(optionsViewModel));

            ChooseDirectory = new CommandHandler(OpenDirectoryDialogue);
            ShowOptions = new CommandHandler(ShowOptionsWindow);

            ImageLoadProgress = new Progress<ImagesLoadedProgress>();
            ImageLoadProgress.ProgressChanged += ImageLoadProgress_ProgressChanged;

            LoadImages = new CommandHandler(
                async () => LoadedImages = await _processor.LoadImages(new DirectoryInfo(SelectedPath), ImageLoadProgress),
                () => Directory.Exists(SelectedPath));

            FindSimilarImages = new CommandHandler(
                async () => SimilarImages = await _processor.FindSimilarImages(),
                () => LoadedImages > 0);

            MoveImages = new CommandHandler(
                () => _processor.FindBetterImages(),
                () => SimilarImages > 0);
        }

        private void ImageLoadProgress_ProgressChanged(object? sender, ImagesLoadedProgress e)
        {
            LoadedImagesPercentage = e.PercentageDone;
        }

        private string selectedPath = string.Empty;
        public string SelectedPath
        { get => selectedPath; set => SetProperty(ref selectedPath, value); }

        private int loadedImages;
        public int LoadedImages
        { get => loadedImages; set => SetProperty(ref loadedImages, value); }

        private int loadedImagesPercentage;
        public int LoadedImagesPercentage
        { get => loadedImagesPercentage; set => SetProperty(ref loadedImagesPercentage, value); }

        readonly Progress<ImagesLoadedProgress> ImageLoadProgress;

        private int similarImages;
        public int SimilarImages
        { get => similarImages; set => SetProperty(ref similarImages, value); }

        public ICommand ChooseDirectory { get; }
        public ICommand LoadImages { get; }
        public ICommand FindSimilarImages { get; }
        public ICommand MoveImages { get; }
        public ICommand ShowOptions { get; }

        private void ShowOptionsWindow()
        {
            if (Application.Current.Windows.OfType<OptionsView>().Any())
                return;

            var window = new OptionsView(_optionsViewModel)
            {
                DataContext = OptionsWindow
            };

            window.Show();
        }

        private void OpenDirectoryDialogue()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() != true) return;

            SelectedPath = folderDialog.SelectedPath;
        }
    }
}
