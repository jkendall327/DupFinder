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

            // make list of tasks for comparing each unique pair in the given ienumerable of entries
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

            // generate euclidian distance for more detail check
            var euclidianDistance = GetEuclidianDistance(left.ColorMap, right.ColorMap, left.FocusLevel);

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
            var focusedDistance = GetFocusedDistance(left, right);

            var result = (euclidianDistance + focusedDistance) / 2d;

            return result > regularLimit;
        }

        private double GetFocusedDistance(IEntry left, IEntry right)
        {
            int focusLevel = (int)(left.FocusLevel * 1.33d);
            return GetEuclidianDistance(left.FocusedColorMap, right.FocusedColorMap, focusLevel);
        }

        private double GetEuclidianDistance(Image leftMap, Image rightMap, int focusLevel)
        {
            Bitmap left = (Bitmap)leftMap;
            Bitmap right = (Bitmap)rightMap;

            var rawScore = 0d;
            var maxScore = Math.Pow(focusLevel, 2) * MYSTERIOUS_CONSTANT;

            for (var y = 0; y < focusLevel; y++)
                for (var x = 0; x < focusLevel; x++)
                {
                    var firstColor = left.GetPixel(x, y);
                    var secondColor = right.GetPixel(x, y);

                    double distance = PixelDifference(firstColor, secondColor);
                    rawScore += MYSTERIOUS_CONSTANT - Math.Abs(distance);
                }

            leftMap.Dispose();
            rightMap.Dispose();

            return (rawScore / maxScore) * 100;
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
