﻿using Shipwreck.Phash;
using System;
using System.Drawing;

namespace DupFinderCore
{
    /// <summary>
    /// Contains relevant information about an image: file info, hash, colourmap, size, etc.
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// The image's number of pixels.
        /// </summary>
        int Pixels { get; init; }

        /// <summary>
        /// The image's aspect ratio.
        /// </summary>
        double AspectRatio { get; init; }

        /// <summary>
        /// The image's focus level.
        /// </summary>
        int FocusLevel { get; set; }

        /// <summary>
        /// The image's filesize on disk.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// The path to the image's location on disk.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// The image's filename.
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// The date of the image's creation.
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        /// The <see cref="ImagePhash"/> digest of the image. Used to calculate similarity to other <see cref="IEntry"/> items.
        /// </summary>
        Digest Hash { get; }

        /// <summary>
        /// The entry's color map. Used for comparing Euclidian distances.
        /// </summary>
        Image ColorMap { get; }

        Image GetColorMap(Image baseImage, int focusLevel, bool crop = false);
    }
}
