using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

namespace TheOctadecayotton
{
    static class TheOctadecayottonExtensions
    {
        internal static int AsInt(this bool b)
        {
            return b ? 1 : 0;
        }

        internal static int Abs(this int i)
        {
            return Math.Abs(i);
        }

        internal static bool IsNullOrEmpty(this string str)
        {
            return str == null || str == string.Empty;
        }

        internal static Vector3 Merge(this Vector3 a, Vector3 b, float f = 0.5f)
        {
            float negF = 1 - f;
            return new Vector3(a.x * negF + b.x * f, a.y * negF + b.y * f, a.z * negF + b.z * f);
        }

        internal static string ToBinary(this int i, int length)
        {
            string str = Convert.ToString(i, 2).PadLeft(length, '0');
            return str.Substring(str.Length - length, length);
        }

        internal static string InvertBinary(this string str, int index)
        {
            char[] input = str.ToCharArray();
            input[index] = input[index] == '0' ? '1' : '0';
            return new string(input);
        }

        internal static string GrayToBinary(this string str)
        {
            char[] output = str.ToCharArray();
            for (int i = 1; i < output.Length; i++)
                output[i] = output[i - 1] == str[i] ? '0' : '1';
            return new string(output);
        }

        internal static string BinaryToGray(this string str)
        {
            char[] output = str.ToCharArray();
            for (int i = 1; i < output.Length; i++)
                output[i] = str[i - 1] == str[i] ? '0' : '1';
            return new string(output);
        }

        internal static string Xor(this string str1, string str2)
        {
            char[] output = new char[str1.Length];
            for (int i = 0; i < str1.Length; i++)
                output[i] = str1[i] == str2[i] ? '0' : '1';
            return new string(output);
        }

        internal static void PlaySound(this TheOctadecayottonScript octadecayotton, string sound)
        {
            octadecayotton.Audio.PlaySoundAtTransform(sound, octadecayotton.transform);
        }

        internal static Color Step(this Color colorA, Color colorB, int divider)
        {
            return new Color(colorA.r + ((colorB.r - colorA.r) / divider), colorA.g + ((colorB.g - colorA.g) / divider), colorA.b + ((colorB.b - colorA.b) / divider), colorA.a + ((colorB.a - colorA.a) / divider));
        }

        internal static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary(e => e.Key, e => e.Value);
        }

        internal static float InOutBounce(this float f)
        {
            return f < 0.5f ? (1 - OutBounce(1 - 2 * f)) / 2 : (1 + OutBounce(2 * f - 1)) / 2;
        }

        internal static float InBounce(this float f)
        {
            return 1 - OutBounce(1 - f);
        }

        internal static float OutBounce(this float f)
        {
            const float n1 = 7.5625f, d1 = 2.75f;
            if (f < 1 / d1)
                return n1 * f * f;
            if (f < 2 / d1)
                return n1 * (f -= 1.5f / d1) * f + 0.75f;
            if (f < 2.5 / d1)
                return n1 * (f -= 2.25f / d1) * f + 0.9375f;
            return n1 * (f -= 2.625f / d1) * f + 0.984375f;
        }

        internal static float ElasticInOut(this float x)
        {
            const float c5 = (float)(2 * Math.PI) / 4.5f;
            return x == 0 ? 0 : x == 1 ? 1 : x < 0.5
              ? -(float)(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * c5)) / 2
              : (float)(Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 + 1;
        }

        internal static Vector3 ToVector3(this bool[] pos, int dimensions, bool stretchToFit)
        {
            float x = 0, y = 0, z = 0;
            for (int i = 0; i < pos.Length; i++)
            {
                x += pos[i].AsInt() * Position.weights[i, 0];
                y += pos[i].AsInt() * Position.weights[i, 1];
                z += pos[i].AsInt() * Position.weights[i, 2];
            }
            float xMax = 0 , yMax = 0, zMax = 0;
            for (int i = 0; i < dimensions; i++)
            {
                xMax += Position.weights[i, 0];
                yMax += Position.weights[i, 1];
                zMax += Position.weights[i, 2];
            }
            float max = Math.Max(xMax, Math.Max(yMax, zMax));
            return stretchToFit
                ? new Vector3(x / xMax, y / yMax, z / zMax)
                : new Vector3(x / max, y / max, z / max);
        }

