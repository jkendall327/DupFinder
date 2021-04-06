using DupFinderApp.ViewModels;
using System.Windows;

namespace DupFinderApp
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        public OptionsView()
        {
            InitializeComponent();
            DataContext = new OptionsViewModel();
        }
    }
}
