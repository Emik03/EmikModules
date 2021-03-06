using System.Runtime.InteropServices;

namespace OneDimensionalChess
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PieceMove
    {
        internal Piece Piece;
        internal sbyte Origin;
        internal sbyte Destination;
    }
}

