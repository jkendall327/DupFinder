using Serilog;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                JudgePairs(pair.Left, pair.Right);
            }

            MovePairs();
        }

        private void JudgePairs(Image left, Image right)
        {
            int Pixels(Image image) => image.Width * image.Height;

            if (Pixels(left) > Pixels(right))
            {
                Keep.Append(left); Trash.Append(right);
                return;
            }

            if (GetSize(left) > GetSize(right))
            {
                Keep.Append(left); Trash.Append(right);
            }

            double Aspect(Image image) => (double)image.Width / image.Height;

            var aspectRatioDifference = Math.Abs(Aspect(left) - Aspect(right));
            var pixelDifference = Math.Abs(((double)Pixels(left) / Pixels(right)) - 1d);
            var sizeDifference = Math.Abs(((double)GetSize(left) / GetSize(right)) - 1d);

            if (Math.Abs(pixelDifference - sizeDifference) >= 1 || aspectRatioDifference > 0.05)
            {
                Unsure.Append(left); Unsure.Append(right);
            }

            // todo what if left is smaller than right etc.
            // todo put this into own class with a list of rules
            // that can be extended easily -- passing in funcs?
        }

        // https://stackoverflow.com/questions/221345/how-to-get-the-file-size-of-a-system-drawing-image
        private long GetSize(Image image)
        {
            using var ms = new MemoryStream(); // estimatedLength can be original fileLength
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); // save image to stream in Jpeg format
            return ms.Length;
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

            }

            throw new NotImplementedException();
        }
    }
}
