using DupFinderCore;

namespace DupFinderApp.ViewModels
{
    public class OptionsViewModel : VMBase
    {
        private bool checkDates = true;

        public bool CheckDates
        {
            get { return checkDates; }
            set { SetProperty(ref checkDates, value); }
        }

        private bool checkPixels = true;

        public bool CheckPixels
        {
            get { return checkPixels; }
            set { SetProperty(ref checkPixels, value); }
        }

        private bool checkSize = true;

        public bool CheckSize
        {
            get { return checkSize; }
            set { SetProperty(ref checkSize, value); }
        }

        public UserSettings GetSettings()
        {
            return new UserSettings()
            {
                CompareByDate = CheckDates,
                CompareBySize = CheckSize,
                CompareByPixels = CheckPixels
            };
        }

    }
}
