using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static Crabtopus.Infrastructure.NativeMethods;

namespace Crabtopus.Infrastructure
{
    // Fix for windows 10 rect size: https://stackoverflow.com/a/34143777.
    // Display overlay on another application: https://stackoverflow.com/a/15715587.
    // Disable focus for window: https://stackoverflow.com/a/12628353.
    internal class Overlayer
    {
        private readonly string _processName;
        private readonly Window _window;
        private readonly OverlayPosition _windowPosition;
        private readonly WindowInteropHelper _wih;

        public Overlayer(string processName, Window window, OverlayPosition windowPosition)
        {
            _processName = processName;
            _window = window;
            _windowPosition = windowPosition;

            _wih = new WindowInteropHelper(_window);
            SetWindowLong(_wih.Handle, GWL_EXSTYLE, GetWindowLong(_wih.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        [SuppressMessage("Performance", "RCS1096:Use bitwise operation instead of calling 'HasFlag'.", Justification = "Enum.HasFlag is 'fixed' since .NET Core 2.1.")]
        public void Update()
        {
            Process process = Process.GetProcessesByName(_processName).FirstOrDefault();
            if (process != null)
            {
                IntPtr processWindowHandle = process.MainWindowHandle;
                GetWindowRect(processWindowHandle, out Rectangle rect);
                DwmGetWindowAttribute(processWindowHandle, DwmWindowAttribute.ExtendedFrameBounds, out Rectangle frame, Marshal.SizeOf(typeof(Rect)));
                IntPtr foregroundWindowHandle = GetForegroundWindow();

                // Show the overlay if the process is the foreground window or if _window is the foreground window.

                // TODO: needed in order to not have other overlays disapear
            SetWindowLong(_wih.Handle, GWL_EXSTYLE, GetWindowLong(_wih.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
                if (foregroundWindowHandle == processWindowHandle || foregroundWindowHandle == _wih.Handle)
                {
                    if (_windowPosition.HasFlag(OverlayPosition.Top))
                    {
                        _window.SetCurrentValue(Window.TopProperty, (double)rect.Top + (frame.Top - rect.Top));
                    }

                    if (_windowPosition.HasFlag(OverlayPosition.Bottom))
                    {
                        _window.SetCurrentValue(Window.TopProperty, rect.Bottom - _window.Height + (frame.Bottom - rect.Bottom));
                    }

                    if (_windowPosition.HasFlag(OverlayPosition.Left))
                    {
                        _window.SetCurrentValue(Window.LeftProperty, (double)rect.Left - (frame.Right - rect.Right));
                    }

                    if (_windowPosition.HasFlag(OverlayPosition.Right))
                    {
                        _window.SetCurrentValue(Window.LeftProperty, rect.Right - _window.Width - (frame.Left - rect.Left));
                    }

                    if (!_window.IsVisible)
                    {
                        _window.Show();
                    }
                }
                else
                {
                    _window.Hide();
                }
            }
            else
            {
                _window.Hide();
            }
        }
    }
}
