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
            Targets = await _loader.GetImages(baseFolder);
            _logger.Information("Images loaded.");

            return Targets.Count();
        }

        public async Task<int> Process()
        {
            var pairs = await FindPairs(Targets);
            return pairs.Count();
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

            MovePairs(_comparer.Keep, new DirectoryInfo("Keep"));
            MovePairs(_comparer.Trash, new DirectoryInfo("Trash"));
            MovePairs(_comparer.Unsure, new DirectoryInfo("Unsure"));
        }

        private void MovePairs(IEnumerable<Entry> images, DirectoryInfo destination)
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

                File.Move(image.FullPath, destination.FullName + image.Filename, overrideFiles ? result : false);

                // todo: should it delete the original as well?
            }
        }
    }
}
