using Microsoft.Extensions.Configuration;
using Serilog;
using Shipwreck.Phash;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace DupFinderCore
{
    /// <inheritdoc cref="IPairFinder"/>
    public class PairFinder
    {
        private readonly ConcurrentBag<(IEntry, IEntry)> SimilarImages = new();

        private readonly double regularLimit = 99.999;

        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public PairFinder(IConfiguration config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConcurrentBag<(IEntry, IEntry)>> FindPairs(ConcurrentBag<IEntry> images, IProgress<PercentageProgress>? progress = null)
        {
            SimilarImages.Clear();

            var tasks = images.UniquePairs()
            .Select(pair => Task.Run(() => Compare(pair.Item1, pair.Item2)))
            .ToList();

            var totalTasks = tasks.Count;

            int total = 0;
            while (tasks.Any())
            {
                var finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);

                total++;
                progress?.Report(new PercentageProgress() { TotalImages = totalTasks, AmountDone = total });
            }

            _logger.Information($"Pairs found: {SimilarImages.Count}");

            return SimilarImages;
        }

        private void Compare(IEntry left, IEntry right)
        {
            // generate phash for initial check
            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);

            if (phash < 0.86)
                return;

            // generate euclidian distance for more detailed check
            var euclidianDistance = left.ColorMap.CompareWith(right.ColorMap);

            // distance is below arbitrary threshold
            if (euclidianDistance < TruncatedPercentage(regularLimit))
                return;

            if (_config.GetValue<bool>("WeightedImageComparison"))
            {
                var focusedDistance = left.FocusedColorMap.CompareWith(right.FocusedColorMap);

                var averageDistance = (euclidianDistance + focusedDistance) / 2;

                if (averageDistance < regularLimit)
                    return;
            }

            // images are similar
            SimilarImages.Add((left, right));
        }

        private double TruncatedPercentage(double input)
            => Math.Truncate(input * 10 / 10);
    }
}
