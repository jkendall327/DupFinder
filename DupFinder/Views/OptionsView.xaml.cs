using System.Windows;
using DupFinderApp.ViewModels;

namespace DupFinderApp
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        private readonly OptionsViewModel _vm;
        public OptionsView(OptionsViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }
    }
}
