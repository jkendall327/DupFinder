using DupFinderCore.Interfaces;

namespace DupFinderCore.Models
{
    public record Pair(IEntry Left, IEntry Right);
}
