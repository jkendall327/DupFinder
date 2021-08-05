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

        public async Task<ConcurrentBag<IEntry>> LoadImages(DirectoryInfo directory)
        {
            ConcurrentBag<IEntry> entries = new();

            IEnumerable<Task> tasks =
                GetFiles(directory)
                .Select(x =>
                {
                    void create()
                    {
                        entries.Add(new Entry(x.FullName));
                        _logger.LogDebug("Created new entry from {EntryName}", x.Name);
                    }

                    return Task.Run(create);
                });

            await Task.WhenAll(tasks);

            return entries;
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
