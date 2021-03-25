using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IImageSetLoader
    {
        Task<IEnumerable<Image>> GetImages();
    }
}