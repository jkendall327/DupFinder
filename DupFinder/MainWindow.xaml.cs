using DupFinderCore;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IImageLoader _loader;

        public MainWindow(IImageLoader loader)
        {
            _loader = loader;

            InitializeComponent();

            testc();
        }

        private void testc()
        {
            _loader.Test();
        }
    }
}
