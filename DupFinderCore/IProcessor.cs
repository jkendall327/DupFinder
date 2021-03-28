using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IProcessor
    {
        Task<int> AddTargets(DirectoryInfo baseFolder);
        Task<int> Process();
        void Prune();
    }
}
