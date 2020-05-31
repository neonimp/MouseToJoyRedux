using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MouseToJoystick2
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public int DeviceId { get; set; } = 1;
        public bool InvertX { get; set; } = false;
        public bool InvertY { get; set; } = false;
        public bool LeftJoy { get; set; } = false;
        public bool RightJoy { get; set; } = false;
        public double SenseLeft { get; set; } = 1;
        public double SenseRight { get; set; } = 1;
        public bool UseScroll { get; set; } = false;
        public bool? ShouldRun { get; set; } = false;
        public bool AutoScreenSize { get; set; } = true;
        public string ScreenWidth { get; set; } = "640";
        public string ScreenHeight { get; set; } = "480";
        public bool AutoCenter { get; set; } = true;
        public bool SettingsEnabled { get { return settingsEnabled; } set { settingsEnabled = value; NotifyPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool settingsEnabled = true;
    }
}
