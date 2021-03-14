using System.Runtime.InteropServices;

namespace OneDimensionalChess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Piece
    {
        public PieceType Type;
        public PieceColor Color;
    }
}
