using System.Runtime.InteropServices;
using System;

namespace OneDimensionalChess
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGameResult
    {
        internal Piece Piece;
        internal SByte Origin;
        internal SByte Destination;
        internal SByte Evaluation;
    }
}
