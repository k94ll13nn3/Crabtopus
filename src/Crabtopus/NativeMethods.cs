using System;
using System.Runtime.InteropServices;

namespace Crabtopus
{
    internal static class NativeMethods
    {
        public const int GWL_EXSTYLE = -20;

        public const int WS_EX_NOACTIVATE = 0x08000000;

        internal enum DwmWindowAttribute : uint
        {
            ExtendedFrameBounds = 9,
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern void GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("dwmapi.dll")]
        internal static extern void DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out Rectangle pvAttribute, int cbAttribute);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
