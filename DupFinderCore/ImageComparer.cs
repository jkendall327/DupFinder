using System;
using System.Collections.Generic;

namespace DupFinderCore
{
    public interface IImagerComparer
    {
        List<Func<Entry, Entry, Entry>> Rules { get; set; }
        Entry GetBetterImage();
        void SetImages(Entry left, Entry right);
    }

    public class ImageComparer : IImagerComparer
    {
        public Entry Left { get; private set; }
        public Entry Right { get; private set; }


        //a list of methods that take in two images as parameters and return an image
        public List<Func<Entry, Entry, Entry>> Rules { get; set; } = new List<Func<Entry, Entry, Entry>>();

        public ImageComparer()
        {
            Rules.Add(ComparePixels);
        }

        public void SetImages(Entry left, Entry right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        private Entry ComparePixels(Entry left, Entry right)
        {
            if (left.Pixels > right.Pixels)
            {
                return left;
            }
            else if (left.Pixels < right.Pixels)
            {
                return right;
            }

            return left;
        }

        public Entry GetBetterImage()
        {
            int leftWins = 0;
            int rightwins = 0;

            // we can add as many rules as we like when comparing images without modifying this class
            foreach (var rule in Rules)
            {
                if (rule(Left, Right) == Left)
                {
                    leftWins++;
                }
                else
                {
                    rightwins++;
                }
            }

            return leftWins > rightwins ? Left : Right;

            // return null if uncertain...
        }
    }
}
