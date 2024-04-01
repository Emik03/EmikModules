// SPDX-License-Identifier: MPL-2.0
using System;
using Wawa.IO;
using Wawa.Optionals;

namespace OneDimensionalChess
{
    static class Engine
    {
        internal const string LibraryName = "rustmate";

        static Func<string, int, bool, CGameResult> _calculate;

        static Func<string, sbyte, sbyte, bool> _isLegalMove;

        internal static Func<string, int, bool, CGameResult> Calculate
        {
            get
            {
                return _calculate != null
                    ? _calculate
                    : PathFinder.GetUnmanaged<Func<string, int, bool, CGameResult>>(LibraryName, "best_move")
                       .Match(
                            x => _calculate = x,
                            _ => (__, ___, ____) => { throw new DllNotFoundException("best_move"); }
                        );
            }
        }

        internal static Func<string, sbyte, sbyte, bool> IsLegalMove
        {
            get
            {
                return _isLegalMove != null
                    ? _isLegalMove
                    : PathFinder.GetUnmanaged<Func<string, sbyte, sbyte, bool>>(LibraryName, "legality")
                       .Match(
                            x => _isLegalMove = x,
                            _ => (__, ___, ____) => { throw new DllNotFoundException("legality"); }
                        );
            }
        }
    }
}
