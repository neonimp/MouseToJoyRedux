using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MouseToJoystick2
{
    public partial class MainWindow : MetroWindow
    {
        private MouseToJoystickHandler handler = null;
        private readonly uint[] availableJoys;

        public MainWindow()
        {
            InitializeComponent();
            PrintMessage($"Please send this log if you need help.\nVersion {typeof(MainWindow).Assembly.GetName().Version}");
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
            PrintMessage($"Width(detected): {model.ScreenWidth}, Height(detected): {model.ScreenHeight}");

            availableJoys = MouseToJoystickHandler.GetActiveJoys();
            foreach (var joy in availableJoys)
            {
                PrintMessage($"Found device with id: {joy}");
                vJoyDeviceInput.Items.Add(joy);
            }
        }

        private void Keep_themeSync(object sender, EventArgs e) => ThemeManager.Current.SyncTheme();
        private void PrintMessage(string message) => LogBox.Text += message + "\n";

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
                    M2JConfig cfg = new M2JConfig
                    {
                        VjoyDevId = deviceId,
                        InvertX = model.InvertX,
                        InvertY = model.InvertY,
                        AutoCenter = model.AutoCenter,
                        AutoSize = model.AutoScreenSize,
                        ManualWidth = manualWidth,
                        ManualHeight = manualHeight,
                        LeftJoy = model.LeftJoy
                    };
                    handler = new MouseToJoystickHandler(cfg);
                    PrintMessage($"Aquiring device with properties: {cfg}");
                    model.SettingsEnabled = false;
                }
                catch (Exception err)
                {
                    this.ShowModalMessageExternal("Error", err.Message);
                    PrintMessage($"{err}");
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => new OSSInfoWindow().Show();
        private void XSense_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => XSensePresc.Value = XSense.Value;
        private void YSense_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => YSensePresc.Value = YSense.Value;
        private void XSensePresc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => XSense.Value = XSensePresc.Value == null ? 0 : (double)XSensePresc.Value;
        private void YSensePresc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => YSense.Value = YSensePresc.Value == null ? 0 : (double)YSensePresc.Value;

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
