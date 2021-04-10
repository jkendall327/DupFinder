using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace DupFinderCore
{
    /// <inheritdoc cref="IImageSetLoader"/> 
    public class ImageSetLoader
    {
        readonly ILogger _logger;
        public ImageSetLoader(ILogger logger) => _logger = logger;

        readonly ConcurrentBag<Entry> Entries = new();

        public async Task<IEnumerable<Entry>> LoadImages(DirectoryInfo directory)
        {
            var tasks = GetFiles(directory)
                .Where(file => file.Exists)
                .Where(file => file.IsImage())
                .Select(file => Task.Run(() => MakeEntry(file)));

            await Task.WhenAll(tasks);

            return Entries;
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
                    .EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
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

        private void MakeEntry(FileInfo file)
        {
            var entry = new Entry(file.FullName);
            _logger.Debug($"Loaded file {file.Name}");
            Entries.Add(entry);
        }
    }
}
