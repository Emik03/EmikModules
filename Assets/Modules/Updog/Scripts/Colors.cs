using System.Collections.Generic;
using UnityEngine;

namespace Updog
{
    internal static class Colors
    {
        internal static Dictionary<Color, string> GetAll
        {
            get
            {
                return new Dictionary<Color, string>
                {
                    { red, Red },
                    { orange, Orange },
                    { yellow, Yellow },
                    { green, Green },
                    { blue, Blue },
                    { purple, Purple },
                    { white, White },
                    { black, Black }
                };
            }
        }

        internal static Color[] GetFinal
        {
            get
            {
                return new[]
                {
                    GetRandom,
                    black,
                    GetRandomPrimary,
                    black,
                    GetRandomPrimary,
                    black,
                    GetRandom,
                    black,
                    black,
                    black
                };
            }
        }

        internal static Color GetRandom
        {
            get
            {
                return new[] { red, orange, yellow, green, blue, purple }
                    .PickRandom();
            }
        }

        internal static Color GetRandomPrimary
        {
            get
            {
                return new[] { red, green, blue }
                    .PickRandom();
            }
        }

        internal static readonly Color 
            red = new Color(0.4f, 0.05f, 0.05f),
            orange = new Color(0.4f, 0.3f, 0.05f),
            yellow = new Color(0.5f, 0.5f, 0.05f),
            blue = new Color(0.05f, 0.05f, 0.5f),
            green = new Color(0.05f, 0.5f, 0.05f),
            purple = new Color(0.5f, 0.05f, 0.5f),
            white = new Color(0.75f, 0.75f, 0.75f),
            black = new Color(0.05f, 0.05f, 0.05f);

        internal const string 
            Red = "Red", 
            Orange = "Orange",
            Yellow = "Yellow",
            Green = "Green", 
            Blue = "Blue", 
            Purple = "Purple",
            White = "Solve!",
            Black = "";
    }
}
