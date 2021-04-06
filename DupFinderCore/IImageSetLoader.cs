using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IImageSetLoader
    {
        Task<IEnumerable<Entry>> LoadImages(DirectoryInfo dir);
    }
}