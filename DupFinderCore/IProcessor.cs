using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IProcessor
    {
        Task<int> AddTargets();
        Task Process();
        void Prune();
    }
}
