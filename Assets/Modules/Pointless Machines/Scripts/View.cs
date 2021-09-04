using System;

namespace PointlessMachines
{
    [Flags]
    internal enum View
    {
        None = 0,
        Left = 1,
        Down = 1 << 1,
        Up = 1 << 2,
        Right = 1 << 3
    }
}
