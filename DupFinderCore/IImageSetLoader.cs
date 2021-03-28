using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IImageSetLoader
    {
        Task<IEnumerable<Entry>> GetImages(DirectoryInfo dir);
    }
}