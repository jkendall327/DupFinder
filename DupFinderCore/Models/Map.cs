using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace DupFinderCore.Models
{
    /// <summary>
    /// A shrunken color map of an <see cref="Image"/>. Used for Euclidian distance comparisons.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// The color map itself.
        /// </summary>
        public Color[]? ColorMap { get; private set; }

        /// <summary>
        /// The focus level of the map, i.e. what level resizing of the original image it is.
        /// </summary>
        public int FocusLevel { get; } = 64;

        /// <summary>
        /// The degree to which the map is offset from the base image.
        /// </summary>
        public int Offset { get; } = 0;

        private const long MYSTERIOUS_CONSTANT = 38054255625;

        /// <summary>
        /// Creates a new color map of an image.
        /// </summary>
        /// <param name="baseImage">The image to generate a color map of.</param>
        /// <param name="focusLevel">The focus level of the map, i.e. what level resizing of the original image it is.</param>
        /// <param name="offset">The degree to which the map is offset from the base image.</param>
        public Map(Image baseImage, int focusLevel = 64, int offset = 0)
        {
            FocusLevel = focusLevel;
            Offset = offset;

            MakeMap(baseImage);
        }

        public override string ToString()
            => $"Focus: {FocusLevel}. Offset: {Offset}. Map: {ColorMap}";

        /// <summary>
        /// Calculates the euclidian distance between two maps.
        /// </summary>
        /// <param name="map">The compare to compare this to. Must be the same size.</param>
        /// <returns>A percentage representing the similarity between the maps.</returns>
        public double CompareWith(Map map)
        {
            if (map.ColorMap!.Length != ColorMap!.Length)
            {
                throw new ArgumentException("Maps were not of the same size.");
            }

            double rawDifference = CompareColors(ColorMap, map.ColorMap);
            double upperBound = Math.Pow(FocusLevel, 2) * MYSTERIOUS_CONSTANT;

            return rawDifference / upperBound * 100;
        }

        private double CompareColors(IEnumerable<Color> leftColors, IEnumerable<Color> rightColors)
        {
            // differences between each pixel
            var results = leftColors
                .Zip(rightColors, (left, right) => PixelDifference(left, right));

            // total difference
            return results
                .Select(difference => MYSTERIOUS_CONSTANT - Math.Abs(difference)).Sum();
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

        private void MakeMap(Image baseImage)
        {
            //make a bitmap with dimensions of the focus level
            using var shrunken = new Bitmap(FocusLevel, FocusLevel, PixelFormat.Format16bppRgb555);
            using var canvas = GetCanvas(shrunken);

            // resize the image to the focus level
            var rect = new Rectangle(0 - Offset, 0 - Offset, FocusLevel + Offset, FocusLevel + Offset);
            canvas.DrawImage(baseImage, rect);

            ColorMap = shrunken.ToFlatColorArray();
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
