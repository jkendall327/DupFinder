using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class ImageSetLoader : IImageSetLoader
    {
        readonly ILogger _logger;
        public ImageSetLoader(ILogger logger) => _logger = logger;

        public async Task<IEnumerable<Entry>> GetImages(DirectoryInfo directory)
        {
            var tasks = GetFiles(directory)
                .Where(x => x.Exists)
                .Where(x => IsImage(x))
                .Select(x => MakeEntryAsync(x));

            return await Task.WhenAll(tasks);
        }

        private IEnumerable<FileInfo> GetFiles(DirectoryInfo directory)
        {
            if (directory is null)
            {
                _logger.Warning($"{nameof(directory)} was null.");
                return Enumerable.Empty<FileInfo>();
            }

            ParallelQuery<FileInfo> files;

            try
            {
                files = directory
                    .EnumerateFiles("*.*", SearchOption.AllDirectories)
                    .AsParallel();
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.Error($"Directory not found: {directory.Name}");
                _logger.Error(ex.StackTrace);
                return Enumerable.Empty<FileInfo>();
            }
            catch (SecurityException ex)
            {
                _logger.Error($"Security exception when processing {directory.Name}.");
                _logger.Error(ex.StackTrace);
                return Enumerable.Empty<FileInfo>();
            }

            if (!files.Any())
            {
                _logger.Debug($"{directory.Name} was empty.");
                return Enumerable.Empty<FileInfo>();
            }

            _logger.Debug($"Loading files from {directory.FullName}...");

            return files;
        }

        private async Task<Entry> MakeEntryAsync(FileInfo file)
        {
            var entry = await Task.Run(() => new Entry(file.FullName));
            _logger.Debug($"Loaded file {file.Name}");
            return entry;
        }

        // todo improve
        private bool IsImage(FileInfo file)
            => Regex.IsMatch(file.FullName, @".jpg|.png|.jpeg$", RegexOptions.IgnoreCase);
    }
}