        internal static Rotation[][][] GetRandomRotations(RotationOptions options)
        {
            var output = new List<List<Rotation>>[options.RotationCount];
            var allAxies = InteractScript.allAxies.Take(options.Dimension);
            
            for (int i = 0; i < options.RotationCount; i++)
            {
                output[i] = new List<List<Rotation>>();
                var possibleAxies = new Stack<Axis>(allAxies.ToArray().Shuffle());

                for (int j = 0; j < options.MinRotations ||
                    (j < options.MaxRotations && possibleAxies.Count >= options.MinLengthPerRotation && 
                    (Rnd.Range(0, 1f) <= options.ChanceToRepeat || j == 0)); j++)
                    AddRandomRotations(output, possibleAxies, options, ref i, ref j);
            }

            return output.Select(a => a.Select(b => b.ToArray())).Select(a => a.ToArray()).ToArray();
        }

        private static void AddRandomRotations(List<List<Rotation>>[] output, Stack<Axis> possibleAxies, RotationOptions options, ref int i, ref int j)
        {
            output[i].Add(new List<Rotation>());
            int rnd = Rnd.Range(options.MinLengthPerRotation,
                Math.Min(Math.Min(possibleAxies.Count, options.MaxLengthPerRotation), possibleAxies.Count() - options.MinRotations + 1) + 1);

            Debug.Log(rnd + "  " + possibleAxies.Join(", "));
            for (int k = 0; k < rnd; k++)
                output[i][j].Add(new Rotation(Rnd.Range(0, 1f) > options.ChanceForNegativeRotation, possibleAxies.Pop()));

            switch (output[i][j].Count)
            {
                case 1: // Positive 1-cycle rotations are the same as 0-cycle rotations.
                    output[i][j][0].IsNegative = true;
                    break;
                case 2: // Equal 2-cycle rotations are the same as 2 1-cycle rotations.
                    if (output[i][j][0].IsNegative == output[i][j][1].IsNegative)
                        output[i][j][0].IsNegative = !output[i][j][0].IsNegative;
                    break;
            }
        }

        internal static Rotation[][][] ToRotations(this string str)
        {
            List<List<List<Rotation>>> output = new List<List<List<Rotation>>> { new List<List<Rotation>>() { new List<Rotation>() } };

            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '&':
                        output.Add(new List<List<Rotation>>());
                        output[output.Count - 1].Add(new List<Rotation>());
                        break;

                    case ',':
                        output[output.Count - 1].Add(new List<Rotation>());
                        break;

                    case '+':
                    case '-':
                        output[output.Count - 1][output[output.Count - 1].Count - 1].Add(new Rotation(
                            str[i] == '-',
                            (Axis)Enum.Parse(typeof(Axis), str[++i].ToString())));
                        break;
                }
            }

            return output.Select(o => o.Select(n => n.ToArray()).ToArray()).ToArray();
        }

        internal static string ToLog(this Dictionary<Axis, bool> dictionary)
        {
            return dictionary.Select(a => (a.Value ? "+" : "-") + a.Key).Join("");
        }

        internal static string ToLog(this Rotation[][][] rotations)
        {
            const string divider = " & ", subdivider = ", ";
            string output = string.Empty;

            for (int i = 0; i < rotations.Length; i++)
            {
                for (int j = 0; j < rotations[i].Length; j++)
                {
                    for (int k = 0; k < rotations[i][j].Length; k++)
                        output += (rotations[i][j][k].IsNegative ? '-' : '+') + rotations[i][j][k].Axis.ToString();

                    if (j < rotations[i].Length - 1)
                        output += subdivider;
                }

                if (i < rotations.Length - 1)
                    output += divider;
            }

            return output;
        }
    }
}
