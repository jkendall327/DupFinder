using DupFinderCore;

namespace DupFinderApp.ViewModels
{
    public class OptionsViewModel : VMBase
    {
        private readonly UserSettings _settings;

        public OptionsViewModel(UserSettings settings) => _settings = settings;

        private bool checkDates = true;
        public bool CheckDates
        {
            get => checkDates;
            set => SetProperty(ref checkDates, value);
        }

        private bool checkPixels = true;
        public bool CheckPixels
        {
            get => checkPixels;
            set => SetProperty(ref checkPixels, value);
        }

        private bool checkSize = true;
        public bool CheckSize
        {
            get => checkSize;
            set => SetProperty(ref checkSize, value);
        }

        public UserSettings GetSettings()
        {
            _settings.CompareByDate = CheckDates;
            _settings.CompareByPixels = CheckPixels;
            _settings.CompareBySize = CheckSize;

            return _settings;
        }
    }
}
