using System.Collections.Generic;

namespace DupFinderCore
{
    /// <summary>
    /// Compares pairs of <see cref="IEntry"/> items and sorts them into lists based on criteria found in a <see cref="IImageComparisonRuleset"/>.
    /// </summary>
    public interface IImageComparer
    {
        /// <summary>
        /// <see cref="IEntry"/> items to be kept.
        /// </summary>
        public List<IEntry> Keep { get; set; }

        /// <summary>
        /// <see cref="IEntry"/> items to be trashed.
        /// </summary>
        public List<IEntry> Trash { get; set; }

        /// <summary>
        /// <see cref="IEntry"/> items whose relative quality cannot be determined.
        /// </summary>
        public List<IEntry> Unsure { get; set; }

        /// <summary>
        /// Compares pairs of <see cref="IEntry"/> items and sorts them into lists indicating their relative quality.
        /// </summary>
        /// <param name="pairs">A list of <see cref="IEntry"/> pairs to be compared.</param>
        /// <param name="settings">The user's desired criteria for comparing images: pixel count, file size, etc.</param>
        void Compare(IEnumerable<(IEntry left, IEntry right)> pairs);
    }
}
