using System;
using System.Collections.Generic;

namespace DupFinderCore
{
    public interface IImageComparisonRuleset
    {
        List<Func<Entry, Entry, Judgement>> Rules { get; set; }
    }

    public class ImageComparisonRuleset : IImageComparisonRuleset
    {
        public List<Func<Entry, Entry, Judgement>> Rules { get; set; } = new List<Func<Entry, Entry, Judgement>>();

        public ImageComparisonRuleset()
        {
            Rules.Add(ComparePixels);
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
    }
}
