using DupFinderCore.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class Processor
    {
        private readonly ImageSetLoader _loader;
        private readonly ILogger _logger;
        private readonly IImageComparer _comparer;
        private readonly IConfiguration _config;
        private readonly PairFinder _finder;

        public ConcurrentBag<IEntry> Targets { get; private set; } = new();
        public ConcurrentBag<Pair> Pairs { get; private set; } = new();

        public Processor(ImageSetLoader loader, ILogger logger, IImageComparer comparer, IConfiguration config, PairFinder finder)
        {
            _logger = logger;

            _loader = loader;
            _comparer = comparer;
            _config = config;
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

        public void MoveImages(DirectoryInfo baseFolder)
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
                Move(pair.collection, destination);
            }
        }

        public int MovedImageCount => _comparer.Keep.Count + _comparer.Trash.Count + _comparer.Unsure.Count;

        private void Move(IEnumerable<IEntry> images, DirectoryInfo destination)
        {
            foreach (var image in images)
            {
                if (!File.Exists(image.FullPath))
                {
                    _logger.Warning($"Image {image.FullPath} not found.");
                    continue;
                }

                bool overrideFiles = _config.GetValue<bool>("OverwriteExistingFiles");
                string destinationPath = destination.FullName + Path.DirectorySeparatorChar + image.Filename;

                File.Move(image.FullPath, destinationPath, overrideFiles);
            }
        }
    }
}
