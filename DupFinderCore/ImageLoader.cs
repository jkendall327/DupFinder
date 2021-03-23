using Microsoft.Extensions.Logging;
using System.Drawing;
using System.IO;

namespace DupFinderCore
{
    public class ImageLoader : IImageLoader
    {
        readonly ILogger<ImageLoader> _logger;

        public ImageLoader(ILogger<ImageLoader> logger)
        {
            _logger = logger;
        }

        public Image Load(string filepath)
        {
            try
            {
                return Image.FromFile(filepath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError($"File not found: {ex.FileName}. Error message: {ex.Message}. Error at {ex.TargetSite}. Stack: {ex.StackTrace}");

                throw;
            }
        }
    }
}
