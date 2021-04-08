using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace DupFinderCore
{
    /// <inheritdoc cref="IEntry"/>
    public class Entry : IEntry, IDisposable
    {
        // image data
        Color[,] OriginalImage { get; set; };
        public int Pixels { get; init; }
        public double AspectRatio { get; init; }
        public int FocusLevel { get; set; } = 64;
        public Image ColorMap { get; set; }
        public Image FocusedColorMap => GetColorMap(Image, (int)(FocusLevel * 1.33d), true);
        public Digest Hash { get; }

        // file data
        public FileInfo OriginalFile { get; set; }
        public string FullPath => OriginalFile.FullName;
        public string Filename => Path.GetFileName(FullPath);
        public long Size => OriginalFile.Length;
        public DateTime Date => OriginalFile.CreationTimeUtc;


        public Entry(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("File not found", Path.GetFileName(filepath));

            OriginalFile = new(filepath);

            var img = Image.FromFile(FullPath);

            OriginalImage = img.ToColorArray();
            Pixels = img.Width * img.Height;
            AspectRatio = img.Width / img.Height;
            Hash = ImagePhash.ComputeDigest(img.ToBitmap().ToLuminanceImage());
            ColorMap = GetColorMap(img, FocusLevel);
        }

        public Image GetColorMap(Image baseImage, int focusLevel, bool crop = false)
        {
            var shrunken = new Bitmap(focusLevel, focusLevel, PixelFormat.Format16bppRgb555);

            using var canvas = Graphics.FromImage(shrunken);
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.HighQualityBilinear;
            canvas.SmoothingMode = SmoothingMode.HighQuality;

            var offset = crop ? (int)(focusLevel * 0.166) : 0;

            canvas.DrawImage(baseImage, 0 - offset, 0 - offset, focusLevel + offset, focusLevel + offset);

            return shrunken;
        }
        public override string ToString() => Filename;
    }
}
