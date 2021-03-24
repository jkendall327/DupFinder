using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DupFinderCore
{
    public class Processor : IProcessor
    {
        public IEnumerable<Image> Targets { get; set; } = Enumerable.Empty<Image>();

        readonly IImageSetLoader _loader;
        readonly ILogger _logger;

        public Processor(IImageSetLoader loader, ILogger logger)
        {
            _loader = loader;
            _logger = logger;

        }

        public void AddTargets()
        {
            Targets = _loader.GetImages().ToList();
            _logger.Information("Images loaded.");
        }

        public void Process()
        {
            throw new NotImplementedException();
        }

        public void Prune()
        {
            throw new NotImplementedException();
        }
    }
}
