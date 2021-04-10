namespace DupFinderCore
{
    public class ImagesLoadedProgress
    {
        public int TotalImages { get; set; }
        public int AmountDone { get; set; }
        public int PercentageDone => (int)(AmountDone * 100 / (double)TotalImages);
    }
}