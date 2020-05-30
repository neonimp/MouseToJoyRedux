using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Resources;
using System.Reflection;

namespace MouseToJoystick2
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
                var response = MouseToJoystick2.Properties.Resources.Licenses;
                License.Text = response;
            }
        }
    }
}
