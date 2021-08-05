using System;
using System.Collections.Generic;
using System.Linq;
using DupFinderCore.Enums;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using Microsoft.Extensions.Logging;

namespace DupFinderCore.Services
{
    /// <inheritdoc cref="IImageComparer"/>
    public class ImageComparer : IImageComparer
    {
        public List<IEntry> Keep { get; set; } = new List<IEntry>();
        public List<IEntry> Trash { get; set; } = new List<IEntry>();
        public List<IEntry> Unsure { get; set; } = new List<IEntry>();

        private IImageComparisonRuleset _ruleset { get; }
        private ILogger<ImageComparer> _logger { get; }

        public ImageComparer(IImageComparisonRuleset ruleset, ILogger<ImageComparer> logger)
        {
            _ruleset = ruleset;
            _logger = logger;
        }

        public void Compare(IEnumerable<Pair> pairs)
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

        private void Judge(Pair pair)
        {
            if (DetermineUnsure(pair.Left, pair.Right))
            {
                Unsure.Add(pair.Left);
                Unsure.Add(pair.Right);

                _logger.LogInformation("{LeftFilename} and {RightFilename} comparison inconclusive.", pair.Left.TruncatedFilename, pair.Right.TruncatedFilename);

                return;
            }

            int leftWins = 0;
            int rightWins = 0;
            int unsure = 0;

            foreach (var Test in _ruleset.Rules)
            {
                var judgement = Test(pair.Left, pair.Right);

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
                Unsure.Add(pair.Left);
                Unsure.Add(pair.Right);
                _logger.LogInformation("{LeftFilename} and {RightFilename} comparison inconclusive.", pair.Left.TruncatedFilename, pair.Right.TruncatedFilename);

                return;
            }
            if (highest == leftWins)
            {
                Keep.Add(pair.Left);
                Trash.Add(pair.Right);
                _logger.LogInformation("{LeftFilename} better than {RightFilename}.", pair.Left.TruncatedFilename, pair.Right.TruncatedFilename);

                return;
            }
            if (highest == rightWins)
            {
                Keep.Add(pair.Right);
                Trash.Add(pair.Left);
                _logger.LogInformation("{RightFilename} better than {LeftFilename}.", pair.Right.TruncatedFilename, pair.Left.TruncatedFilename);

                return;
            }
        }
    }
}
