using System.ComponentModel;

namespace DupFinderApp.ViewModels
{
    public class VMBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T backingField, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            //only send out update if the value is different
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(backingField, value)) return;

            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
