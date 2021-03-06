using UnityEngine;

namespace Coinage
{
    internal static class Sounds
    {
        internal static string Flip { get { return "Coinage" + Random.Range(1, 4); } }

        internal const string Solve = "CoinageSolve", 
            Strike = "CoinageStrike";
    }
}
