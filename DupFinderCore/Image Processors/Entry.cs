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
    public class Entry : IEntry
    {
        // image data
        public int Pixels { get; init; }
        public double AspectRatio { get; init; }
        public int FocusLevel { get; set; } = 64;
        public Color[,] OriginalImage { get; set; }
        public Color[,] ColorMap { get; }
        public Color[,] FocusedColorMap { get; }
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

            using var img = Image.FromFile(filepath);

            OriginalImage = img.ToColorArray();

            Pixels = img.Width * img.Height;
            AspectRatio = img.Width / img.Height;

            Hash = ImagePhash.ComputeDigest(img.ToBitmap().ToLuminanceImage());

            ColorMap = GetMap(img, FocusLevel, 0);

            int increasedFocus = (int)(FocusLevel * 1.33d);
            var offset = (int)(increasedFocus * 0.166);
            FocusedColorMap = GetMap(img, increasedFocus, offset);

        }

        private Color[,] GetMap(Image baseImage, int focusLevel, int offset = 0)
        {
            //make a bitmap with dimensions of the focus level - a compressed version
            var shrunken = new Bitmap(FocusLevel, FocusLevel, PixelFormat.Format16bppRgb555);
            using Graphics canvas = GetCanvas(shrunken);

            canvas.DrawImage(baseImage, 0 - offset, 0 - offset, focusLevel + offset, focusLevel + offset);

            return shrunken.ToColorArray();
        }

        private static Graphics GetCanvas(Bitmap shrunken)
        {
            var canvas = Graphics.FromImage(shrunken);
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.HighQualityBilinear;
            canvas.SmoothingMode = SmoothingMode.HighQuality;
            return canvas;
        }

        public override string ToString() => Filename;
    }
}
