using DupFinderCore;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DupFinderApp.ViewModels
{
    public class MainWindowViewModel : VMBase
    {
        private readonly Processor _processor;

        public event EventHandler? ShowHelpWindowRequested;
        public event EventHandler? ShowOptionsWindowRequested;
        public event EventHandler? ShowDirectoryDialogueRequested;

        /// <summary>
        /// A sink for the <see cref="ILogger"/> that the UI binds to.
        /// </summary>
        public ObservableCollection<string> Logger { get; set; }

        public ICommand ChooseDirectoryCommand { get; }
        public ICommand LoadImagesCommand { get; }
        public ICommand FindSimilarImagesCommand { get; }
        public ICommand MoveImagesCommand { get; }
        public ICommand ShowOptionsCommand { get; }
        public ICommand ShowHelpCommand { get; }

        public MainWindowViewModel(Processor processor, ObservableCollection<string> _log)
        {
            _processor = processor;
            Logger = _log;

            ChooseDirectoryCommand = new RelayCommand(() => InvokeAndUpdate(ShowDirectoryDialogueRequested!));
            ShowOptionsCommand = new RelayCommand(() => InvokeAndUpdate(ShowOptionsWindowRequested!));
            ShowHelpCommand = new RelayCommand(() => InvokeAndUpdate(ShowHelpWindowRequested!));

            LoadImagesCommand = new AsyncRelayCommand(LoadImages, () => Directory.Exists(SelectedPath));
            FindSimilarImagesCommand = new AsyncRelayCommand(FindSimilarImages, () => LoadedImages > 0);
            MoveImagesCommand = new RelayCommand(MoveImages, () => SimilarImages > 0);

            Commands = new(6)
            {
                ChooseDirectoryCommand,
                ShowOptionsCommand,
                ShowHelpCommand,
                LoadImagesCommand,
                FindSimilarImagesCommand,
                MoveImagesCommand
            };
        }

        private readonly List<ICommand> Commands = new();

        private void UpdateAllCommands()
        {
            foreach (ICommand? command in Commands)
            {
                if (command is not RelayCommand r)
                {
                    continue;
                }

                r.NotifyCanExecuteChanged();
            }
        }

        private DirectoryInfo BaseFolder => new DirectoryInfo(SelectedPath);

        private async Task LoadImages()
        {
            await _processor.LoadImages(BaseFolder);
            LoadedImages = _processor.Targets.Count;

            UpdateAllCommands();
        }

        private async Task FindSimilarImages()
        {
            await _processor.FindSimilarImages(_processor.Targets);
            SimilarImages = _processor.Pairs.Count;

            UpdateAllCommands();
        }

        private void MoveImages()
        {
            _processor.CompareImages(_processor.Pairs);
            _processor.MoveImages(BaseFolder);

            MovedImages = _processor.MovedImageCount;

            UpdateAllCommands();
        }

        private void InvokeAndUpdate(EventHandler e)
        {
            e?.Invoke(this, EventArgs.Empty);

            UpdateAllCommands();
        }

        private string selectedPath;
        public string SelectedPath
        {
            get => selectedPath;
            set => SetProperty(ref selectedPath, value);
        }

        private int similarImages;
        public int SimilarImages
        {
            get => similarImages;
            set => SetProperty(ref similarImages, value);
        }

        private int loadedImages;
        public int LoadedImages
        {
            get => loadedImages;
            set => SetProperty(ref loadedImages, value);
        }

        private int movedImages;
        public int MovedImages
        {
            get => movedImages;
            set => SetProperty(ref movedImages, value);
        }
    }
}
