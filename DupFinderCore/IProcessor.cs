using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IProcessor
    {
        Task<int> LoadImages(DirectoryInfo baseFolder);
        Task<int> FindSimilarImages();
        void FindBetterImages(UserSettings settings);
    }
}
