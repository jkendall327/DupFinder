using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace DupFinderCore.Image_Processors
{
    /// <summary>
    /// A shrunken color map of a <see cref="Image"/>. Used for Euclidian distance comparisons.
    /// </summary>
    public class Map
    {
        public Color[] ColorMap { get; }
        public int FocusLevel { get; } = 64;
        public int Offset { get; } = 0;

        private const long MYSTERIOUS_CONSTANT = 38054255625;

        // ctor where you set the focus level and offset

        public Map(Color[,] baseImage, int focusLevel = 64, int offset = 0)
        {
            FocusLevel = focusLevel;
            Offset = offset;

            ColorMap = Generate(baseImage);
        }

        public Map(Image baseImage, int focusLevel = 64, int offset = 0) :
            this(baseImage.ToColorArray(), focusLevel, offset)
        { }

        /// <summary>
        /// Returns the euclidian distance between two color maps.
        /// </summary>
        /// <param name="map"></param>
        /// <returns>A double representing the raw euclidian distance between the two maps</returns>
        public double CompareWith(Map map)
        {
            if (map.ColorMap.Length != this.ColorMap.Length)
            {
                throw new ArgumentException();
            }

            double rawDifference = CompareColors(ColorMap, map.ColorMap);
            var upperBound = Math.Pow(FocusLevel, 2) * MYSTERIOUS_CONSTANT;

            return (rawDifference / upperBound) * 100;
        }

        private double CompareColors(IEnumerable<Color> leftColors, IEnumerable<Color> rightColors)
        {
            // get differences between each pixel
            var results = leftColors.Zip(rightColors, (left, right) => PixelDifference(left, right));

            // calculate total difference
            return results.Select(difference => MYSTERIOUS_CONSTANT - Math.Abs(difference)).Sum();
        }

        private double PixelDifference(Color first, Color second)
        {
            double squaredDistance(byte first, byte second)
                => Math.Pow(first - second, 2.0);

            double red = squaredDistance(first.R, second.R);
            double green = squaredDistance(first.G, second.G);
            double blue = squaredDistance(first.B, second.B);

            return Math.Pow(red + green + blue, 2);
        }

        private Color[] Generate(Color[,] baseImage)
        {
            //make a bitmap with dimensions of the focus level - a compressed version
            var shrunken = new Bitmap(FocusLevel, FocusLevel, PixelFormat.Format16bppRgb555);
            using var canvas = GetCanvas(shrunken);

            var rect = new Rectangle(0 - Offset, 0 - Offset, FocusLevel + Offset, FocusLevel + Offset);
            canvas.DrawImage(baseImage.ToImage(), rect);

            return shrunken.ToFlatColorArray().ToArray();
        }

        private static Graphics GetCanvas(Bitmap shrunken)
        {
            var canvas = Graphics.FromImage(shrunken);
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.HighQualityBilinear;
            canvas.SmoothingMode = SmoothingMode.HighQuality;
            return canvas;
        }
    }
}
