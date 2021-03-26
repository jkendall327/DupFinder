using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IImageSetLoader
    {
        Task<List<Image>> GetImages(DirectoryInfo dir);
    }
}