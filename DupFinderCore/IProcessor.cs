using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IProcessor
    {
        void AddTargets();
        Task Process();
        Task Prune();
    }
}
