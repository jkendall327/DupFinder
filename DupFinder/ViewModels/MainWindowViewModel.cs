﻿using DupFinderApp.Commands;
using DupFinderCore;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DupFinderApp.ViewModels
{
    public class MainWindowViewModel : VMBase
    {
        private readonly Processor _processor;

        private OptionsViewModel _optionsViewModel;
        public OptionsViewModel OptionsWindow { get => _optionsViewModel; set => SetProperty(ref _optionsViewModel, value); }

        /// <summary>
        /// A sink for the <see cref="ILogger"/> that the UI binds to.
        /// </summary>
        public ObservableCollection<string> Logger { get; set; }

        public MainWindowViewModel(Processor processor, OptionsViewModel optionsViewModel, ObservableCollection<string> _log)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));

            _optionsViewModel = optionsViewModel ?? throw new ArgumentNullException(nameof(optionsViewModel));

            Logger = _log ?? throw new ArgumentNullException(nameof(_log));

            // wire up commands
            ChooseDirectory = new CommandHandler(OpenDirectoryDialogue);
            ShowOptions = new CommandHandler(ShowOptionsWindow);

            var ImageLoadProgress = new Progress<PercentageProgress>();
            ImageLoadProgress.ProgressChanged += ImageLoadProgress_ProgressChanged;

            LoadImages = new CommandHandler(
                async () => LoadedImages = await _processor.LoadImages(new DirectoryInfo(SelectedPath), ImageLoadProgress),
                () => Directory.Exists(SelectedPath));

            var SimilarImageProgress = new Progress<PercentageProgress>();
            SimilarImageProgress.ProgressChanged += SimilarImageProgress_ProgressChanged;

            FindSimilarImages = new CommandHandler(
                async () => SimilarImages = await _processor.FindSimilarImages(SimilarImageProgress),
                () => LoadedImages > 0);

            MoveImages = new CommandHandler(
                () => _processor.FindBetterImages(),
                () => SimilarImages > 0);
        }

        private string selectedPath = string.Empty;
        public string SelectedPath
        { get => selectedPath; set => SetProperty(ref selectedPath, value); }

        #region Finding Similar Images

        private int similarImages;
        public int SimilarImages
        { get => similarImages; set => SetProperty(ref similarImages, value); }

        private int similarImagesPercentage;
        public int SimilarImagesPercentage
        { get => similarImagesPercentage; set => SetProperty(ref similarImagesPercentage, value); }

        private void SimilarImageProgress_ProgressChanged(object? sender, PercentageProgress e)
        {
            SimilarImagesPercentage = e.PercentageDone;
        }

        #endregion

        #region Loading Images

        private int loadedImages;
        public int LoadedImages
        { get => loadedImages; set => SetProperty(ref loadedImages, value); }

        private int loadedImagesPercentage;
        public int LoadedImagesPercentage
        { get => loadedImagesPercentage; set => SetProperty(ref loadedImagesPercentage, value); }

        private void ImageLoadProgress_ProgressChanged(object? sender, PercentageProgress e)
        {
            LoadedImagesPercentage = e.PercentageDone;
            LoadedImages = e.AmountDone;
        }

        #endregion

        private int movedImages;
        public int MovedImages
        { get => movedImages; set => SetProperty(ref movedImages, value); }

        #region Commands

        public ICommand ChooseDirectory { get; }
        public ICommand LoadImages { get; }
        public ICommand FindSimilarImages { get; }
        public ICommand MoveImages { get; }
        public ICommand ShowOptions { get; }

        #endregion

        private void ShowOptionsWindow()
        {
            // don't open multiple windows
            if (Application.Current.Windows.OfType<OptionsView>().Any())
                return;

            // keeping the same viewmodel means user settings are preserved
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
