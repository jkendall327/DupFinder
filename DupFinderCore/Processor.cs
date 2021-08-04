using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using DupFinderCore.Services;
using Serilog;

namespace DupFinderCore
{
    public class Processor
    {
        private readonly ImageSetLoader _loader;
        private readonly ILogger _logger;
        private readonly IImageComparer _comparer;
        private readonly PairFinder _finder;

        public ConcurrentBag<IEntry> Targets { get; private set; } = new();
        public ConcurrentBag<Pair> Pairs { get; private set; } = new();

        public Processor(ImageSetLoader loader, ILogger logger, IImageComparer comparer, PairFinder finder)
        {
            _logger = logger;

            _loader = loader;
            _comparer = comparer;
            _finder = finder;
        }

        public async Task LoadImages(DirectoryInfo baseFolder)
        {
            Targets = await _loader.LoadImages(baseFolder);
        }

        public async Task FindSimilarImages(IEnumerable<IEntry> targets)
        {
            Pairs = await _finder.FindPairs(targets);
        }

        public void CompareImages(IEnumerable<Pair> pairs)
        {
            _comparer.Compare(pairs);
        }

        public void MoveImages(DirectoryInfo baseFolder, bool overwriteExistingFiles = true)
        {
            string basePath = baseFolder?.FullName + Path.DirectorySeparatorChar;

            var collectionsAndPaths = new List<(List<IEntry>, string)>()
            {
                (_comparer.Keep, "Keep"),
                (_comparer.Trash, "Trash"),
                (_comparer.Unsure, "Unsure")
            };

            foreach ((List<IEntry> collection, string path) pair in collectionsAndPaths)
            {
                DirectoryInfo destination = Directory.CreateDirectory(basePath + pair.path);
                Move(pair.collection, destination, overwriteExistingFiles);
            }
        }

        public int MovedImageCount => _comparer.Keep.Count + _comparer.Trash.Count + _comparer.Unsure.Count;

        private void Move(IEnumerable<IEntry> images, DirectoryInfo destination, bool overwriteExistingFiles)
        {
            foreach (var image in images)
            {
                if (!File.Exists(image.FullPath))
                {
                    _logger.Warning($"Image {image.FullPath} not found.");
                    continue;
                }

                string destinationPath = destination.FullName + Path.DirectorySeparatorChar + image.Filename;

                File.Move(image.FullPath, destinationPath, overwriteExistingFiles);
            }
        }
    }
}
