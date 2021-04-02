using KeepCodingAndNobodyExplodes;
using System;
using System.Linq;
using UnityEngine;
using IntTuple = KeepCodingAndNobodyExplodes.Tuple<int, int>;
using MazeTuple = KeepCodingAndNobodyExplodes.Tuple<System.Text.StringBuilder[], string[]>;
using String = System.Text.StringBuilder;

namespace Updog
{
    internal static class Extensions
    {
        private const string WallCharacters = "-|+";

        internal static int FiveToElevenIndex(this int i)
        { 
            return (i * 2) + 1;
        }

        internal static int ElevenToFiveIndex(this int i)
        { 
            return (i - 1) / 2; 
        }

        internal static void Copy<T>(this T[] source, T[] destination)
        { 
            Array.Copy(source, destination, source.Length); 
        }

        internal static bool IsSolved(this String[] strings)
        {
            return !strings.Any(s => s.ToString().Any(c => c == 'x'));
        }

        internal static IntTuple New(this IntTuple pos, int i, int j)
        {
            return new IntTuple(pos.Item1 + i, pos.Item2 + j);
        }

        internal static string[] AsString(this Color[] array, string toReplaceWithEmptyString = "")
        {
            return new string[array.Length]
                .Select((i, n) => Colors.GetAll[array[n]])
                .Select(i => i == string.Empty ? toReplaceWithEmptyString : i)
                .ToArray();
        }

        internal static int CountBones(this String[] strings)
        {
            return strings
                .Select(s => s.ToString())
                .Join("")
                .Where(c => c == 'x')
                .Count();
        }

        internal static void ToLog(this IntTuple pos, UpdogScript updog)
        {
            updog.Log("The starting position is ({0},{1})."
                .Form(pos.Item2.ElevenToFiveIndex(), pos.Item1.ElevenToFiveIndex()));
        }

        internal static void ToLog(this bool[] order, UpdogScript updog)
        {
            updog.Log("Press the buttons in the order of ({0}), where D is a dog button, and N is a normal button."
                .Form((object)order.Select(b => b ? "D" : "N").Join("")));
        }

        internal static void ToLog(this String[] strings, UpdogScript updog, IntTuple pos)
        {
            updog.Log("Notice: The coordinates used are formatted as (x,y) and are 0-indexed.");
            updog.Log("The maze is as follows: (S = start, x = bone)");

            string[] logs = new string[strings.Length];

            for (int i = 0; i < strings.Length; i++)
                logs[i] =  strings[i].ToString()
                    .Select((c, j) => pos.Item1 == i && pos.Item2 == j ? 'S' 
                    : char.IsUpper(c) ? ' ' : c).Join("");

            if (Application.isEditor)
                updog.Log('\n' + logs.Join("\n"));
            else
                foreach (var log in logs)
                    updog.Log(log);
        }

        internal static String[] InsertBones(this MazeTuple maze)
        {
            String[] output = new String[maze.Item1.Length];
            for (int i = 0; i < maze.Item1.Length; i++)
            {
                output[i] = maze.Item1[i];

                for (int j = 0; i % 2 == 1 && j < maze.Item2[i.ElevenToFiveIndex()].Length; j++)
                    if (maze.Item2[i.ElevenToFiveIndex()][j] != ' ')
                        output[i][j.FiveToElevenIndex()] = maze.Item2[i.ElevenToFiveIndex()][j];
            }
            return output;
        }

        internal static bool IsValidMove(this String[] strings, UpdogScript updog, ref IntTuple pos, int i)
        {
            IntTuple intercept;
            ApplyMovement(ref pos, out intercept, i);

            if (pos.Item1 < 0 || pos.Item1 >= Mazes.MaxLength ||
                pos.Item2 < 0 || pos.Item2 >= Mazes.MaxLength)
                updog.Log("The dog fell out of bounds ({0},{1}), strike for incompetence!"
                    .Form(pos.Item1.ElevenToFiveIndex(), pos.Item2.ElevenToFiveIndex()));

            else if (WallCharacters.Contains(strings[intercept.Item1][intercept.Item2]))
                updog.Log("The dog hit a wall, strike for animal cruelty!");

            else return true;

            return false;
        }

        internal static bool[] GetValidMovements(this String[] strings, ref IntTuple pos)
        {
            return new bool[]
            {
                !WallCharacters.Contains(strings[pos.New(0, -1).Item1][pos.New(0, -1).Item2]),
                !WallCharacters.Contains(strings[pos.New(1, 0).Item1][pos.New(1, 0).Item2]),
                !WallCharacters.Contains(strings[pos.New(-1, 0).Item1][pos.New(-1, 0).Item2]),
                !WallCharacters.Contains(strings[pos.New(0, 1).Item1][pos.New(0, 1).Item2])
            };
        }

        internal static void ApplyMovement(ref IntTuple pos, out IntTuple intercept, int i)
        {
            switch (i)
            {
                case 0:
                    intercept = pos.New(0, -1);
                    pos = pos.New(0, -2);
                    break;
                case 1:
                    intercept = pos.New(1, 0);
                    pos = pos.New(2, 0);
                    break;
                case 2:
                    intercept = pos.New(-1, 0);
                    pos = pos.New(-2, 0);
                    break;
                case 3:
                    intercept = pos.New(0, 1);
                    pos = pos.New(0, 2);
                    break;
                default: throw new NotImplementedException("i: " + i);
            }
        }
    }
}
