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
        readonly IImagerComparer _comparer;

        public IEnumerable<Image> Targets { get; set; } = Enumerable.Empty<Image>();

        public List<(Image Left, Image Right)> Pairs { get; set; } = new();

        public Processor(IImageSetLoader loader, ILogger logger, IImagerComparer comparer)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public void AddTargets()
        {
            // todo: can this be asynchronous too?
            Targets = _loader.GetImages();
            _logger.Information("Images loaded.");
        }

        public async Task Process()
        {
            // tood: this should probably all be parallel not async...
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

        readonly IEnumerable<Image> Keep = Enumerable.Empty<Image>();
        readonly IEnumerable<Image> Trash = Enumerable.Empty<Image>();
        readonly IEnumerable<Image> Unsure = Enumerable.Empty<Image>();

        public void Prune()
        {
            foreach (var pair in Pairs)
            {
                _comparer.SetImages(pair.Left, pair.Right);
                // maybe give the ienumerables to the comparer as well
                var winner = _comparer.GetBetterImage();

                if (winner is null)
                {
                    Unsure.Append(pair.Left); Unsure.Append(pair.Right);
                    continue;
                }

                Keep.Append(winner);
                // how to get trashed image? give enumerables to comparer...
                // it should tell you what images to move and keep, not the processor
            }

            MovePairs();
        }

        private void MovePairs()
        {
            /*
             * Actually do something with the newly-judged pairs.
             * I.e. move them to the right folders.
             * Hide this behind an interface maybe.
             */

            foreach (var image in Keep)
            {
                // move to folder...
            }
        }
    }
}
