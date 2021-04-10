using DupFinderApp.Commands;
using DupFinderCore;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DupFinderApp.ViewModels
{
    public class MainWindowViewModel : VMBase, INotifyCollectionChanged
    {
        private readonly IProcessor _processor;
        private readonly ILogger _logger;

        private OptionsViewModel _optionsViewModel;
        public OptionsViewModel OptionsWindow { get => _optionsViewModel; set => SetProperty(ref _optionsViewModel, value); }

        private readonly ConcurrentQueue<string> uiLog = new();
        public ConcurrentQueue<string> UiLog
        {
            get { return uiLog; }
            set { CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(; }
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public ObservableCollection<string> people = new();
        public ObservableCollection<string> People { get { return people; } }

        public MainWindowViewModel(IProcessor processor, OptionsViewModel optionsViewModel, ConcurrentQueue<string> uiLog, ILogger logger)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _optionsViewModel = optionsViewModel ?? throw new ArgumentNullException(nameof(optionsViewModel));

            UiLog = uiLog;

            ChooseDirectory = new CommandHandler(OpenDirectoryDialogue);
            ShowOptions = new CommandHandler(ShowOptionsWindow);

            LoadImages = new CommandHandler(
                async () => LoadedImages = await _processor.LoadImages(new DirectoryInfo(SelectedPath)),
                () => Directory.Exists(SelectedPath));

            FindSimilarImages = new CommandHandler(
                async () => SimilarImages = await _processor.FindSimilarImages(),
                () => LoadedImages > 0);

            MoveImages = new CommandHandler(
                () => _processor.FindBetterImages(),
                () => SimilarImages > 0);
            _logger = logger;
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
            _logger.Information("Chose directory...");
        }
    }
}
