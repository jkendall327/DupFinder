using Microsoft.Extensions.Configuration;
using Serilog;
using Shipwreck.Phash;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public async Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images)
        {
            SimilarImages.Clear();

            //make list of tasks for comparing each unique pair in the given ienumerable of entries
            var tasks = images.UniquePairs()
                .Select(pair => Task.Run(() => Compare(pair.Item1, pair.Item2)));

            await Task.WhenAll(tasks);

            _logger.Information($"All images processed. {SimilarImages.Count} pairs found.");

            return SimilarImages;
        }

        public async Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images, IProgress<PercentageProgress> progress)
        {
            SimilarImages.Clear();

            // make list of tasks for comparing each unique pair in the given ienumerable of entries
            var tasks = images.UniquePairs().ToList();

            int counter = 0;

            foreach (var (left, right) in tasks)
            {
                await Task.Run(() => Compare(left, right));
                counter++;
                progress.Report(new PercentageProgress() { TotalImages = tasks.Count, AmountDone = counter });
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

            left.Dispose();
            right.Dispose();

            // images are similar
            SimilarImages.Add((left, right));
        }

        private double TruncatedPercentage(double input)
            => Math.Truncate(input * 10 / 10);
    }
}
