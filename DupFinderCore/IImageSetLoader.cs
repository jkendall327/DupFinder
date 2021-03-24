using System.Collections.Generic;
using System.Drawing;

namespace DupFinderCore
{
    public interface IImageSetLoader
    {
        IEnumerable<Image> GetImages();
    }
}