using System;
using System.Globalization;
using System.Windows;

namespace MouseToJoystick2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MouseToJoystickHandler handler = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the button text
            if ((string)this.start_btn.Content == "Run")
            {
                this.start_btn.Content = "Stop";
            }
            else if((string)this.start_btn.Content == "Stop")
            {
                this.start_btn.Content = "Run";
            }
            
            var model = (MainWindowModel)this.DataContext;

            if (model.ShouldRun == true)
            {
                uint deviceId = Convert.ToUInt32(model.DeviceId);
                int manualWidth = Convert.ToInt32(model.ScreenWidth);
                int manualHeight = Convert.ToInt32(model.ScreenHeight);
                try
                {
                    handler = new MouseToJoystickHandler(deviceId, model.InvertX, model.InvertY, model.AutoCenter, model.AutoScreenSize, manualWidth, manualHeight);
                    model.SettingsEnabled = false;
                }
                catch (Exception err)
                {
                    Console.WriteLine("Whoops!");
                    System.Windows.Forms.MessageBox.Show(err.Message);
                    model.ShouldRun = false;
                }
            }
            else
            {
                if (this.handler != null)
                {
                    this.handler.Dispose();
                    this.handler = null;
                }
                model.SettingsEnabled = true;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            new OSSInfoWindow().Show();
        }

        private void XSense_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.XSensePresc.Text = XSense.Value.ToString("#.##", CultureInfo.InvariantCulture);
        }

        private void YSense_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.YSensePresc.Text = YSense.Value.ToString("#.##", CultureInfo.InvariantCulture);
        }
    }
}
