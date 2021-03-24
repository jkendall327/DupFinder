namespace DupFinderCore
{
    public interface IProcessor
    {
        void AddTargets();
        void Process();
        void Prune();
    }
}
