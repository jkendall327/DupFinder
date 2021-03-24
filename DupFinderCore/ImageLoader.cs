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
        }

        /// <summary>
        /// Loads an image from a path.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when file can't be found.</exception>
        /// <param name="filepath">Image's path on the hard drive.</param>
        /// <returns>The image loaded into memory.</returns>
        public Image Load(string filepath)
        {
            try
            {
                return Image.FromFile(filepath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.Error($"File {ex.FileName} skipped: file not found. Error message: {ex.Message}. Error at {ex.TargetSite}. Stack: {ex.StackTrace}");
                throw;
            }
        }
    }
}
