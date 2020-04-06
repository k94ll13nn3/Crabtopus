using System;

namespace Crabtopus
{
    /// <summary>
    /// Specify where the overlay window must be located.
    /// </summary>
    [Flags]
    internal enum OverlayPosition
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
    }
}
