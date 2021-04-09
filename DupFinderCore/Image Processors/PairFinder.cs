using Shipwreck.Phash;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DupFinderCore
{
    /// <inheritdoc cref="IPairFinder"/>
    public class PairFinder : IPairFinder
    {
        private readonly ConcurrentBag<(IEntry, IEntry)> SimilarImages = new();

        public async Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images)
        {
            SimilarImages.Clear();

            //make list of tasks for comparing each unique pair in the given ienumerable of entries

            var tasks = images.UniquePairs()
                .Select(pair => Task.Run(() => Compare(pair.Item1, pair.Item2)));

            await Task.WhenAll(tasks);

            return SimilarImages;
        }

        private readonly bool weighted = false;
        private readonly double regularLimit = 99.999;
        private readonly double unsureLimit = 98;

        private void Compare(IEntry left, IEntry right)
        {
            // generate phash for initial similarity check
            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);
            if (phash < 0.86) return;

            // generate euclidian distance for more detailed check
            var euclidianDistance = left.ColorMap.CompareWith(right.ColorMap);

            if (euclidianDistance > TruncatedPercentage(unsureLimit))
            {
                // todo: eScore not high enough to be sure it's a pair, but high enough to be unsure
                // do something with this or ignore?
                return;
            }

            // distance is below arbitrary threshold
            if (euclidianDistance < TruncatedPercentage(regularLimit))
            {
                return;
            }

            // todo weighted comparisons should be in IConfiguration
            if (weighted)
            {
                if (!WeightedComparison(left, right, euclidianDistance))
                {
                    return;
                }
            }

            // images are similar
            SimilarImages.Add((left, right));
        }

        private double TruncatedPercentage(double input)
            => Math.Truncate(input * 10 / 10);

        private bool WeightedComparison(IEntry left, IEntry right, double euclidianDistance)
        {
            var focusedDistance = left.FocusedColorMap.CompareWith(right.FocusedColorMap);

            var result = (euclidianDistance + focusedDistance) / 2;

            return result > regularLimit;
        }
    }
}
