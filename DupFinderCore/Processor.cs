using Serilog;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class Processor : IProcessor
    {
        readonly IImageSetLoader _loader;
        readonly ILogger _logger;

        public IEnumerable<Image> Targets { get; set; } = Enumerable.Empty<Image>();

        public List<(Image Left, Image Right)> Pairs { get; set; } = new();

        public Processor(IImageSetLoader loader, ILogger logger)
        {
            _loader = loader;
            _logger = logger;

        }

        public void AddTargets()
        {
            // todo: can this be asynchronous too?
            Targets = _loader.GetImages().ToList();
            _logger.Information("Images loaded.");
        }

        public async Task Process()
        {
            await foreach (var item in FindPairs(Targets))
            {
                Pairs.Add(item);
            }

            // we now have a list of pairs -- images which are similar to each other
        }

        private async IAsyncEnumerable<(Image, Image)> FindPairs(IEnumerable<Image> images)
        {
            /*
             * take in two images
             * compare hashes etc
             * decide if they're pairs
             * if so, add to pairs enumerable
             * do this for every image in database
             */

            /*
             * use shipwreck.phash to get image similarity score
             * if image similarity score is high (above 86 in imgrefinery)
             * compare euclidian distances
             * 
             * if phash score is too low, do nothing, return nothing -- not a pair
             */

            var pairs = images.ToList().GetAllPairs();
            foreach ((Image left, Image right) pair in pairs)
            {
                if (Similarity(pair.left, pair.right) < 86) continue;

                // todo understand euclidian distance and implement

                yield return pair;
            }
        }

        private float Similarity(Image original, Image compare)
        {
            var bitmap = (Bitmap)original;
            var hash1 = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());

            var bitmap2 = (Bitmap)compare;
            var hash2 = ImagePhash.ComputeDigest(bitmap2.ToLuminanceImage());

            return ImagePhash.GetCrossCorrelation(hash1, hash2);
        }

        public async Task Prune()
        {
            await JudgePairs();

            MovePairs();
        }

        private void MovePairs()
        {
            /*
             * Actually do something with the newly-judged pairs.
             * I.e. move them to the right folders.
             * Hide this behind an interface maybe.
             */

            throw new NotImplementedException();
        }

        private Task JudgePairs()
        {
            /*
             * Decide which of the two images to keep.
             * Give each image some tag to decide whether to
             * keep it, trash it, or do nothing with it (unsure).
             */

            throw new NotImplementedException();
        }
    }
}
