using System.Drawing;

namespace DupFinderCore
{
    public interface IImageLoader
    {
        Image Load(string filepath);
    }
}
