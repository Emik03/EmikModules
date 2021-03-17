using System.Runtime.InteropServices;
using System;

namespace OneDimensionalChess
{
    internal static class Engine
    {
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CGameResult best_move(String state, Int32 max_moves, Boolean white_first);
        internal static Func<string, int, bool, CGameResult> Calculate { get { return best_move; } }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Boolean legality(String position, SByte origin, SByte destination);
        internal static Func<string, sbyte, sbyte, bool> IsLegalMove { get { return legality; } }

        internal const string LibraryName = "rustmate";
    }
}
