using PhosphorescenceModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// Contains code that is needed repeatedly throughout.
/// </summary>
namespace PhosphorescenceModule
{
    internal static class Function
    {
        /// <summary>
        /// Used to take the individual characters from a user's submission and get the equivalent ButtonType.
        /// </summary>
        internal static readonly Dictionary<char, ButtonType> charToButton = new Dictionary<char, ButtonType>()
        {
            { 'a', ButtonType.Azure },
            { 'b', ButtonType.Blue },
            { 'c', ButtonType.Crimson },
            { 'd', ButtonType.Diamond },
            { 'e', ButtonType.Emerald },
            { 'f', ButtonType.Fuchsia },
            { 'g', ButtonType.Green },
            { 'h', ButtonType.Hazel },
            { 'i', ButtonType.Ice },
            { 'j', ButtonType.Jade },
            { 'k', ButtonType.Kiwi },
            { 'l', ButtonType.Lime },
            { 'm', ButtonType.Magenta },
            { 'n', ButtonType.Navy },
            { 'o', ButtonType.Orange },
            { 'p', ButtonType.Purple },
            { 'q', ButtonType.Quartz },
            { 'r', ButtonType.Red },
            { 's', ButtonType.Salmon },
            { 't', ButtonType.Tan },
            { 'u', ButtonType.Ube },
            { 'v', ButtonType.Vibe },
            { 'w', ButtonType.White },
            { 'x', ButtonType.Xotic },
            { 'y', ButtonType.Yellow },
            { 'z', ButtonType.Zen }
        };

        /// <summary>
        /// Stores all of the button types in an array.
        /// </summary>
        internal static IEnumerable<ButtonType> AllButtonTypes = Enum.GetValues(typeof(ButtonType)).Cast<ButtonType>();

        /// <summary>
        /// Plays a sound.
        /// </summary>
        /// <param name="sound">The name of the sound file.</param>
        internal static void PlaySound(this PhosphorescenceScript pho, string sound)
        {
            pho.Audio.PlaySoundAtTransform(sound, pho.transform);
        }

        /// <summary>
        /// ElasticIn ease that returns the current step.
        /// </summary>
        /// <param name="k">The current step. Generally in range of 0 to 1 (both inclusive).</param>
        /// <returns>The current step of the number provided.</returns>
        internal static float ElasticIn(float k)
        {
            return k % 1 == 0 ? k : -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
        }

        /// <summary>
        /// ElasticOut ease that returns the current step.
        /// </summary>
        /// <param name="k">The current step. Generally in range of 0 to 1 (both inclusive).</param>
        /// <returns>The current step of the number provided.</returns>
        internal static float ElasticOut(float k)
        {
            return k % 1 == 0 ? k : Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
        }

        /// <summary>
        /// Returns the character as lowercase.
        /// </summary>
        /// <typeparam name="T">The datatype of the variable.</typeparam>
        /// <param name="source">The variable to apply lowercase to.</param>
        /// <returns>The lowercase version of the character.</returns>
        internal static char ToLower<Tchar>(this Tchar source)
        {
            return source.ToString().ToLowerInvariant()[0];
        }

        /// <summary>
        /// Emulates a foreach loop that can be used inline alongside other Linq functions.
        /// </summary>
        /// <typeparam name="T">The datatype of the variable.</typeparam>
        /// <param name="source">The variable to apply.</param>
        /// <param name="act">The action to apply to the variable.</param>
        /// <returns>The modification of the variable after each element goes through the action provided.</returns>
        internal static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (T element in source) act(element);
            return source;
        }

        /// <summary>
        /// Converts a button type array to a string array.
        /// </summary>
        /// <typeparam name="T">The datatype of the variable.</typeparam>
        /// <param name="buttons">The array to parse with.</param>
        /// <returns>String array of names of ButtonType array provided.</returns>
        internal static string[] ToStringArray<T>(this ButtonType[] buttons)
        {
            return buttons.Select(b => b.ToString()).ToArray();
        }

        /// <summary>
        /// Inverts a boolean.
        /// </summary>
        /// <param name="boolean">The boolean to invert.</param>
        internal static void InvertBoolean(ref bool boolean)
        {
            boolean = !boolean;
        }

        /// <summary>
        /// Emulates a foreach loop that can be used inline alongside other Linq functions.
        /// </summary>
        /// <typeparam name="T">The datatype of the variable.</typeparam>
        /// <param name="source">The variable to apply.</param>
        /// <param name="act">The action to apply to the variable.</param>
        /// <returns>The modification of the variable after each element goes through the action provided.</returns>
        internal static Color32[] IterateColors(this IEnumerable<ButtonType> source)
        {
            List<Color32> output = new List<Color32>();
            foreach (ButtonType element in source)
                output.Add(GetColor(element));
            return output.ToArray();
        }

