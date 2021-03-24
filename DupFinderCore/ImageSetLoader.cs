using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DupFinderCore
{
    public class ImageSetLoader : IImageSetLoader
    {
        readonly ILogger _logger;
        readonly IImageLoader _loader;
        readonly IConfiguration _config;

        public ImageSetLoader(ILogger logger, IImageLoader loader, IConfiguration config)
        {
            _logger = logger;
            _loader = loader;
            _config = config;
        }

        public IEnumerable<Image> GetImages()
        {
            var directory = _config.GetSection("BaseFolder").Value;
            DirectoryInfo dirInfo = new(directory);

            ParallelQuery<FileInfo> files;

            try
            {
                files = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).AsParallel();
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Error($"Directory not found: {dirInfo.Name}");
                yield break;
            }

            if (!files.Any())
            {
                _logger.Debug($"{dirInfo.Name} was empty.");
            }

            _logger.Debug($"Loading files from {dirInfo.FullName}...");

            foreach (var file in files)
            {
                if (IsNotImage(file)) continue;

                _logger.Debug($"Loaded file {file.Name}");
                yield return _loader.Load(file.FullName);
            }
        }

        // todo improve
        private bool IsNotImage(FileInfo file)
            => !Regex.IsMatch(file.FullName, @".jpg|.png|.jpeg$", RegexOptions.IgnoreCase) || file.FullName.Contains(".txt");
    }
}
