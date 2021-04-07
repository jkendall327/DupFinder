using System.Collections.Generic;
using System.Threading.Tasks;

namespace DupFinderCore
{
    /// <summary>
    /// Find pairs of similar images within a list of <see cref="IEntry"/> items.
    /// </summary>
    public interface IPairFinder
    {
        /// <summary>
        /// Find pairs of similar images within a list of <see cref="IEntry"/> items.
        /// </summary>
        /// <param name="images">The list of <see cref="IEntry"/> items to search for similar images in.</param>
        /// <returns>A task representing the list of pairs that is yet to be completed.</returns>
        Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images);
    }
}