        /// <summary>
        /// Assigns KMSelectable.OnInteract event handlers. Reminder that your method should have only a single integer parameter, which will be used to pass the index of the button pressed.
        /// </summary>
        /// <param name="selectables">The array to create event handlers for.</param>
        /// <param name="method">The method that will be called whenever an event is triggered.</param>
        internal static void OnInteractArray(this KMSelectable[] selectables, Func<int, KMSelectable.OnInteractHandler> method)
        {
            for (int i = 0; i < selectables.Length; i++)
                selectables[i].OnInteract += method(i);
        }

        /// <summary>
        /// Mixes the two colors provided and sets the renderer.material.color to be that color. Weighting can be included.
        /// </summary>
        /// <param name="renderer">The renderer to change color. This does mean that the renderer's material must support color.</param>
        /// <param name="colorA">The first color, as f approaches 0.</param>
        /// <param name="colorB">The second color, as f approaches 1.</param>
        /// <param name="f">The weighting of color mixing, with 0 being 100% colorA and 1 being 100% colorB.</param>
        internal static void SetIntertwinedColor(this Renderer renderer, Color32 colorA, Color32 colorB, float f = 0.5f)
        {
            float negF = 1 - f;
            renderer.material.color = new Color32((byte)((colorA.r * negF) + (colorB.r * f)), (byte)((colorA.g * negF) + (colorB.g * f)), (byte)((colorA.b * negF) + (colorB.b * f)), 255);
        }

        /// <summary>
        /// Trims all strings in a jagged string array.
        /// </summary>
        /// <param name="source">The jagged array to trim all strings with.</param>
        /// <returns>The same instance of the jagged array, all of them trimmed.</returns>
        internal static string[][] TrimAll(this string[][] source)
        {
            for (int i = 0; i < source.Length; i++)
                for (int j = 0; j < source[i].Length; j++)
                    source[i][j] = source[i][j].Trim();

            return source;
        }

        /// <summary>
        /// Generates and returns a boolean array that is random.
        /// </summary>
        /// <param name="length">The length of the array.</param>
        /// <returns>A boolean array of random values.</returns>
        internal static bool[] RandomBools(int length, float weighting = 0.5f)
        {
            bool[] array = new bool[length];
            for (int i = 0; i < array.Length; i++)
                array[i] = Rnd.Range(0, 1f) > weighting;
            return array;
        }

