using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using DupFinderCore.Services;

namespace DupFinderCore
{
    public class Processor
    {
        private readonly ImageSetLoader _loader;
        private readonly IImageComparer _comparer;
        private readonly PairFinder _finder;
        private readonly Mover _mover;

        public IEnumerable<IEntry> Targets { get; private set; } = new List<IEntry>();
        public IEnumerable<Pair> Pairs { get; private set; } = new List<Pair>();

        public Processor(ImageSetLoader loader, IImageComparer comparer, PairFinder finder, Mover mover)
        {
            _loader = loader;
            _comparer = comparer;
            _finder = finder;
            _mover = mover;
        }

        public async Task LoadImages(DirectoryInfo baseFolder)
        {
            var result = _loader.LoadImagesAsync(baseFolder);

            Targets = await result.ToListAsync();
        }

        public async Task FindSimilarImages(IEnumerable<IEntry> targets)
        {
            Pairs = await _finder.FindPairs(targets).ToListAsync();
        }

        public void CompareImages(IEnumerable<Pair> pairs)
        {
            _comparer.Compare(pairs);
        }

        public void MoveImages(DirectoryInfo baseFolder)
        {
            _mover.MoveImages(baseFolder);
        }

        public int MovedImageCount => _comparer.Keep.Count + _comparer.Trash.Count + _comparer.Unsure.Count;
    }
}
