using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DupFinderCore
{
    public interface IImagerComparer
    {
        List<Func<Image, Image, Image>> Rules { get; set; }
        Image GetBetterImage();
        void SetImages(Image left, Image right);
    }

    public class ImageComparer : IImagerComparer
    {
        public Image Left { get; private set; }
        public Image Right { get; private set; }

        private long leftSize;

        // cache value since it's probably expensive
        public long LeftSize
        {
            get
            {
                if (leftSize == default)
                {
                    leftSize = GetSize(Left);
                    return leftSize;
                }
                else
                {
                    return leftSize;
                }
            }
            set { leftSize = value; }
        }


        //a list of methods that take in two images as parameters and return an image
        public List<Func<Image, Image, Image>> Rules { get; set; } = new List<Func<Image, Image, Image>>();

        public ImageComparer()
        {
            Rules.Add(ComparePixels);
        }

        public void SetImages(Image left, Image right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            leftSize = default;
        }

        private Image ComparePixels(Image left, Image right)
        {
            if (Pixels(left) > Pixels(right))
            {
                return left;
            }
            else if (Pixels(left) < Pixels(right))
            {
                return right;
            }

            return left;
        }

        public Image GetBetterImage()
        {
            int leftWins = 0;
            int rightwins = 0;

            // we can add as many rules as we like when comparing images without modifying this class
            foreach (var rule in Rules)
            {
                if (rule(Left, Right) == Left)
                {
                    leftWins++;
                }
                else
                {
                    rightwins++;
                }
            }

            return leftWins > rightwins ? Left : Right;

            // return null if uncertain...
        }

        // https://stackoverflow.com/questions/221345/how-to-get-the-file-size-of-a-system-drawing-image
        private long GetSize(Image image)
        {
            using var ms = new MemoryStream(); // estimatedLength can be original fileLength
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); // save image to stream in Jpeg format
            return ms.Length;
        }

        int Pixels(Image image) => image.Width * image.Height;

        double Aspect(Image image) => (double)image.Width / image.Height;

    }
}
