using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IImageSetLoader
    {
        Task<List<Entry>> GetImages(DirectoryInfo dir);
    }
}