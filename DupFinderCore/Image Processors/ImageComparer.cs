using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    /// <inheritdoc cref="IImageComparer"/>
    public class ImageComparer : IImageComparer
    {
        public List<IEntry> Keep { get; set; } = new List<IEntry>();
        public List<IEntry> Trash { get; set; } = new List<IEntry>();
        public List<IEntry> Unsure { get; set; } = new List<IEntry>();

        private IImageComparisonRuleset _ruleset { get; }
        private ILogger _logger { get; }

        public ImageComparer(IImageComparisonRuleset ruleset, ILogger logger)
        {
            _ruleset = ruleset;
            _logger = logger;
        }

        public void Compare(IEnumerable<(IEntry left, IEntry right)> pairs)
        {
            Keep.Clear();
            Trash.Clear();
            Unsure.Clear();

            _ruleset.Configure();

            foreach (var pair in pairs)
            {
                Judge(pair);
            }
        }

        private bool DetermineUnsure(IEntry left, IEntry right)
        {
            var aspectRatioDifference = Math.Abs(left.AspectRatio - right.AspectRatio);
            var pixelDifference = Math.Abs(((double)left.Pixels / right.Pixels) - 1d);
            var sizeDifference = Math.Abs(((double)left.Size / right.Size) - 1d);

            return Math.Abs(pixelDifference - sizeDifference) >= 1 || aspectRatioDifference > 0.05;
        }

        private void Judge((IEntry left, IEntry right) pair)
        {
            if (DetermineUnsure(pair.left, pair.right))
            {
                Unsure.Add(pair.left);
                Unsure.Add(pair.right);

                _logger.Information($"Images {pair.left.TruncatedFilename} and {pair.right.TruncatedFilename} could not be judged conclusively. Both moved to 'Unsure' folder.");

                return;
            }

            int leftWins = 0;
            int rightWins = 0;
            int unsure = 0;

            foreach (var Test in _ruleset.Rules)
            {
                var judgement = Test(pair.left, pair.right);

                switch (judgement)
                {
                    case Judgement.Left:
                        leftWins++;
                        break;
                    case Judgement.Right:
                        rightWins++;
                        break;
                    case Judgement.Unsure:
                        unsure++;
                        break;
                    default:
                        break;
                }
            }

            var highest = new List<int>() { leftWins, rightWins, unsure }.Max();

            if (highest == unsure)
            {
                Unsure.Add(pair.left);
                Unsure.Add(pair.right);
                _logger.Information($"{pair.left.TruncatedFilename} and {pair.right.TruncatedFilename} comparison inconclusive.");

                return;
            }
            if (highest == leftWins)
            {
                Keep.Add(pair.left);
                Trash.Add(pair.right);
                _logger.Information($"{pair.left.TruncatedFilename} better than {pair.right.TruncatedFilename}.");

                return;
            }
            if (highest == rightWins)
            {
                Keep.Add(pair.right);
                Trash.Add(pair.left);
                _logger.Information($"{pair.right.TruncatedFilename} better than {pair.left.TruncatedFilename}.");

                return;
            }
        }
    }
}
