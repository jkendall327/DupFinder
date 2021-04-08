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
        public async Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images)
        {
            var uniquePairs = images.UniquePairs().ToList();

            var similarImages = new ConcurrentBag<(IEntry, IEntry)>();

            // use return await task.whenall here

            await Task.Run(() => Parallel.ForEach(uniquePairs, pair =>
            {
                if (ImagesAreSimilar(pair))
                    similarImages.Add(pair);
            }));

            return similarImages;
        }

        private const long MYSTERIOUS_CONSTANT = 38054255625;
        readonly bool weighted = false;
        readonly double regularLimit = 99.999;
        readonly double unsureLimit = 98;

        private bool ImagesAreSimilar((IEntry, IEntry) pair)
        {
            var left = pair.Item1;
            var right = pair.Item2;

            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);

            if (phash < 0.86) return false;

            var euclidianDistance = GetEuclidianDistance((Bitmap)left.ColorMap, (Bitmap)right.ColorMap, left.FocusLevel);

            if (euclidianDistance > TruncatedPercentage(unsureLimit))
            {
                // todo: eScore not high enough to be sure it's a pair, but high enough to be unsure
                // do something with this or ignore?
                return false;
            }

            if (euclidianDistance < TruncatedPercentage(regularLimit))
            {
                return false;
            }

            if (weighted)
            {
                return WeightedComparison(left, right, euclidianDistance);
            }

            return true;
        }

        private double TruncatedPercentage(double input)
            => Math.Truncate((input * 10) / 10);

        private bool WeightedComparison(IEntry left, IEntry right, double euclidianDistance)
        {
            var leftMap = (Bitmap)left.FocusedColorMap;
            var rightMap = (Bitmap)right.FocusedColorMap;

            int focusLevel = (int)(left.FocusLevel * 1.33d);

            var focusedDistance = GetEuclidianDistance(leftMap, rightMap, focusLevel);

            euclidianDistance = (euclidianDistance + focusedDistance) / 2d;

            return euclidianDistance > regularLimit;
        }

        private double GetEuclidianDistance(Bitmap leftMap, Bitmap rightMap, int focusLevel)
        {
            var rawScore = 0d;
            var maxScore = Math.Pow(focusLevel, 2) * MYSTERIOUS_CONSTANT;

            for (var y = 0; y < focusLevel; y++)
                for (var x = 0; x < focusLevel; x++)
                {
                    var firstColor = leftMap.GetPixel(x, y);
                    var secondColor = rightMap.GetPixel(x, y);

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
