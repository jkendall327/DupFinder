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

        private readonly IImageComparisonRuleset _ruleset;

        public ImageComparer(IImageComparisonRuleset ruleset)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
        }

        public void Compare(IEnumerable<(IEntry left, IEntry right)> pairs)
        {
            if (pairs is null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            Keep.Clear();
            Trash.Clear();
            Unsure.Clear();

            _ruleset.Configure();

            foreach (var pair in pairs)
            {
                DetermineJudgement(pair);
            }
        }

        private void DetermineJudgement((IEntry left, IEntry right) pair)
        {
            if (DetermineUnsure(pair.left, pair.right))
            {
                Unsure.Add(pair.left);
                Unsure.Add(pair.right);
                return;
            }

            int leftWins = 0;
            int rightWins = 0;
            int unsure = 0;

            foreach (var Rule in _ruleset.Rules)
            {
                var judgement = Rule(pair.left, pair.right);
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

            // if two sides have the same amount of wins, it will default to unsure
            // and then to left, and then to right
            // this is arbitrary

            if (highest == unsure)
            {
                Unsure.Add(pair.left);
                Unsure.Add(pair.right);
                return;
            }
            if (highest == leftWins)
            {
                Keep.Add(pair.left);
                Trash.Add(pair.right);
                return;
            }
            if (highest == rightWins)
            {
                Keep.Add(pair.right);
                Trash.Add(pair.left);
                return;
            }
        }

        private bool DetermineUnsure(IEntry left, IEntry right)
        {
            var aspectRatioDifference = Math.Abs(left.AspectRatio - right.AspectRatio);
            var pixelDifference = Math.Abs(((double)left.Pixels / right.Pixels) - 1d);
            var sizeDifference = Math.Abs(((double)left.Size / right.Size) - 1d);

            return Math.Abs(pixelDifference - sizeDifference) >= 1 || aspectRatioDifference > 0.05;
        }
    }
}
