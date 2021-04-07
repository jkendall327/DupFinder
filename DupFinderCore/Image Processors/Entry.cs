using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DupFinderCore
{
    public enum Status
    {
        Undecided,
        Keep,
        Trash,
        Unsure
    }

    /// <summary>
    /// Wrapper around System.Drawing.Image that maintains file info, hash, colourmap, status, etc.
    /// </summary>
    public class Entry : IEntry
    {
        public Image Image { get; }
        public int Pixels { get; init; }
        public double AspectRatio { get; init; }
        public int FocusLevel { get; set; } = 64;
        public long Size { get; }

        public string FullPath { get; }
        public string Filename => Path.GetFileName(FullPath);

        public DateTime Date { get; }

        public Image? ColorMap { get; set; }

        public override string ToString() => FullPath;

        /// <summary>
        /// Used for calculating similarity with PHash.
        /// </summary>
        public Digest Hash { get; }

        public Entry(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("File not found", Path.GetFileName(filepath));

            FullPath = filepath;

            Image = Image.FromFile(FullPath);

            Pixels = Image.Width * Image.Height;
            AspectRatio = (double)Image.Width / Image.Height;

            FileInfo file = new(FullPath);
            Size = file.Length;
            Date = file.CreationTimeUtc;

            Hash = ImagePhash.ComputeDigest((Image as Bitmap).ToLuminanceImage());

            GenerateColorMap();

            Image.Dispose();
        }

        private void GenerateColorMap(bool crop = false)
        {
            var Shrunken = new Bitmap(FocusLevel, FocusLevel, PixelFormat.Format16bppRgb555);
            var Canvas = Graphics.FromImage(Shrunken);
            Canvas.CompositingQuality = CompositingQuality.HighQuality;
            Canvas.InterpolationMode = InterpolationMode.HighQualityBilinear;
            Canvas.SmoothingMode = SmoothingMode.HighQuality;

            var offset = crop ? (int)(FocusLevel * 0.166) : 0;
            Canvas.DrawImage(Image, 0 - offset, 0 - offset, FocusLevel + offset, FocusLevel + offset);

            ColorMap = Shrunken;

            Canvas.Dispose();
        }
    }
}
