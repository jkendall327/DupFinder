using Serilog;
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

            foreach (var image in images)
            {
                foreach (var comparison in images)
                {
                    if (image == comparison) continue;

                    if (new Random().Next() % 2 == 0)
                    {
                        yield return (image, comparison);
                    }
                }
            }
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
