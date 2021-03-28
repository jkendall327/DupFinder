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

        public IEnumerable<Entry> Targets { get; set; } = Enumerable.Empty<Entry>();

        public List<(Entry Left, Entry Right)> Pairs { get; set; } = new();

        public Processor(IImageSetLoader loader, ILogger logger, IImagerComparer comparer)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
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

        readonly IEnumerable<Entry> Keep = Enumerable.Empty<Entry>();
        readonly IEnumerable<Entry> Trash = Enumerable.Empty<Entry>();
        readonly IEnumerable<Entry> Unsure = Enumerable.Empty<Entry>();

        public void Prune()
        {
            foreach (var pair in Pairs)
            {
                _comparer.SetImages(pair.Left, pair.Right);
                // maybe give the ienumerables to the comparer as well
                var winner = _comparer.GetBetterImage();

                if (winner is null)
                {
                    Unsure.Append(pair.Left); Unsure.Append(pair.Right);
                    continue;
                }

                Keep.Append(winner);
                // how to get trashed image? give enumerables to comparer...
                // it should tell you what images to move and keep, not the processor
            }

            MovePairs();
        }

        private void MovePairs()
        {
            /*
             * Actually do something with the newly-judged pairs.
             * I.e. move them to the right folders.
             * Hide this behind an interface maybe.
             */

            foreach (var image in Keep)
            {

            }
        }
    }
}
