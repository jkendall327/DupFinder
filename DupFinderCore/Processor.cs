using Microsoft.Extensions.Configuration;
using Serilog;
using Shipwreck.Phash;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class Processor : IProcessor
    {
        readonly IImageSetLoader _loader;
        readonly ILogger _logger;
        readonly IImagerComparer _comparer;
        readonly IConfiguration _config;

        DirectoryInfo BaseFolder;

        public IEnumerable<Entry> Targets { get; set; } = Enumerable.Empty<Entry>();

        public List<(Entry Left, Entry Right)> Pairs { get; set; } = new();

        public Processor(IImageSetLoader loader, ILogger logger, IImagerComparer comparer, IConfiguration config)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<int> AddTargets(DirectoryInfo baseFolder)
        {
            BaseFolder = baseFolder;
            Targets = await _loader.GetImages(BaseFolder);
            _logger.Information("Images loaded.");

            return Targets.Count();
        }

        public async Task<int> Process()
        {
            var result = await FindPairs(Targets);
            Pairs = result.ToList();
            return Pairs.Count();
            // we now have a list of pairs -- images which are similar to each other
        }

        private async Task<IEnumerable<(Entry, Entry)>> FindPairs(IEnumerable<Entry> images)
        {
            // todo compare euclidian distances if phash score is over 86%

            var uniquePairs = images.GetAllUniquePairs().ToList();

            var similarImages = new ConcurrentBag<(Entry, Entry)>();

            await Task.Run(() => Parallel.ForEach(uniquePairs, pair =>
            {
                var result = Similarity(pair.Item1, pair.Item2);
                if (result > 0.86)
                {
                    similarImages.Add(pair);
                }
            }));

            return similarImages;
        }

        private float Similarity(Entry original, Entry compare) => ImagePhash.GetCrossCorrelation(original.Hash, compare.Hash);

        public void Prune()
        {
            _comparer.Process(Pairs);
            string path = BaseFolder.FullName + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(path + "Keep");
            Directory.CreateDirectory(path + "Trash");
            Directory.CreateDirectory(path + "Unsure");

            Move(_comparer.Keep, new DirectoryInfo(path + "Keep"));
            Move(_comparer.Trash, new DirectoryInfo(path + "Trash"));
            Move(_comparer.Unsure, new DirectoryInfo(path + "Unsure"));
        }

        private void Move(IEnumerable<Entry> images, DirectoryInfo destination)
        {
            foreach (var image in images)
            {
                if (!File.Exists(image.FullPath))
                {
                    _logger.Warning($"Image {image.FullPath} not found.");
                    continue;
                }

                // check config for whether or not to override files, if no config assume don't override
                bool overrideFiles = bool.TryParse(_config.GetSection("Override").Value, out bool result);
                // , overrideFiles ? result : false
                File.Move(image.FullPath, destination.FullName + Path.DirectorySeparatorChar + image.Filename);

                // todo: should it delete the original as well?
            }
        }
    }
}
