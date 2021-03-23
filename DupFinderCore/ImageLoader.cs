using Serilog;
using System.Drawing;
using System.IO;

namespace DupFinderCore
{
    public class ImageLoader : IImageLoader
    {
        readonly ILogger _logger;

        public ImageLoader(ILogger logger)
        {
            _logger = logger;
            _logger.Information("Starting image loader");
        }

        public void Test()
        {
            _logger.Information("hello!");
        }

        public Image Load(string filepath)
        {
            try
            {
                return Image.FromFile(filepath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.Error($"File not found: {ex.FileName}. Error message: {ex.Message}. Error at {ex.TargetSite}. Stack: {ex.StackTrace}");

                throw;
            }
        }
    }
}
