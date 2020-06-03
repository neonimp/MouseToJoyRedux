using System.Net;
using System.Windows;

namespace MouseToJoyRedux
{
    /// <summary>
    /// Interaction logic for OSSInfoWindow.xaml
    /// </summary>
    public partial class OSSInfoWindow : Window
    {
        public OSSInfoWindow()
        {
            InitializeComponent();
            try
            {
                var response = new WebClient().DownloadString("https://cdn.cmdforge.net/pub/docs/jtmr-legal-complete.txt");
                License.Text = response;
            }
            catch (WebException)
            {
                var response = MouseToJoyRedux.Properties.Resources.Licenses;
                License.Text = response;
            }
        }
    }
}
