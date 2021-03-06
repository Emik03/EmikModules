using UnityEngine;

namespace Netherite
{
    internal static class Sounds
    {
        internal static string Dig { get { return "NetheriteDig" + Random.Range(1, 6); } }
        internal static string Hit { get { return "NetheriteHit" + Random.Range(1, 6); } }

        internal const string Ping = "NetheritePing",
            Solve = "NetheriteSolve";
    }
}
