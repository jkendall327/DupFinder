using System;
using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    public interface IImagerComparer
    {
        List<Func<Entry, Entry, Judgement>> Rules { get; set; }

        public List<Entry> Keep { get; }
        public List<Entry> Trash { get; }
        public List<Entry> Unsure { get; }

        void Process();
        void Reset();
        void SetImages(IEnumerable<(Entry left, Entry right)> images);
    }

    public enum Judgement
    {
        Left, Right, Unsure
    }

    public class ImageComparer : IImagerComparer
    {
        //a list of methods that take in two images as parameters and return an image
        public List<Func<Entry, Entry, Judgement>> Rules { get; set; } = new List<Func<Entry, Entry, Judgement>>();

        private List<(Entry left, Entry right)> Pairs { get; set; } = new List<(Entry left, Entry right)>();

        public List<Entry> Keep => new List<Entry>();

        public List<Entry> Trash => new List<Entry>();

        public List<Entry> Unsure => new List<Entry>();

        public ImageComparer()
        {
            Rules.Add(ComparePixels);
        }

        public void SetImages(IEnumerable<(Entry left, Entry right)> pairs)
        {
            if (pairs is null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }

            Pairs = pairs.ToList();
        }

        public void Process()
        {
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

            foreach (var Rule in Rules)
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

            var list = new List<int>() { leftWins, rightWins, unsure };

            if (list.Max() == leftWins)
            {
                Keep.Add(pair.left);
                Trash.Add(pair.right);
            }
            if (list.Max() == rightWins)
            {
                Keep.Add(pair.right);
                Trash.Add(pair.left);
            }
            if (list.Max() == unsure)
            {
                Unsure.Add(pair.left);
                Unsure.Add(pair.right);
            }
        }

        private Judgement ComparePixels(Entry left, Entry right)
        {
            if (left.Pixels > right.Pixels)
            {
                return Judgement.Left;
            }
            else if (left.Pixels < right.Pixels)
            {
                return Judgement.Right;
            }

            return Judgement.Unsure;
        }


        public void Reset()
        {
            Keep.Clear();
            Trash.Clear();
            Unsure.Clear();
            Rules.Clear();
        }
    }
}
