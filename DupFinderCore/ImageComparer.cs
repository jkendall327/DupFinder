using System;
using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    public interface IImagerComparer
    {
        public List<Entry> Keep { get; }
        public List<Entry> Trash { get; }
        public List<Entry> Unsure { get; }

        void Process(IEnumerable<(Entry left, Entry right)> images);
    }

    public enum Judgement
    {
        Left, Right, Unsure
    }

    public class ImageComparer : IImagerComparer
    {
        private List<(Entry left, Entry right)> Pairs { get; set; } = new List<(Entry left, Entry right)>();

        public List<Entry> Keep => new List<Entry>();
        public List<Entry> Trash => new List<Entry>();
        public List<Entry> Unsure => new List<Entry>();

        readonly IImageComparisonRuleset _ruleset;

        public ImageComparer(IImageComparisonRuleset ruleset)
        {
            _ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
        }

        public void Process(IEnumerable<(Entry left, Entry right)> pairs)
        {
            if (pairs is null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            foreach ((Entry left, Entry right) pair in Pairs)
            {
                DetermineJudgement(pair);
            }
        }

        private void DetermineJudgement((Entry left, Entry right) pair)
        {
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
    }
}
