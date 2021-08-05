using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupFinderCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace DupFinderCore.Services
{
    public class Mover
    {
        private readonly IImageComparer _comparer;
        private readonly ILogger<Mover> _logger;

        public bool OverwriteExistingFiles { get; set; }
        public int MovedImageCount => _comparer.Keep.Count + _comparer.Trash.Count + _comparer.Unsure.Count;

        public Mover(IImageComparer comparer, ILogger<Mover> logger)
        {
            _comparer = comparer;
            _logger = logger;
        }

        public void MoveImages(DirectoryInfo baseFolder)
        {
            string basePath = baseFolder?.FullName + Path.DirectorySeparatorChar;

            var collectionsAndPaths = new List<(List<IEntry>, string)>()
            {
                (_comparer.Keep, "Keep"),
                (_comparer.Trash, "Trash"),
                (_comparer.Unsure, "Unsure")
            };

            foreach ((List<IEntry> collection, string path) pair in collectionsAndPaths)
            {
                DirectoryInfo destination = Directory.CreateDirectory(basePath + pair.path);
                Move(pair.collection, destination);
            }
        }

        private void Move(IEnumerable<IEntry> images, DirectoryInfo destination)
        {
            foreach (var image in images)
            {
                if (!File.Exists(image.FullPath))
                {
                    _logger.LogWarning("Image {ImagePath} not found.", image.FullPath);
                    continue;
                }

                string destinationPath = destination.FullName + Path.DirectorySeparatorChar + image.Filename;

                File.Move(image.FullPath, destinationPath, OverwriteExistingFiles);
            }
        }
    }
}
