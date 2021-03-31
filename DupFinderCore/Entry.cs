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
    public class Entry
    {
        public Image Image { get; }

        public string FullPath { get; }
        public string Filename => Path.GetFileName(FullPath);
        public int Pixels;
        public double AspectRatio;

        public int FocusLevel { get; set; } = 64;

        public long Size { get; }

        public DateTime Date { get; }

        public Image? ColorMap { get; set; }

        public Status Status { get; set; } = Status.Undecided;

        public override string ToString()
        {
            return FullPath;
        }

        /// <summary>
        /// Used for calculating similarity with PHash.
        /// </summary>
        public Digest Hash { get; }

        public Entry(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }

            FullPath = filepath;
            Image = Image.FromFile(FullPath);

            Pixels = Image.Width * Image.Height;
            AspectRatio = (double)Image.Width / Image.Height;

            FileInfo fileInfo = new(FullPath);

            Size = fileInfo.Length;
            Date = new FileInfo(FullPath).CreationTimeUtc;

            var bitmap = (Bitmap)Image;
            Hash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());

            GenerateColorMap();

            Image.Dispose();
        }

        public void GenerateColorMap(bool Crop = false)
        {
            var Shrunken = new Bitmap(FocusLevel, FocusLevel, PixelFormat.Format16bppRgb555);
            var Canvas = Graphics.FromImage(Shrunken);
            Canvas.CompositingQuality = CompositingQuality.HighQuality;
            Canvas.InterpolationMode = InterpolationMode.HighQualityBilinear;
            Canvas.SmoothingMode = SmoothingMode.HighQuality;

            var offset = Crop ? (int)(FocusLevel * 0.166) : 0;
            Canvas.DrawImage(Image, 0 - offset, 0 - offset, FocusLevel + offset, FocusLevel + offset);

            ColorMap = Shrunken;

            Canvas.Dispose();
        }
    }
}
