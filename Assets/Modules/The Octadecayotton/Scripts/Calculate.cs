using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOctadecayotton
{
    internal static class Calculate
    {
        internal static Dictionary<Axis, bool> Get(this Rotation[][][] rotations, int dimension, int moduleId)
        {
            return GetAnchorSphere(GetPrimaryValues(rotations, ref dimension, ref moduleId), rotations, ref dimension, ref moduleId);
        }

        internal static bool Validate(this List<Axis> inputs, Dictionary<Axis, bool> startingSphere, Dictionary<Axis, bool> anchorSphere, Dictionary<Axis, int> axesUsed, Axis[] order, ref int breakCount, int dimension, ref int moduleId)
        {
            if (inputs.Count == dimension)
            {
                Debug.LogFormat("[The Octadecayotton #{0}]: Submitting the current sphere...", moduleId);
                return startingSphere.Select((a, n) => a.Value != anchorSphere.ElementAt(n).Value).All(b => !b);
            }

            if (inputs.Count != (dimension == 3 ? 1 : 3))
                return true;

            Debug.LogFormat("[The Octadecayotton #{0}]: Submitting {1}...", moduleId, inputs.Select(a => (int)a).Join(dimension > 10 ? ", " : ""));

            for (int i = 0; i < inputs.Count; i++)
            {
                startingSphere[order[(int)inputs[i]]] = !startingSphere[order[(int)inputs[i]]];
                axesUsed[order[(int)inputs[i]]]++;
            }

            if (axesUsed.Where(a => inputs.Contains(a.Key)).Any(a => a.Value >= 3))
            {
                breakCount++;
                Debug.LogFormat("[The Octadecayotton #{0}]: The constraint rule has been broken {1}/{2} times.", moduleId, breakCount, dimension == 4 ? 6 : 5);
            }

            Debug.LogFormat("[The Octadecayotton #{0}]: The starting sphere is now located in {1}. (XYZWVURST-ordered)", moduleId, startingSphere.Select(a => a.Value ? "+" : "-").Join(""));
            return breakCount < (dimension == 4 ? 6 : 5);
        }

        private static int[] GetPrimaryValues(Rotation[][][] rotations, ref int dimension, ref int moduleId)
        {
            if (Application.isEditor)
                LogPrimaryValues();
            int[] output = new int[rotations.Length];
            for (int i = 0; i < rotations.Length; i++)
            {
                Debug.LogFormat("[The Octadecayotton #{0}]: ROTATION {1}", moduleId, i + 1);
                for (int j = 0; j < rotations[i].Length; j++)
                    if (rotations[i][j].Length != 1)
                        for (int k = 0; k < rotations[i][j].Length; k++)
                            output[i] += GetPrimaryValue(rotations[i][j][k], rotations[i][j][(k + 1) % rotations[i][j].Length], moduleId);
                    else
                        output[i] += GetPrimaryValue(rotations[i][j][0], rotations[i][j][0], moduleId);
            }
            Debug.LogFormat("[The Octadecayotton #{0}]: The primary values are {1}.", moduleId, output.Select(i => i.Abs()).Join(", "));
            return output;
        }

        private static int GetPrimaryValue(Rotation rotationA, Rotation rotationB, int moduleId)
        {
            int output = (((int)(Mathf.Pow((int)rotationB.Axis, 2) + (int)Mathf.Pow((int)rotationA.Axis, 3)) % 9) + 1) * (rotationA.IsNegative == rotationB.IsNegative ? 1 : -1);
            if (moduleId > 0)
                Debug.LogFormat("[The Octadecayotton #{0}]: {1} = {2}", moduleId, (rotationA.IsNegative ? "-" : "+") + rotationA.Axis + (rotationB.IsNegative ? "-" : "+") + rotationB.Axis, output);
            return output;
        }

        private static void LogPrimaryValues()
        {
            Debug.LogWarning("Filter out this table by pressing the warning icon in the top right of your unity!");
            for (int i = 0; i < Enum.GetValues(typeof(Axis)).GetLength(0); i++)
            {
                string str = string.Empty;
                for (int j = 0; j < Enum.GetValues(typeof(Axis)).GetLength(0); j++)
                    str += GetPrimaryValue(new Rotation(true, (Axis)i), new Rotation(true, (Axis)j), 0);
                Debug.LogWarning(str);
            }
        }

        private static Dictionary<Axis, bool> GetAnchorSphere(int[] pValues, Rotation[][][] rotations, ref int dimension, ref int moduleId)
        {
            string[] aValues = new string[pValues.Length + 1];
            aValues[0] = string.Empty.PadLeft(dimension, '0');

            for (int i = 1; i < aValues.Length; i++)
            {
                aValues[i] = pValues[i - 1].Abs().ToBinary(dimension);
                Debug.LogFormat("[The Octadecayotton #{0}]: Primary to binary -> a{1} = {2}", moduleId, i, aValues[i]);

                for (int j = 0; j < rotations[i - 1].Length; j++)
                    for (int k = 0; k < rotations[i - 1][j].Length; k++)
                        aValues[i] = aValues[i].InvertBinary(rotations[i - 1][j][k].IsNegative ? dimension - (int)rotations[i - 1][j][k].Axis - 1 : (int)rotations[i - 1][j][k].Axis);

                if (aValues[i][aValues[i].Length / 2] == '1')
                    for (int j = (i - 1) * (dimension / 3); j < i * (dimension / 3) && j < aValues[0].Length; j++)
                        aValues[0] = aValues[0].InvertBinary(j);

                Debug.LogFormat("[The Octadecayotton #{0}]: Axis inversion -> a{1} = {2}", moduleId, i, aValues[i]);
                aValues[i] = aValues[i].GrayToBinary();
                Debug.LogFormat("[The Octadecayotton #{0}]: Gray to binary -> a{1} = {2}", moduleId, i, aValues[i]);
            }

            Debug.LogFormat("[The Octadecayotton #{0}]: Final value -> a0 = {1}", moduleId, aValues[0]);

            for (int i = 1; i < aValues.Length; i++)
                aValues[i] = aValues[i].Xor(aValues[i - 1]);

            Debug.LogFormat("[The Octadecayotton #{0}]: Anchor Sequence = {1}", moduleId, aValues.Last());

            var output = new Dictionary<Axis, bool>();
            for (int i = 0; i < aValues.Last().Length; i++)
                output.Add((Axis)i, aValues.Last()[i] == '1');

            return output;
        }

        internal static int[][] GetAnswer(this Dictionary<Axis, bool> start, Dictionary<Axis, bool> end, Dictionary<Axis, int> used, Axis[] order, bool includeOrderChecks = false)
        {
            if (order.Length == 3)
                return start.GetSimpleAnswer(end, order, includeOrderChecks);

            List<int[]> output = new List<int[]>();
            var newStart = start.Clone();
            var newUsed = used.Clone();

            // Thanks to 'betaveros' for the order identification algorithm.
            // They provided me with the algorithm, and here I am butchering it with my C# code.
            if (includeOrderChecks)
            {
                for (int i = 0; i < newStart.Count - 3; i += 2)
                {
                    output.Add(new[] { i + 2, i + 1, i });

                    for (int j = i; j < i + 3; j++)
                    {
                        newUsed[order[j]]++;
                        newStart[order[j]] = !newStart[order[j]];
                    }
                }

                output.Add(new[] { newStart.Count - 1 - (newStart.Count % 2 == 0 ? 0 : 1), newStart.Count - 2 - (newStart.Count % 2 == 0 ? 0 : 1), 0 });

                newUsed[order[newStart.Count - 2 - (newStart.Count % 2 == 0 ? 0 : 1)]]++;
                newUsed[order[newStart.Count - 1 - (newStart.Count % 2 == 0 ? 0 : 1)]]++;
                newUsed[order[0]]++;

                newStart[order[newStart.Count - 2 - (newStart.Count % 2 == 0 ? 0 : 1)]] = !newStart[order[newStart.Count - 2 - (newStart.Count % 2 == 0 ? 0 : 1)]];
                newStart[order[newStart.Count - 1 - (newStart.Count % 2 == 0 ? 0 : 1)]] = !newStart[order[newStart.Count - 1 - (newStart.Count % 2 == 0 ? 0 : 1)]];
                newStart[order[0]] = !newStart[order[0]];
            }

            Stack<int> validMoves = new Stack<int>();
            for (int i = 0; i < newStart.Count; i++)
                if (newStart[(Axis)i] != end[(Axis)i] && newUsed[(Axis)i] < 3)
                    validMoves.Push(i);

            while (validMoves.Count >= 3)
            {
                int[] next = { validMoves.Pop(), validMoves.Pop(), validMoves.Pop() };
                for (int i = 0; i < next.Length; i++)
                    newStart[(Axis)next[i]] = !newStart[(Axis)next[i]];
                output.Add(next);
            }

            validMoves = new Stack<int>();
            for (int i = 0; i < newStart.Count; i++)
                if (newStart[(Axis)i] != end[(Axis)i])
                    validMoves.Push(i);

            while (validMoves.Count >= 3 && validMoves.Count != 4)
                output.Add(new[] { validMoves.Pop(), validMoves.Pop(), validMoves.Pop() });

            int[] corrections = validMoves.ToArray();
            List<int> dummies = corrections.GetDummies(4 - corrections.Length + (corrections.Length == 4 ? 1 : 0));

            switch (corrections.Length)
            {
                case 1:
                    output.Add(new[] { corrections[0], dummies[0], dummies[1] }.OrderByDescending(x => x).ToArray());
                    output.Add(new[] { corrections[0], dummies[1], dummies[2] }.OrderByDescending(x => x).ToArray());
                    output.Add(new[] { corrections[0], dummies[0], dummies[2] }.OrderByDescending(x => x).ToArray());
                    break;
                case 2:
                    output.Add(new[] { corrections[0], dummies[0], dummies[1] }.OrderByDescending(x => x).ToArray());
                    output.Add(new[] { corrections[1], dummies[0], dummies[1] }.OrderByDescending(x => x).ToArray());
                    break;
                case 4:
                    output.Add(new[] { corrections[0], corrections[1], dummies[0] }.OrderByDescending(x => x).ToArray());
                    output.Add(new[] { corrections[2], corrections[3], dummies[0] }.OrderByDescending(x => x).ToArray());
                    break;
            }

            if (output.Any(i => i.Contains(-1)))
                throw new IndexOutOfRangeException("output has -1: " + output.Select(i => i.Join("")).Join(", "));
            return output.Select(i => i.Select(j => Array.IndexOf(order, (Axis)j)).OrderByDescending(x => x).ToArray()).ToArray();
        }

        private static int[][] GetSimpleAnswer(this Dictionary<Axis, bool> start, Dictionary<Axis, bool> end, Axis[] order, bool includeOrderChecks = false)
        {
            List<int[]> output = new List<int[]>();
            var newStart = start.Clone();

            if (includeOrderChecks)
                for (int i = 0; i < newStart.Count - 1; i++)
                {
                    output.Add(new[] { i });
                    newStart[order[i]] = !newStart[order[i]];
                }

            for (int i = 0; i < newStart.Count; i++)
                if (newStart[order[i]] != end[order[i]])
                {
                    output.Add(new[] { i });
                    newStart[order[i]] = !newStart[order[i]];
                }

            if (output.Any(i => i.Contains(-1)))
                throw new IndexOutOfRangeException("i has -1: " + output.Select(i => i.Join("")).Join(", "));
            return output.ToArray();
        }

        private static List<int> GetDummies(this int[] corrections, int length)
        {
            List<int> dummies = new List<int>();
            for (int i = 0; i < Enum.GetValues(typeof(Axis)).Length; i++)
            {
                if (dummies.Count == length)
                    break;
                if (corrections.Contains(i))
                    continue;
                dummies.Add(i);
            }
            return dummies;
        }
    }
}
