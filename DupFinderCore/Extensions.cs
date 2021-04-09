using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DupFinderCore
{
    static class Extensions
    {
        // https://stackoverflow.com/questions/17031771/comparing-each-element-with-each-other-element-in-a-list

        /// <summary>
        /// Sorts a list of items into a list of its unique pairs, in tuples.
        /// </summary>
        public static IEnumerable<(T, T)> UniquePairs<T>(this IEnumerable<T> source)
            => source.SelectMany((_, i) => source.Where((_, j) => i < j), (x, y) => (x, y));

        public static Color[,] ToColorArray(this Image image) => ToColorArray((Bitmap)image);

        public static Color[,] ToColorArray(this Bitmap image)
        {
            Color[,] array = new Color[image.Width, image.Height];

            for (int i = 0; i < image.Width; i++)
            {
                for (int x = 0; x < image.Height; x++)
                {
                    array[i, x] = image.GetPixel(i, x);
                }
            }

            return array;
        }

        public static List<Color> Flatten(this Color[,] array)
        {
            List<Color> list = new();

            foreach (var pixel in array)
            {
                list.Add(pixel);
            }
            return list;
        }
    }
}
