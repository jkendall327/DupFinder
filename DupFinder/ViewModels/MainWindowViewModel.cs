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
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));

            ChooseDirectory = new CommandHandler(OpenDirectoryDialogue, () => true);

            LoadImages = new CommandHandler(
                async () => LoadedImages = await _processor.LoadImages(new DirectoryInfo(SelectedPath)),
                () => !string.IsNullOrWhiteSpace(SelectedPath) || Directory.Exists(SelectedPath));

            FindSimilarImages = new CommandHandler(
                async () => SimilarImages = await _processor.FindSimilarImages(),
                () => LoadedImages > 0);

            MoveImages = new CommandHandler(
                () => _processor.FindBetterImages(OptionsWindow.GetSettings()),
                () => SimilarImages > 0);

            ShowOptions = new CommandHandler(ShowOptionsWindow, () => true);
        }

        private string selectedPath = string.Empty;
        public string SelectedPath
        { get => selectedPath; set => SetProperty(ref selectedPath, value); }

        private int loadedImages;
        public int LoadedImages
        { get => loadedImages; set => SetProperty(ref loadedImages, value); }

        private int similarImages;
        public int SimilarImages
        { get => similarImages; set => SetProperty(ref similarImages, value); }

        private OptionsViewModel optionsWindow = new OptionsViewModel();
        public OptionsViewModel OptionsWindow { get => optionsWindow; set => SetProperty(ref optionsWindow, value); }

        public ICommand ChooseDirectory { get; }
        public ICommand LoadImages { get; }
        public ICommand FindSimilarImages { get; }
        public ICommand MoveImages { get; }
        public ICommand ShowOptions { get; }

        private void ShowOptionsWindow()
        {
            var window = new OptionsView
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
