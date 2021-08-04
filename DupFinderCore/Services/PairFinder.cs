using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using Serilog;
using Shipwreck.Phash;

namespace DupFinderCore.Services
{
    /// <inheritdoc cref="IPairFinder"/>
    public class PairFinder
    {
        private readonly ILogger _logger;
        private readonly double regularLimit = 99.999;
        private readonly double phashLimit = 0.86;

        public PairFinder(ILogger logger)
        {
            _logger = logger;
        }

        private static double TruncatedPercentage(double input) => Math.Truncate(input * 10 / 10);

        public async Task<ConcurrentBag<Pair>> FindPairs(IEnumerable<IEntry> images, bool doWeightedComparison = true)
        {
            ConcurrentBag<Pair> similarImages = new();

            IEnumerable<Task> tasks = images.UniquePairs()
            .Select(pair =>
            {
                void compare()
                {
                    if (!AreSimilar(pair.Item1, pair.Item2, doWeightedComparison)) return;

                    similarImages.Add(new(pair.Item1, pair.Item2));
                    _logger.Information($"Pair found: {pair}");
                }

                return Task.Run(compare);
            });

            await Task.WhenAll(tasks);

            _logger.Information($"Pairs found: {similarImages.Count}");

            return similarImages;
        }

        private bool AreSimilar(IEntry left, IEntry right, bool doWeightedComparison)
        {
            // generate phash for initial check
            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);

            if (phash < phashLimit)
                return false;

            // generate euclidian distance for more detailed check
            var euclidianDistance = left.ColorMap.CompareWith(right.ColorMap);

            // distance is below arbitrary threshold
            if (euclidianDistance < TruncatedPercentage(regularLimit))
                return false;

            if (doWeightedComparison)
            {
                var focusedDistance = left.FocusedColorMap.CompareWith(right.FocusedColorMap);

                var averageDistance = (euclidianDistance + focusedDistance) / 2;

                if (averageDistance < regularLimit)
                    return false;
            }

            _logger.Information($"Pair found: {left.TruncatedFilename} and {right.TruncatedFilename}");
            return true;
        }
    }
}
