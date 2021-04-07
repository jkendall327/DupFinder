using System.Collections.Generic;

namespace DupFinderCore
{
    public interface IImagerComparer
    {
        public List<IEntry> Keep { get; set; }
        public List<IEntry> Trash { get; set; }
        public List<IEntry> Unsure { get; set; }

        void Process(IEnumerable<(IEntry left, IEntry right)> images, UserSettings settings);
    }
}
