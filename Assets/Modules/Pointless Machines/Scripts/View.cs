using System;

namespace PointlessMachines
{
    [Flags]
    internal enum View
    {
        None = 0,
        Left = 1,
        Down = 2,
        Up = 4,
        Right = 8
    }
}
