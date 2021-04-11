using System.Diagnostics;
using System.Windows;

namespace DupFinderApp.Views
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : Window
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void Label_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(e.Uri.AbsoluteUri);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
            e.Handled = true;
        }
    }
}
