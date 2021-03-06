using System.Runtime.InteropServices;

namespace OneDimensionalChess
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Piece
    {
        public PieceType Type;
        public PieceColor Color;
    }
}
