using System;
using System.IO;
using System.Threading.Tasks;

namespace DupFinderCore
{
    /// <summary>
    /// A facade that exposes methods for loading, comparing and sorting <see cref="IEntry"/> items.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Loads <see cref="IEntry"/> items into memory from a folder on disk.
        /// </summary>
        /// <param name="baseFolder">The folder to search for images in.</param>
        /// <returns>A task representing the total number of images loaded into memory.</returns>
        Task<int> LoadImages(DirectoryInfo baseFolder);

        /// <summary>
        /// Finds pairs of similar images within the loaded <see cref="IEntry"/> items.
        /// </summary>
        /// <returns>A task representing the number of potential pairs found within the loaded images.</returns>
        Task<int> FindSimilarImages();
        Task<int> LoadImages(DirectoryInfo directoryInfo, IProgress<ImagesLoadedProgress> imageLoadProgress);

        /// <summary>
        /// Compares potential pairs to automatically sort them based on quality (size, pixels, etc.).
        /// Moves images to 'Better', 'Trash' or 'Unsure' folders based on their assessed quality.
        /// Original files are affected.
        /// </summary>
        /// <param name="settings">The user's settings indicating what image comparison criteria they want to apply.</param>
        void FindBetterImages();
    }
}
