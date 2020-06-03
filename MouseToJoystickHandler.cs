using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using vJoyInterfaceWrap;
using DirectInputWrapper;

namespace MouseToJoyRedux
{
    public struct M2JConfig
    {
        internal uint VjoyDevId { get; set; }
        internal bool InvertX { get; set; }
        internal bool InvertY { get; set; }
        internal bool AutoCenter { get; set; }
        internal bool AutoSize { get; set; }
        internal int ManualWidth { get; set; }
        internal int ManualHeight { get; set; }
        internal double SenseX { get; set; }
        internal double SenseY { get; set; }
        internal bool LeftJoy { get; set; }

        public M2JConfig(uint vjoyDevId, bool invertX, bool invertY, bool autoCenter, bool autoSize, int manualWidth, int manualHeight, double senseX, double senseY, bool leftJoy = true)
        {
            VjoyDevId = vjoyDevId;
            InvertX = invertX;
            InvertY = invertY;
            AutoCenter = autoCenter;
            AutoSize = autoSize;
            ManualWidth = manualWidth;
            ManualHeight = manualHeight;
            SenseX = senseX;
            SenseY = senseY;
            LeftJoy = leftJoy;
        }

        public override string ToString()
        {
            return $"VjoyDevId: {VjoyDevId}\n" +
                $"InvertX: {InvertX}\n" +
                $"InvertY: {InvertY}\n" +
                $"AutoCenter: {AutoCenter}\n" +
                $"AutoSize: {AutoSize}\n" +
                $"ManualWidth: {ManualWidth}\n" +
                $"ManualHeight: {ManualHeight}\n" +
                $"sense(X,Y): {SenseX},{SenseY}\n" +
                "Joy: " + (LeftJoy ? "left" : "right");
        }
    }

    public struct KeyBindings
    {
        internal DirectKeyCombo DirectCombo { get; set; }
    }

    internal sealed class MouseToJoystickHandler : IDisposable
    {
        #region class_vars_declr

        private readonly int _invertX;
        private readonly int _invertY;
        private readonly double _senseX;
        private readonly double _senseY;
        private readonly bool _autoCenter;
        private readonly bool _autoSize;
        private readonly int _manualWidth;
        private readonly int _manualHeight;
        private readonly bool _leftJoy;

        // MouseKeyHook stuff
        private IKeyboardMouseEvents _eventHooker = null;

        private int _lastX;
        private int _lastY;

        // vJoy stuff
        private vJoy _joystick = null;
        private readonly uint _id;

        private readonly long _axisMax;
        private readonly long _axisMin;
        private readonly long _axisMid;

        private const uint VjoyBtn1 = 1;
        private const uint VjoyBtn2 = 2;
        private const uint VjoyBtn3 = 3;
        #endregion

        public MouseToJoystickHandler(M2JConfig cfgObj)
        {
            this._id = cfgObj.VjoyDevId;
            this._invertX = cfgObj.InvertX ? -1 : 1;
            this._invertY = cfgObj.InvertY ? -1 : 1;
            this._senseX = cfgObj.SenseX == 0 ? 0.1 : cfgObj.SenseX;
            this._senseY = cfgObj.SenseY == 0 ? 0.1 : cfgObj.SenseY;
            this._autoCenter = cfgObj.AutoCenter;
            this._autoSize = cfgObj.AutoSize;
            this._manualWidth = cfgObj.ManualWidth;
            this._manualHeight = cfgObj.ManualHeight;
            this._leftJoy = cfgObj.LeftJoy;

            _joystick = new vJoy();

            // Make sure driver is enabled
            if (!_joystick.vJoyEnabled())
            {
                throw new InvalidOperationException("vJoy driver not enabled: Failed Getting vJoy attributes");
            }

            // Make sure we can get the joystick
            VjdStat status = _joystick.GetVJDStatus(_id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                case VjdStat.VJD_STAT_FREE:
                    break;

                case VjdStat.VJD_STAT_BUSY:
                    throw new InvalidOperationException("vJoy device is already owned by another feeder");

                case VjdStat.VJD_STAT_MISS:
                    throw new InvalidOperationException("vJoy device is not installed or is disabled");

                default:
                    throw new Exception("vJoy device general error");
            };

            if (!this._joystick.AcquireVJD(this._id))
            {
                throw new Exception("Failed to acquire vJoy device");
            }

            if (!this._joystick.ResetVJD(this._id))
            {
                throw new Exception("Failed to reset vJoy device");
            }

            if (_leftJoy)
            {
                if (!this._joystick.GetVJDAxisMax(this._id, HID_USAGES.HID_USAGE_X, ref this._axisMax))
                {
                    throw new Exception("Failed to get vJoy axis max");
                }

                if (!this._joystick.GetVJDAxisMin(this._id, HID_USAGES.HID_USAGE_X, ref this._axisMin))
                {
                    throw new Exception("Failed to get vJoy axis min");
                }
            }
            else
            {
                if (!this._joystick.GetVJDAxisMax(this._id, HID_USAGES.HID_USAGE_RX, ref this._axisMax))
                {
                    throw new Exception("Failed to get vJoy axis max");
                }

                if (!this._joystick.GetVJDAxisMin(this._id, HID_USAGES.HID_USAGE_RX, ref this._axisMin))
                {
                    throw new Exception("Failed to get vJoy axis min");
                }
            }

            this._axisMid = _axisMax - (_axisMax - _axisMin) / 2;

            // Register for mouse and keyboard events
            _eventHooker = Hook.GlobalEvents();
            _eventHooker.MouseMove += HandleMouseMove;
            _eventHooker.MouseDown += HandleMouseDown;
            _eventHooker.MouseUp += HandleMouseUp;
        }

