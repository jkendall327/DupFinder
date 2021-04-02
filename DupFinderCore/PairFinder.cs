using Shipwreck.Phash;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public interface IPairFinder
    {
        Task<IEnumerable<(Entry, Entry)>> FindPairs(IEnumerable<Entry> images);
    }

    public class PairFinder : IPairFinder
    {
        public async Task<IEnumerable<(Entry, Entry)>> FindPairs(IEnumerable<Entry> images)
        {
            var uniquePairs = images.GetAllUniquePairs().ToList();

            var similarImages = new ConcurrentBag<(Entry, Entry)>();

            await Task.Run(() => Parallel.ForEach(uniquePairs, pair =>
            {
                if (ImagesAreSimilar(pair))
                    similarImages.Add(pair);
            }));

            return similarImages;
        }

        private bool ImagesAreSimilar((Entry, Entry) pair)
        {
            var left = pair.Item1;
            var right = pair.Item2;

            var phash = ImagePhash.GetCrossCorrelation(left.Hash, right.Hash);

            return phash > 0.86 && CompareEuclidianDistance(left, right);
        }

        private bool CompareEuclidianDistance(Entry left, Entry right)
        {
            return true;
        }

    }
}
