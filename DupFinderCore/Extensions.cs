using System.Collections.Generic;
using System.Linq;

namespace DupFinderCore
{
    static class Extensions
    {
        // https://stackoverflow.com/questions/17031771/comparing-each-element-with-each-other-element-in-a-list
        public static IEnumerable<(T, T)> GetAllPairs<T>(this IList<T> source)
        {
            return source.SelectMany((_, i) => source.Where((_, j) => i < j),
                (x, y) => (x, y));
        }
    }
}
