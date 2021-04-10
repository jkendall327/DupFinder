using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class Processor
    {
        readonly ImageSetLoader _loader;
        readonly ILogger _logger;
        readonly IImageComparer _comparer;
        readonly IConfiguration _config;
        readonly PairFinder _finder;

        DirectoryInfo? BaseFolder;

        public IEnumerable<IEntry> Targets { get; set; } = Enumerable.Empty<Entry>();

        public List<(IEntry Left, IEntry Right)> Pairs { get; set; } = new();

        public Processor(ImageSetLoader loader, ILogger logger, IImageComparer comparer, IConfiguration config, PairFinder finder)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _finder = finder ?? throw new ArgumentNullException(nameof(finder));
        }

        public async Task<int> LoadImages(DirectoryInfo baseFolder)
        {
            BaseFolder = baseFolder;
            Targets = await _loader.LoadImages(BaseFolder);
            _logger.Information("Images loaded.");

            return Targets.Count();
        }


        public async Task<int> LoadImages(DirectoryInfo baseFolder, IProgress<PercentageProgress> imageLoadProgress)
        {
            BaseFolder = baseFolder;
            Targets = await _loader.LoadImages(BaseFolder, imageLoadProgress);
            _logger.Information("Images loaded.");

            return Targets.Count();
        }

        public async Task<int> FindSimilarImages()
        {
            var result = await _finder.FindPairs(Targets);
            Pairs = result.ToList();

            return Pairs.Count();
        }

        public async Task<int> FindSimilarImages(IProgress<PercentageProgress> progress)
        {
            var result = await _finder.FindPairs(Targets, progress);
            Pairs = result.ToList();
            return Pairs.Count();
        }

        public void FindBetterImages()
        {
            _comparer.Compare(Pairs);
            string path = BaseFolder?.FullName + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(path + "Keep");
            Directory.CreateDirectory(path + "Trash");
            Directory.CreateDirectory(path + "Unsure");

            Move(_comparer.Keep, new DirectoryInfo(path + "Keep"));
            Move(_comparer.Trash, new DirectoryInfo(path + "Trash"));
            Move(_comparer.Unsure, new DirectoryInfo(path + "Unsure"));
        }

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
