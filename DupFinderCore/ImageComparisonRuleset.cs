using System;
using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    /// <summary>
    /// Represents a class that contains methods which compare two <see cref="Entry"/> items to determine which is superior.
    /// </summary>
    public interface IImageComparisonRuleset
    {
        /// <summary>
        /// Methods that will compare two <see cref="Entry"/> items and return a <see cref="Judgement"/> indicating which is superior.
        /// </summary>
        List<Func<Entry, Entry, Judgement>> Rules { get; }
    }

    public class ImageComparisonRuleset : IImageComparisonRuleset
    {
        public List<Func<Entry, Entry, Judgement>> Rules { get; private set; } = new List<Func<Entry, Entry, Judgement>>();

        public ImageComparisonRuleset()
        {
            Rules.Add(ComparePixels);
            Rules.Add(CompareDate);
            Rules.Add(CompareSize);
        }

        private Judgement ComparePixels(Entry left, Entry right)
        {
            var pixels = new Func<Entry, int>(x => x.Pixels);
            return CompareByProperty(left, right, pixels, Order.Biggest);
        }

        private Judgement CompareDate(Entry left, Entry right)
        {
            var date = new Func<Entry, DateTime>(x => x.Date);
            return CompareByProperty(left, right, date, Order.Biggest);
        }

        private Judgement CompareSize(Entry left, Entry right)
        {
            var size = new Func<Entry, long>(x => x.Size);
            return CompareByProperty(left, right, size, Order.Smallest);
        }

        private enum Order { Biggest, Smallest }

        private Judgement CompareByProperty<T>(Entry left, Entry right, Func<Entry, T> propertyToCompare, Order order)
        {
            // evaluate the properties so we can compare for equality
            T leftProperty = propertyToCompare(left);
            T rightProperty = propertyToCompare(right);

            // check if the two entries have the same value for the given property
            if (EqualityComparer<T>.Default.Equals(leftProperty, rightProperty))
            {
                return Judgement.Unsure;
            }

            IEnumerable<Entry> list = new List<Entry>() { left, right };

            // inject the property to compare
            // decide whether to find the smallest or biggest value by switching on order
            list = order switch
            {
                Order.Smallest => list.OrderBy(propertyToCompare),
                Order.Biggest => list.OrderByDescending(propertyToCompare),
                _ => list.OrderByDescending(propertyToCompare),
            };

            var winner = list.First();

            if (winner == left)
            {
                return Judgement.Left;
            }
            if (winner == right)
            {
                return Judgement.Right;
            }

            return Judgement.Unsure;
        }
    }
}
