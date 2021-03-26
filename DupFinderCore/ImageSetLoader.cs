using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DupFinderCore
{
    public class ImageSetLoader : IImageSetLoader
    {
        readonly ILogger _logger;
        readonly IConfiguration _config;

        public ImageSetLoader(ILogger logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<List<Image>> GetImages(DirectoryInfo dirInfo)
        {
            var images = new List<Image>();

            ParallelQuery<FileInfo> files;

            try
            {
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
                images.Add(Load(file.FullName));
            }));

            return images;
        }

        // todo improve
        private bool IsNotImage(FileInfo file)
            => !Regex.IsMatch(file.FullName, @".jpg|.png|.jpeg$", RegexOptions.IgnoreCase) || file.FullName.Contains(".txt");

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
