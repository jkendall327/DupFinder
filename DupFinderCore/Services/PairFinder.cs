using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using Microsoft.Extensions.Logging;
using Shipwreck.Phash;

namespace DupFinderCore.Services
{
    /// <inheritdoc cref="IPairFinder"/>
    public class PairFinder
    {
        private readonly ILogger<PairFinder> _logger;

        private readonly double regularLimit = 99.999;
        private readonly double phashLimit = 0.86;
        public bool WeightedComparisons { get; set; } = true;

        public PairFinder(ILogger<PairFinder> logger)
        {
            _logger = logger;
        }

        private static double TruncatedPercentage(double input) => Math.Truncate(input * 10 / 10);

        public async IAsyncEnumerable<Pair> FindPairs(IEnumerable<IEntry> images)
        {
            foreach (Pair pair in images.UniquePairs())
            {
                if (await Task.Run(() => AreSimilar(pair.Left, pair.Right)))
                {
                    _logger.LogInformation("Pair found: {Pair}", pair);

                    yield return new Pair(pair.Left, pair.Right);
                }
            }
        }

        private bool AreSimilar(IEntry left, IEntry right)
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

            if (WeightedComparisons)
            {
                var focusedDistance = left.FocusedColorMap.CompareWith(right.FocusedColorMap);

                var averageDistance = (euclidianDistance + focusedDistance) / 2;

                if (averageDistance < regularLimit)
                    return false;
            }

            return true;
        }
    }
}
