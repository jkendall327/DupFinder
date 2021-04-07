using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    static class Extensions
    {
        // https://stackoverflow.com/questions/17031771/comparing-each-element-with-each-other-element-in-a-list

        /// <summary>
        /// Sorts a list of items into a list of its unique pairs, in tuples.
        /// </summary>
        public static IEnumerable<(T, T)> UniquePairs<T>(this IEnumerable<T> source)
            => source.SelectMany((_, i) => source.Where((_, j) => i < j), (x, y) => (x, y));
    }
}
