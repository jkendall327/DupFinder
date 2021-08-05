using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using DupFinderCore.Models;
using Microsoft.Extensions.Logging;

namespace DupFinderCore.Services
{
    /// <inheritdoc cref="IImageSetLoader"/> 
    public class ImageSetLoader
    {
        private readonly ILogger<ImageSetLoader> _logger;

        public ImageSetLoader(ILogger<ImageSetLoader> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<IEntry> LoadImagesAsync(DirectoryInfo directory)
        {
            foreach (FileInfo file in GetFiles(directory))
            {
                yield return await MakeEntryAsync(file);
            }
        }

        private async Task<IEntry> MakeEntryAsync(FileInfo fi)
        {
            Entry entry = await Task.Run(() => new Entry(fi.FullName));
            _logger.LogDebug("Created new entry from {EntryName}", fi.Name);

            return entry;
        }

        private IEnumerable<FileInfo> GetFiles(DirectoryInfo directory)
        {
            try
            {
                ParallelQuery<FileInfo>? files = directory
                    .EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.IsImage())
                    .AsParallel();

                _logger.LogDebug("Loaded {FileCount} files from {DirectoryName}.", files.Count(), directory.FullName);

                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when processing {DirectoryName}.", directory.Name);
                return Enumerable.Empty<FileInfo>();
            }
        }
    }
}
