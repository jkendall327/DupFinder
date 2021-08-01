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

        /// <summary>
        /// Read a <see cref="UserSettings"/> object to dynamically add rules for image comparison.
        /// </summary>
        /// <param name="settings"></param>
        void Configure();
    }

    /// <inheritdoc cref="IImageComparisonRuleset"/>
    public class ImageComparisonRuleset : IImageComparisonRuleset
    {
        public List<Func<IEntry, IEntry, Judgement>> Rules { get; private set; } = new();

        private readonly UserSettings _settings;

        public ImageComparisonRuleset(UserSettings settings)
        {
            _settings = settings;
        }

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

        private static Judgement CompareByProperty<T>(IEntry left, IEntry right, Func<IEntry, T> property, Order order)
        {
            // evaluate properties
            T leftProperty = property(left);
            T rightProperty = property(right);

            if (EqualityComparer<T>.Default.Equals(leftProperty, rightProperty))
            {
                // both have same value for given property
                return Judgement.Unsure;
            }

            IEnumerable<IEntry> list = new List<IEntry>() { left, right };

            list = order switch
            {
                Order.Smallest => list.OrderBy(property),
                _ => list.OrderByDescending(property),
            };

            var winner = list.First();

            if (winner == left) return Judgement.Left;
            if (winner == right) return Judgement.Right;
            return Judgement.Unsure;
        }

        public void Configure()
        {
            if (_settings.CompareByDate) Rules.Add(CompareDate);
            
            if (_settings.CompareByPixels) Rules.Add(ComparePixels);
            
            if (_settings.CompareBySize) Rules.Add(CompareSize);
        }
    }
}
