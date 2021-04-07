using System.Collections.Generic;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IPairFinder
    {
        Task<IEnumerable<(IEntry, IEntry)>> FindPairs(IEnumerable<IEntry> images);
    }
}