        public static uint[] GetActiveJoys()
        {
            VjdStat status;
            List<uint> availableDevs = new List<uint>();
            var joystick = new vJoy();


            for (uint id = 0; id < 16; id++)
            {
                status = joystick.GetVJDStatus(id);
                if (status == VjdStat.VJD_STAT_FREE)
                {
                    availableDevs.Add(id);
                }
            }

            return availableDevs.ToArray();
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            uint btnId;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    btnId = VjoyBtn1;
                    break;

                case MouseButtons.Right:
                    btnId = VjoyBtn2;
                    break;

                case MouseButtons.Middle:
                    btnId = VjoyBtn3;
                    break;

                default:
                    return;
            }

            this._joystick.SetBtn(true, this._id, btnId);
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            uint btnId;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    btnId = VjoyBtn1;
                    break;

                case MouseButtons.Right:
                    btnId = VjoyBtn2;
                    break;

                case MouseButtons.Middle:
                    btnId = VjoyBtn3;
                    break;

                default:
                    return;
            }

            this._joystick.SetBtn(false, this._id, btnId);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            var bounds = Screen.PrimaryScreen.Bounds;

            var minX = bounds.Left;
            var maxX = this._autoSize ? bounds.Right : (bounds.Left + this._manualWidth);

            var minY = bounds.Top;
            var maxY = this._autoSize ? bounds.Bottom : (bounds.Top + this._manualHeight);

            var deltaX = e.X - this._lastX;
            var deltaY = e.Y - this._lastY;
            this._lastX = this._autoCenter ? Clamp<int>(minX, e.X, maxX) : (minX + (maxX - minX) / 2);
            this._lastY = this._autoCenter ? Clamp<int>(minY, e.Y, maxY) : (minY + (maxY - minY) / 2);

            int xOut, yOut;
            if (this._autoCenter)
            {
                xOut = Clamp<int>(Convert.ToInt32(_axisMin), (int)Math.Round(_axisMid + _invertX * (deltaX * (deltaX * -1.0 / 1.1 + 500))), Convert.ToInt32(_axisMax));
                yOut = Clamp<int>(Convert.ToInt32(_axisMin), (int)Math.Round(_axisMid + _invertY * (deltaY * (deltaY * -1.0 / 1.1 + 500))), Convert.ToInt32(_axisMax));
            }
            else
            {
                var maxDeltaX = this._autoSize ? bounds.Width : this._manualWidth;
                var maxDeltaY = this._autoSize ? bounds.Height : this._manualHeight;
                var outputPerDeltaX = (_axisMax - _axisMin) / maxDeltaX;
                var outputPerDeltaY = (_axisMax - _axisMin) / maxDeltaY;
                xOut = Clamp<int>(Convert.ToInt32(_axisMin), (int)(_axisMid + _invertX * (deltaX + _senseX) * outputPerDeltaX), Convert.ToInt32(_axisMax));
                yOut = Clamp<int>(Convert.ToInt32(_axisMin), (int)(_axisMid + _invertY * (deltaY + _senseY) * outputPerDeltaY), Convert.ToInt32(_axisMax));
            }

            if (_leftJoy)
            {
                _joystick.SetAxis(xOut, this._id, HID_USAGES.HID_USAGE_X);
                _joystick.SetAxis(yOut, this._id, HID_USAGES.HID_USAGE_Y);
            }
            else
            {
                _joystick.SetAxis(xOut, this._id, HID_USAGES.HID_USAGE_RX);
                _joystick.SetAxis(yOut, this._id, HID_USAGES.HID_USAGE_RY);
            }
        }

        private static T Clamp<T>(T min, T val, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                if (this._eventHooker != null)
                {
                    this._eventHooker.Dispose();
                    this._eventHooker = null;
                }

                // dispose managed state (managed objects).
                if (this._joystick != null)
                {
                    this._joystick.RelinquishVJD(this._id);
                    this._joystick = null;
                }
            }

            _disposedValue = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
