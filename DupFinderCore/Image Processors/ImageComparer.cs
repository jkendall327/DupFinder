﻿using System;
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

        readonly IImageComparisonRuleset _ruleset;

        public ImageComparer(IImageComparisonRuleset ruleset)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
        }

        public void Compare(IEnumerable<(IEntry left, IEntry right)> pairs, UserSettings settings)
        {
            if (pairs is null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            _ruleset.Configure(settings);

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

            // todo what if two or all values are equal somehow...?

            var highest = new List<int>() { leftWins, rightWins, unsure }.Max();

            if (highest == leftWins)
            {
                Keep.Add(pair.left);
                Trash.Add(pair.right);
            }
            if (highest == rightWins)
            {
                Keep.Add(pair.right);
                Trash.Add(pair.left);
            }
            if (highest == unsure)
            {
                Unsure.Add(pair.left);
                Unsure.Add(pair.right);
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