using System.Runtime.InteropServices;

namespace OneDimensionalChess
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CGameResult
    {
        public sbyte Evaluation;
        public PieceMove SuggestedMove;
    }
}

