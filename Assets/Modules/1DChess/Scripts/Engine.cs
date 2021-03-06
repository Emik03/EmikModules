using System.Runtime.InteropServices;
using System;

namespace OneDimensionalChess
{
    internal static class Engine
    {
        [DllImport(Rustmate, CallingConvention = CallingConvention.Cdecl)]
        private static extern CGameResult best_move(string state, int limit, bool whiteFirst);
        internal static Func<string, int, bool, CGameResult> Calculate { get { return best_move; } }

        [DllImport(Rustmate, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool legality(string position, sbyte origin, sbyte destination);
        internal static Func<string, sbyte, sbyte, bool> IsLegalMove { get { return legality; } }

        private const string Rustmate = "rustmate";
    }
}
