using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public static Color[] ToFlatColorArray(this Bitmap image)
        {
            List<Color> array = new();

            for (int i = 0; i < image.Width; i++)
            {
                for (int x = 0; x < image.Height; x++)
                {
                    array.Add(image.GetPixel(i, x));
                }
            }

            return array.ToArray();
        }

        public static Bitmap ToImage(this Color[,] arr)
        {
            // new bitmap of correct size
            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            var bmp = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(x, y, arr[x, y]);
                }
            }

            return bmp;
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

        //Source:
        //https://stackoverflow.com/questions/9354747/how-can-i-determine-if-a-file-is-an-image-file-in-net
        public static bool IsImage(this FileInfo file)
        {
            using var fs = new FileStream(file.FullName, FileMode.Open);

            int mostBytesNeeded = 11; //For JPEG

            if (fs.Length < mostBytesNeeded)
                return false;

            byte[] headerBytes = new byte[mostBytesNeeded];
            fs.Read(headerBytes, 0, mostBytesNeeded);

            //JPEG:
            if (headerBytes[0] == 0xFF &&//FF D8
                headerBytes[1] == 0xD8 &&
                (
                 (headerBytes[6] == 0x4A &&//'JFIF'
                  headerBytes[7] == 0x46 &&
                  headerBytes[8] == 0x49 &&
                  headerBytes[9] == 0x46)
                  ||
                 (headerBytes[6] == 0x45 &&//'EXIF'
                  headerBytes[7] == 0x78 &&
                  headerBytes[8] == 0x69 &&
                  headerBytes[9] == 0x66)
                ) &&
                headerBytes[10] == 00)
            {
                return true;
            }
            //PNG 
            if (headerBytes[0] == 0x89 && //89 50 4E 47 0D 0A 1A 0A
                headerBytes[1] == 0x50 &&
                headerBytes[2] == 0x4E &&
                headerBytes[3] == 0x47 &&
                headerBytes[4] == 0x0D &&
                headerBytes[5] == 0x0A &&
                headerBytes[6] == 0x1A &&
                headerBytes[7] == 0x0A)
            {
                return true;
            }
            //GIF
            if (headerBytes[0] == 0x47 &&//'GIF'
                headerBytes[1] == 0x49 &&
                headerBytes[2] == 0x46)
            {
                return true;
            }
            //BMP
            if (headerBytes[0] == 0x42 &&//42 4D
                headerBytes[1] == 0x4D)
            {
                return true;
            }
            //TIFF
            if ((headerBytes[0] == 0x49 &&//49 49 2A 00
                 headerBytes[1] == 0x49 &&
                 headerBytes[2] == 0x2A &&
                 headerBytes[3] == 0x00)
                 ||
                (headerBytes[0] == 0x4D &&//4D 4D 00 2A
                 headerBytes[1] == 0x4D &&
                 headerBytes[2] == 0x00 &&
                 headerBytes[3] == 0x2A))
            {
                return true;
            }

            return false;
        }
    }
}
