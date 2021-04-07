using Shipwreck.Phash;
using System;

namespace DupFinderCore
{
    public interface IEntry
    {
        int Pixels { get; init; }
        double AspectRatio { get; init; }
        int FocusLevel { get; set; }
        long Size { get; }

        string FullPath { get; }
        string Filename { get; }
        DateTime Date { get; }

        Digest Hash { get; }
    }
}
