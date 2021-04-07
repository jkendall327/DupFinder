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

        readonly bool weighted = false;
        readonly double regularLimit = 99.999;
        readonly double unsureLimit = 98;

        private bool ImagesAreSimilar((IEntry, IEntry) pair)
        {
            var left = pair.Item1;
            var right = pair.Item2;

            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);

            if (phash < 0.86) return false;

            var euclidianDistance = GetEuclidianDistance(left, right);

            if (euclidianDistance > TruncatedPercentage(unsureLimit))
            {
                // eScore not high enough to be sure it's a pair, but high enough to be unsure
                // do something with this or ignore?
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

        private bool WeightedComparison(IEntry left, IEntry right, double euclidianDistance)
        {
            var copy1 = left;
            var copy2 = right;

            copy1.FocusLevel = (int)(copy1.FocusLevel * 1.33d);
            copy2.FocusLevel = (int)(copy2.FocusLevel * 1.33d);

            copy1.GetColorMap(true);
            copy2.GetColorMap(true);

            var focusedEuclidianDistance = GetEuclidianDistance(copy1, copy2);

            euclidianDistance = (euclidianDistance + focusedEuclidianDistance) / 2d;

            return euclidianDistance > regularLimit;
        }

        private double TruncatedPercentage(double input)
            => Math.Truncate((input * 10) / 10);

        private double GetEuclidianDistance(IEntry left, IEntry right)
        {
            var rawScore = 0d;
            var maxScore = Math.Pow(left.FocusLevel, 2) * 38054255625;

            var leftMap = (Bitmap)left.ColorMap;
            var rightMap = (Bitmap)right.ColorMap;

            for (var y = 0; y < left.FocusLevel; y++)
                for (var x = 0; x < left.FocusLevel; x++)
                {
                    var Color1 = leftMap.GetPixel(x, y);
                    var Color2 = rightMap.GetPixel(x, y);

                    rawScore += 38054255625 - Math.Abs(EuclideanDistance(Color1, Color2));
                }

            leftMap.Dispose();
            rightMap.Dispose();

            return (rawScore / maxScore) * 100;
        }

        public double EuclideanDistance(Color Color1, Color Color2)
        {
            double red = Math.Pow(Color1.R - Color2.R, 2.0);
            double green = Math.Pow(Color1.G - Color2.G, 2.0);
            double blue = Math.Pow(Color1.B - Color2.B, 2.0);

            return Math.Pow(red + green + blue, 2);
        }

    }
}
