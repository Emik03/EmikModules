// SPDX-License-Identifier: MPL-2.0
using System;
using Wawa.IO;
using Wawa.Optionals;

namespace OneDimensionalChess
{
    static class Engine
    {
        internal const string LibraryName = "rustmate";

        static readonly Func<string, int, bool, CGameResult> _calculate =
            PathFinder
               .GetUnmanaged<Func<string, int, bool, CGameResult>>(LibraryName, "best_move")
               .UnwrapOr((_, __, ___) => { throw new DllNotFoundException("best_move"); });

        static readonly Func<string, sbyte, sbyte, bool> _isLegalMove =
            PathFinder
               .GetUnmanaged<Func<string, sbyte, sbyte, bool>>(LibraryName, "legality")
               .UnwrapOr((_, __, ___) => { throw new DllNotFoundException("legality"); });

        internal static Func<string, int, bool, CGameResult> Calculate
        {
            get
            {
                return _calculate;
            }
        }

        internal static Func<string, sbyte, sbyte, bool> IsLegalMove
        {
            get
            {
                return _isLegalMove;
            }
        }
    }
}
