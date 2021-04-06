using DupFinderApp.Commands;
using DupFinderCore;
using System;
using System.IO;
using System.Windows.Input;

namespace DupFinderApp.ViewModels
{
    public class MainWindowViewModel : VMBase
    {
        private readonly IProcessor _processor;

        public MainWindowViewModel(IProcessor processor)
            => _processor = processor ?? throw new ArgumentNullException(nameof(processor));

        private string selectedPath = string.Empty;
        public string SelectedPath { get => selectedPath; set => SetProperty(ref selectedPath, value); }

        private int loadedImages;
        public int LoadedImages { get => loadedImages; set => SetProperty(ref loadedImages, value); }

        private int similarImages;
        public int SimilarImages { get => similarImages; set => SetProperty(ref similarImages, value); }

        private ICommand? _chooseDirectoryCommand;
        public ICommand ChooseDirectoryCommand =>
            _chooseDirectoryCommand ??= new CommandHandler(
                () => ChooseDirectory(),
                () => true);

        private ICommand? _loadImagesIntoMemoryCommand;
        public ICommand LoadImagesIntoMemoryCommand =>
            _loadImagesIntoMemoryCommand ??= new CommandHandler(
                async () => LoadedImages = await _processor.LoadImages(new DirectoryInfo(SelectedPath)),
                () => !string.IsNullOrWhiteSpace(SelectedPath) || Directory.Exists(SelectedPath));

        private ICommand? _findSimilarImagesCommand;
        public ICommand FindSimilarImagesCommand =>
            _findSimilarImagesCommand ??= new CommandHandler(
                async () => SimilarImages = await _processor.FindSimilarImages(),
                () => LoadedImages > 0);

        private ICommand? _moveImagesCommand;
        public ICommand MoveImagesCommand =>
            _moveImagesCommand ??= new CommandHandler(
                () => _processor.FindBetterImages(OptionsWindow.GetSettings()),
                () => SimilarImages > 0);


        private ICommand? showOptions;
        public ICommand ShowOptionsCommand =>
            showOptions ??= new CommandHandler(() => ShowOptionsWindow(), () => true);

        private void ShowOptionsWindow()
        {
            var window = new OptionsView
            {
                DataContext = OptionsWindow
            };

            window.Show();
        }

        private OptionsViewModel optionsWindow = new OptionsViewModel();
        public OptionsViewModel OptionsWindow { get => optionsWindow; set => SetProperty(ref optionsWindow, value); }

        private void ChooseDirectory()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (folderDialog.ShowDialog() != true) return;

            SelectedPath = folderDialog.SelectedPath;
        }
    }
}
