using System;
using System.Drawing;
using System.IO;

namespace DupFinderCore
{
    class Entry
    {
        public Image Image { get; set; }

        public string Path { get; set; }
        public int Pixels => Image.Width * Image.Height;
        public double AspectRatio => (double)Image.Width / Image.Height;

        public int FocusLevel = 64;

        public long Size;

        public DateTime Date;

        public Image ColorMap;

        public Entry(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException();
            }

            Path = filepath;
            Image = Image.FromFile(Path);

            Size = new FileInfo(Path).Length;

        }
    }
}
