using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    /// <summary>
    /// Loads <see cref="Entry"/> items from a directory.
    /// </summary>
    public interface IImageSetLoader
    {
        /// <summary>
        /// Loads image files from a directory into memory as <see cref="Entry"/> items.
        /// </summary>
        /// <param name="directory">The directory to load images from.</param>
        /// <returns>A task that represents a collection of images to be loaded.</returns>
        Task<IEnumerable<Entry>> LoadImages(DirectoryInfo dir);
        Task<IEnumerable<IEntry>> LoadImages(DirectoryInfo baseFolder, IProgress<PercentageProgress> imageLoadProgress);
    }
}