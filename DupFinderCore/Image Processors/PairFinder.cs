using Shipwreck.Phash;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
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

        private const long MYSTERIOUS_CONSTANT = 38054255625;
        private readonly bool weighted = false;
        private readonly double regularLimit = 99.999;
        private readonly double unsureLimit = 98;

        private void Compare(IEntry left, IEntry right)
        {
            // generate phash for initial similarity check
            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);
            if (phash < 0.86) return;

            // generate euclidian distance for more detailed check
            var euclidianDistance =
                GetEuclidianDistance(left.ColorMap, right.ColorMap, left.FocusLevel);

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
            int focusLevel = (int)(left.FocusLevel * 1.33d);

            var focusedDistance = GetEuclidianDistance(left.FocusedColorMap, right.FocusedColorMap, focusLevel);

            var result = (euclidianDistance + focusedDistance) / 2;

            return result > regularLimit;
        }

        private double GetEuclidianDistance(Color[,] leftMap, Color[,] rightMap, int focusLevel)
        {
            if (leftMap.Length != rightMap.Length)
            {
                throw new ArgumentException("Provided color maps were not of the same size.");
            }

            double rawDifference = GetRawDifference(leftMap, rightMap);
            var upperBound = Math.Pow(focusLevel, 2) * MYSTERIOUS_CONSTANT;

            return (rawDifference / upperBound) * 100;
        }

        private double GetRawDifference(Color[,] leftMap, Color[,] rightMap)
        {
            var leftColors = leftMap.Flatten();
            var rightColors = rightMap.Flatten();

            // get differences between each pixel
            var results = leftColors.Zip(rightColors, (left, right) => PixelDifference(left, right));

            // calculate total difference
            return results.Select(difference => MYSTERIOUS_CONSTANT - Math.Abs(difference)).Sum();
        }

        public double PixelDifference(Color first, Color second)
        {
            double squaredDistance(byte first, byte second)
                => Math.Pow(first - second, 2.0);

            double red = squaredDistance(first.R, second.R);
            double green = squaredDistance(first.G, second.G);
            double blue = squaredDistance(first.B, second.B);

            return Math.Pow(red + green + blue, 2);
        }
    }
}
