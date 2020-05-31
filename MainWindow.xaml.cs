using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MouseToJoystick2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MouseToJoystickHandler handler = null;

        public MainWindow()
        {
            InitializeComponent();
            var model = (MainWindowModel)this.DataContext;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
            DispatcherTimer dt = new DispatcherTimer();
            dt.Tick += Keep_themeSync;
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Start();
            var bounds = Screen.PrimaryScreen.Bounds;
            model.ScreenWidth = bounds.Width.ToString();
            model.ScreenHeight = bounds.Height.ToString();
        }

        private void Keep_themeSync(object sender, EventArgs e) => ThemeManager.Current.SyncTheme();

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
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
                    this.ShowModalMessageExternal("Error", err.Message);
                    // System.Windows.Forms.MessageBox.Show(err.Message);
                    model.ShouldRun = false;
                    model.SettingsEnabled = true;
                    this.start_btn.IsChecked = false;
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

            this.start_btn.Content = (bool)this.start_btn.IsChecked ? "Stop" : "Start";
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
