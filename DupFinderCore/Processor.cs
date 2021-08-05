using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using DupFinderCore.Services;
using Serilog;

namespace DupFinderCore
{
    public class Processor
    {
        private readonly ImageSetLoader _loader;
        private readonly IImageComparer _comparer;
        private readonly PairFinder _finder;
        private readonly Mover _mover;

        public ConcurrentBag<IEntry> Targets { get; private set; } = new();
        public ConcurrentBag<Pair> Pairs { get; private set; } = new();

        public Processor(ImageSetLoader loader, IImageComparer comparer, PairFinder finder, Mover mover)
        {
            _loader = loader;
            _comparer = comparer;
            _finder = finder;
            _mover = mover;
        }

        public async Task LoadImages(DirectoryInfo baseFolder)
        {
            Targets = await _loader.LoadImages(baseFolder);
        }

        public async Task FindSimilarImages(IEnumerable<IEntry> targets)
        {
            Pairs = await _finder.FindPairs(targets);
        }

        public void CompareImages(IEnumerable<Pair> pairs)
        {
            _comparer.Compare(pairs);
        }

        public void MoveImages(DirectoryInfo baseFolder, bool overwriteExistingFiles = true)
        {
            _mover.MoveImages(baseFolder, overwriteExistingFiles);
        }

        public int MovedImageCount => _comparer.Keep.Count + _comparer.Trash.Count + _comparer.Unsure.Count;
    }
}
