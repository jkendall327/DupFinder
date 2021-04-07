using System;
using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    /// <summary>
    /// Compares two <see cref="Entry"/> items to determine which is superior.
    /// </summary>
    public interface IImageComparisonRuleset
    {
        /// <summary>
        /// List of methods that will compare two <see cref="Entry"/> items and return a <see cref="Judgement"/> indicating which is superior.
        /// </summary>
        List<Func<IEntry, IEntry, Judgement>> Rules { get; }

        void Configure(UserSettings settings);
    }

    /// <inheritdoc cref="IImageComparisonRuleset"/>
    public class ImageComparisonRuleset : IImageComparisonRuleset
    {
        public List<Func<IEntry, IEntry, Judgement>> Rules { get; private set; } = new List<Func<IEntry, IEntry, Judgement>>();

        private Judgement ComparePixels(IEntry left, IEntry right)
        {
            var pixels = new Func<IEntry, int>(x => x.Pixels);
            return CompareByProperty(left, right, pixels, Order.Biggest);
        }

        private Judgement CompareDate(IEntry left, IEntry right)
        {
            var date = new Func<IEntry, DateTime>(x => x.Date);
            return CompareByProperty(left, right, date, Order.Biggest);
        }

        private Judgement CompareSize(IEntry left, IEntry right)
        {
            var size = new Func<IEntry, long>(x => x.Size);
            return CompareByProperty(left, right, size, Order.Smallest);
        }

        private enum Order { Biggest, Smallest }

        private Judgement CompareByProperty<T>(IEntry left, IEntry right, Func<IEntry, T> propertyToCompare, Order order)
        {
            // evaluate the properties so we can compare for equality
            T leftProperty = propertyToCompare(left);
            T rightProperty = propertyToCompare(right);

            // check if the two entries have the same value for the given property
            if (EqualityComparer<T>.Default.Equals(leftProperty, rightProperty))
            {
                return Judgement.Unsure;
            }

            IEnumerable<IEntry> list = new List<IEntry>() { left, right };

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

        /// <summary>
        /// Read a <see cref="UserSettings"/> object to dynamically add rules for image comparison.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(UserSettings settings)
        {
            if (settings.CompareByDate)
            {
                Rules.Add(CompareDate);
            }
            if (settings.CompareByPixels)
            {
                Rules.Add(ComparePixels);
            }
            if (settings.CompareBySize)
            {
                Rules.Add(CompareSize);
            }
        }
    }
}
