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

        private readonly bool weighted = false;
        private readonly double regularLimit = 99.999;
        private readonly double unsureLimit = 98;

        public async Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images)
        {
            SimilarImages.Clear();

            //make list of tasks for comparing each unique pair in the given ienumerable of entries
            var tasks = images.UniquePairs()
                .Select(pair => Task.Run(() => Compare(pair.Item1, pair.Item2)));

            await Task.WhenAll(tasks);

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

            // todo: high enough to be sure it's a pair, but high enough to be unsure
            // do something with this?
            if (euclidianDistance > TruncatedPercentage(unsureLimit))
                return;

            // distance is below arbitrary threshold
            if (euclidianDistance < TruncatedPercentage(regularLimit))
                return;

            // todo whether to do weighted comparisons should be in IConfiguration
            if (weighted)
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
