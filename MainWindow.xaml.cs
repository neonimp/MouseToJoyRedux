using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace MouseToJoyRedux
{
    public partial class MainWindow : MetroWindow
    {
        private MouseToJoystickHandler _handler;
        private readonly uint[] _availableJoys;
        private readonly Guid _runId;

        public MainWindow()
        {
            _runId = Guid.NewGuid();
            InitializeComponent();
            PrintMessage("Please send this log if you need help.\n" +
                         $"info: {typeof(MainWindow).Assembly.GetName().FullName}\n" +
                         $"{typeof(MainWindow).Assembly.GetName().Name}@{typeof(MainWindow).Assembly.GetName().ProcessorArchitecture}\n" +
                         $"{_runId:N}\n" +
                         "No personally identifiable information is included in these logs.");
            var model = (MainWindowModel)DataContext;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
            var dt = new DispatcherTimer();
            dt.Tick += Keep_themeSync;
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Start();

            var bounds = Screen.PrimaryScreen.Bounds;
            PrintMessage($"Width(detected): {bounds.Width}, Height(detected): {bounds.Height}");
            ScreenResLbl.Content = $"Width(detected): {bounds.Width}, Height(detected): {bounds.Height}";
            _availableJoys = MouseToJoystickHandler.GetActiveJoys();
            foreach (var joy in _availableJoys)
            {
                PrintMessage($"Found device(s) with id: {joy}");
                vJoyDeviceInput.Items.Add(joy);
            }
        }

        private static void Keep_themeSync(object sender, EventArgs e) => ThemeManager.Current.SyncTheme();
        private void PrintMessage(string message) => LogBox.Text += message + "\n";

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var model = (MainWindowModel)DataContext;

            if (model.ShouldRun == true)
            {
                var devIdAt = vJoyDeviceInput.Items[model.DeviceIdIndex];
                var deviceId = Convert.ToUInt32(devIdAt);
                var manualWidth = 0;
                var manualHeight = 0;
                try
                {
                    var cfg = new M2JConfig
                    {
                        VjoyDevId = deviceId,
                        InvertX = model.InvertX,
                        InvertY = model.InvertY,
                        AutoCenter = model.AutoCenter,
                        AutoSize = model.AutoScreenSize,
                        ManualWidth = manualWidth,
                        ManualHeight = manualHeight,
                        LeftJoy = model.LeftJoy,
                        SenseX = (double)XSensePresc.Value,
                        SenseY = (double)YSensePresc.Value
                    };
                    PrintMessage("---------------------------------------");
                    PrintMessage($"Acquiring device with properties: {cfg}");
                    _handler = new MouseToJoystickHandler(cfg);
                    model.SettingsEnabled = false;
                }
                catch (Exception err)
                {
                    this.ShowModalMessageExternal("Error", err.Message);
                    PrintMessage($"{err}");
                    model.ShouldRun = false;
                    model.SettingsEnabled = true;
                    StartBtn.IsChecked = false;
                }
            }
            else
            {
                if (_handler != null)
                {
                    _handler.Dispose();
                    _handler = null;
                }
                model.SettingsEnabled = true;
            }

            StartBtn.Content = StartBtn.IsChecked != null && (bool)StartBtn.IsChecked ? "Stop" : "Start";
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => new OSSInfoWindow().Show();
        private void XSense_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => XSensePresc.Value = XSense.Value;
        private void YSense_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => YSensePresc.Value = YSense.Value;
        private void XSensePresc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => XSense.Value = XSensePresc.Value ?? 0;
        private void YSensePresc_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e) => YSense.Value = YSensePresc.Value ?? 0;

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

        }

        private void SaveToFile_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                DefaultExt = ".log",
                SupportMultiDottedExtensions = true,
                FileName = $"{DateTime.UtcNow:yy-MM-dd}-{_runId.ToString().Substring(0, 8)}",
                AddExtension = true,
                Filter = @"log | *.log"
            };
            if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            using (var fd = File.CreateText(saveDialog.FileName))
            {
                fd.Write(LogBox.Text);
            }
        }
    }
}
