using KeepCodingAndNobodyExplodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WordTuple = KeepCodingAndNobodyExplodes.Tuple<Updog.Spelling, Updog.Casing>;

namespace Updog
{
    internal static class Words
    {
        internal static Dictionary<string, WordTuple> GetAll
        {
            get
            {
                return Helper.GetValues<Spelling>()
                    .SelectMany(s => Helper.GetValues<Casing>()
                    .Select(c => new WordTuple(s, c)))
                    .ToArray()
                    .ToDictionary(d => d.AssignCase());
            }
        }

        internal static KeyValuePair<string, WordTuple> GetRandom { get { return GetAll.PickRandom(); } }

        private static bool[] AsOrder(this string str, Casing casing)
        {
            return str.Select(c => c == ' ' ^ casing == Casing.Uppercase).ToArray();
        }

        internal static bool[] GetOrder(Color color, Casing casing)
        {
            switch (Colors.GetAll[color])
            {
                case Colors.Red: return "XXXX".AsOrder(casing);
                case Colors.Orange: return "XX X".AsOrder(casing);
                case Colors.Yellow: return "XXX ".AsOrder(casing);
                case Colors.Green: return "XX  ".AsOrder(casing);
                case Colors.Blue: return "X XX".AsOrder(casing);
                case Colors.Purple: return "X  X".AsOrder(casing);
                default: throw new NotImplementedException("Colors.GetAll[color]: " + Colors.GetAll[color]);
            }
        }

        private static string AssignCase(this WordTuple word)
        {
            if (word.Item2 == Casing.Lowercase)
                return word.Item1.ToString().ToLowerInvariant();
            if (word.Item2 == Casing.Uppercase)
                return word.Item1.ToString().ToUpperInvariant();
            throw new NotImplementedException("casing: " + word.Item2);
        }
    }
}
