namespace DupFinderCore
{
    public class ImagesLoadedProgress
    {
        public int TotalImages { get; set; }
        public int AmountDone { get; set; }
        public int PercentageDone => AmountDone * 100 / TotalImages;
    }
}