using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class ImageSetLoader : IImageSetLoader
    {
        readonly ILogger _logger;

        public ImageSetLoader(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Entry>> GetImages(DirectoryInfo dirInfo)
        {
            var images = new ConcurrentBag<Entry>();

            ParallelQuery<FileInfo> files;

            try
            {
                // todo profile to see if this actually makes a difference -- overhead might make it slower
                files = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).AsParallel();
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Error($"Directory not found: {dirInfo.Name}");
                return images;
            }

            if (!files.Any())
            {
                _logger.Debug($"{dirInfo.Name} was empty.");
                return images;
            }

            _logger.Debug($"Loading files from {dirInfo.FullName}...");

            await Task.Run(() => Parallel.ForEach(files, file =>
            {
                if (IsNotImage(file)) return;

                _logger.Debug($"Loaded file {file.Name}");
                images.Add(new Entry(file.FullName));
            }));

            return images;
        }

        // todo improve
        private bool IsNotImage(FileInfo file)
            => !Regex.IsMatch(file.FullName, @".jpg|.png|.jpeg$", RegexOptions.IgnoreCase) || file.FullName.Contains(".txt");
    }
}
