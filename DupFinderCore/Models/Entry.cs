using System;
using System.Drawing;
using System.IO;
using DupFinderCore.Interfaces;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;

namespace DupFinderCore.Models
{
    /// <inheritdoc cref="IEntry"/>
    public class Entry : IEntry
    {
        // image data
        public int Pixels { get; init; }
        public double AspectRatio { get; init; }
        public Map ColorMap { get; }
        public Map FocusedColorMap { get; }
        public Digest Hash { get; }

        // file data
        public FileInfo OriginalFile { get; set; }
        public string FullPath => OriginalFile.FullName;
        public string Filename => Path.GetFileName(FullPath);
        public string TruncatedFilename => Filename.Substring(0, 40) + "...";
        public long Size => OriginalFile.Length;
        public DateTime Date => OriginalFile.CreationTimeUtc;

        public Entry(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("File not found", Path.GetFileName(filepath));

            OriginalFile = new(filepath);

            using var image = Image.FromFile(filepath);

            Pixels = image.Width * image.Height;
            AspectRatio = image.Width / image.Height;

            Hash = ImagePhash.ComputeDigest(image.ToBitmap().ToLuminanceImage());

            ColorMap = new Map(image);

            // todo bad that this is hardcoded -- focus level should be variable...
            int increasedFocus = (int)(ColorMap.FocusLevel * 1.33d);
            var offset = (int)(increasedFocus * 0.166);
            FocusedColorMap = new Map(image, increasedFocus, offset);
        }

        public override string ToString() => Filename;
    }
}