        /// <summary>
        /// Converts a hexadecimal string into colors.
        /// </summary>
        /// <param name="hex">A string of hexadecimal, which can be formatted as "FFFFFF", "#FFFFFF", or "0xFFFFFF"</param>
        /// <returns>Color converted from hexadecimal string.</returns>
        internal static Color HexToColor(this string hex)
        {
            hex = hex.Replace("0x", "").Replace("#", "");
            return new Color32(byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber), byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber), byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber), (byte)(hex.Length < 8 ? 255 : byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)));
        }

        /// <summary>
        /// Generates button colors based on the solution, then generating random colors.
        /// </summary>
        /// <param name="mustInclude">The buttons that must be included, which is one of the answers.</param>
        /// <param name="buttons">The buttons it can choose from. NOTE: This creates a new instance.</param>
        internal static void GenerateColoredButtons(this string mustInclude, out ButtonType[] buttons)
        {
            List<ButtonType> output = new List<ButtonType>();
            foreach (char c in mustInclude)
                output.Add(charToButton[c.ToLower()]);
            while (output.Count < 8)
            {
                output.AddRange(AllButtonTypes.OrderBy(x => Rnd.Range(0, 1f)).Take(8 - output.Count).ToArray());
                output = output.Distinct().ToList();
            }
            buttons = output.ToArray();
        }

        /// <summary>
        /// Gets the color equivalent of the supplied ButtonType.
        /// </summary>
        /// <param name="buttonType">The type of ButtonType to use.</param>
        /// <returns>The color equivalent of ButtonType.</returns>
        internal static Color GetColor(this ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Azure: return HexToColor("315BA1");
                case ButtonType.Blue: return HexToColor("0000FF");
                case ButtonType.Crimson: return HexToColor("DC143C");
                case ButtonType.Diamond: return HexToColor("B9F2FF");
                case ButtonType.Emerald: return HexToColor("50C878");
                case ButtonType.Fuchsia: return HexToColor("F400A1");
                case ButtonType.Green: return HexToColor("00FF00");
                case ButtonType.Hazel: return HexToColor("ECC0A1");
                case ButtonType.Ice: return HexToColor("DDF9F1");
                case ButtonType.Jade: return HexToColor("00A86B");
                case ButtonType.Kiwi: return HexToColor("7F9A65");
                case ButtonType.Lime: return HexToColor("BFFF00");
                case ButtonType.Magenta: return HexToColor("FF00FF");
                case ButtonType.Navy: return HexToColor("000080");
                case ButtonType.Orange: return HexToColor("FF7F00");
                case ButtonType.Purple: return HexToColor("800080");
                case ButtonType.Quartz: return HexToColor("51484F");
                case ButtonType.Red: return HexToColor("FF0000");
                case ButtonType.Salmon: return HexToColor("FA8072");
                case ButtonType.Tan: return HexToColor("D2B48C");
                case ButtonType.Ube: return HexToColor("8878C3");
                case ButtonType.Vibe: return HexToColor("AC3142");
                case ButtonType.White: return HexToColor("FFFFFF");
                case ButtonType.Xotic: return HexToColor("863336");
                case ButtonType.Yellow: return HexToColor("FFFF00");
                case ButtonType.Zen: return HexToColor("D2D0AE");
                default: throw new NotImplementedException("Unrecognised color in Function.GetColor().");
            }
        }

        /// <summary>
        /// Plays voice lines whenever the timer strikes a specific time.
        /// </summary>
        /// <param name="time">The current time remaining in seconds.</param>
        /// <param name="exception">An exception that, if true, will not play voice lines.</param>
        internal static void PlaySoundFromTime(this PhosphorescenceScript pho, int time, bool exception)
        {
            if (exception)
                return;

            pho.PlaySound(SFX.Pho.TimerTick);

            switch (time)
            {
                case 1: pho.PlaySound(SFX.Pho.Voice.One); break;
                case 2: pho.PlaySound(SFX.Pho.Voice.Two); break;
                case 3: pho.PlaySound(SFX.Pho.Voice.Three); break;
                case 4: pho.PlaySound(SFX.Pho.Voice.Four); break;
                case 5: pho.PlaySound(SFX.Pho.Voice.Five); break;
                case 6: pho.PlaySound(SFX.Pho.Voice.Six); break;
                case 7: pho.PlaySound(SFX.Pho.Voice.Seven); break;
                case 8: pho.PlaySound(SFX.Pho.Voice.Eight); break;
                case 9: pho.PlaySound(SFX.Pho.Voice.Nine); break;
                case 30: pho.PlaySound(SFX.Pho.Voice.Thirty); break;
                case 60: pho.PlaySound(SFX.Pho.Voice.OneMinute); break;
                case 120: pho.PlaySound(SFX.Pho.Voice.TwoMinutes); break;
                case 180: pho.PlaySound(SFX.Pho.Voice.ThreeMinutes); break;
                case 240: pho.PlaySound(SFX.Pho.Voice.FourMinutes); break;
                default: if (time % 60 == 0) pho.PlaySound(SFX.Pho.NotableTimeLeft); break;
            }
        }

        /// <summary>
        /// Counts the number of L-shapes within the boolean array. The boolean array is treated as being 2-dimensional.
        /// </summary>
        /// <param name="colors">The boolean array, which must be of length 49.</param>
        /// <returns>An integer representing the amount of L's found.</returns>
        internal static int LCount(this bool[] colors)
        {
            if (colors.Length != 49)
                throw new IndexOutOfRangeException("Colors is length " + colors.Length);

            const int s = 7;
            int count = 0;

            for (int i = 0; i < s - 1; i++)
                for (int j = 0; j < s - 1; j++)
                    if (colors.IsL(i, j))
                        count++;

            return count;
        }

        /// <summary>
        /// Determines if the current index is looking at an L pattern.
        /// </summary>
        /// <param name="colors">The boolean array, treated as 2-dimensional.</param>
        /// <param name="i">The first dimension's index.</param>
        /// <param name="j">The second dimension's index.</param>
        /// <returns>True if [i, j] of colors (treated as 2-dimensions) is an L pattern.</returns>
        internal static bool IsL(this bool[] colors, int i, int j)
        {
            const int s = 7;

            // Subarray containing the 4 current squares.
            bool[] subArray = new[]
            {
            colors[(i * s) + j],
            colors[(i * s) + j + 1],
            colors[((i + 1) * s) + j],
            colors[((i + 1) * s) + j + 1]
        };

            // Do the current 4 squares contain 1 or 3 black squares? (Which indicates an L-shape).
            return subArray.Where(b => b).Count() % 2 == 1;
        }
    }
}