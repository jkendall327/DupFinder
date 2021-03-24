using DupFinderCore;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IProcessor _processor;

        public MainWindow(IProcessor processor)
        {
            _processor = processor;

            _processor.AddTargets();

            InitializeComponent();
        }
    }
}
