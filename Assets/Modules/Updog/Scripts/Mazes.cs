using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColorDictionary = System.Collections.Generic.Dictionary<KeepCodingAndNobodyExplodes.Tuple<UnityEngine.Color, UnityEngine.Color>, string[]>;
using ColorCombination = KeepCodingAndNobodyExplodes.Tuple<UnityEngine.Color, UnityEngine.Color>;
using IntTuple = KeepCodingAndNobodyExplodes.Tuple<int, int>;
using MazeTuple = KeepCodingAndNobodyExplodes.Tuple<System.Text.StringBuilder[], string[]>;
using String = System.Text.StringBuilder;
using WordTuple = KeepCodingAndNobodyExplodes.Tuple<Updog.Spelling, Updog.Casing>;

namespace Updog
{
    internal static class Mazes
    {
        internal const int MaxLength = 11;

        private static ColorDictionary Bones
        {
            get
            {
                return new ColorDictionary
                {
                    {
                        new ColorCombination(Colors.red, Colors.red),
                        new string[]
                        {
                            "     ",
                            "x    ",
                            "   x ",
                            "  x  ",
                            "x   x"
                        }
                    },
                    {
                        new ColorCombination(Colors.blue, Colors.red),
                        new string[]
                        {
                            "x  xx",
                            "    x",
                            " x   ",
                            "     ",
                            "     "
                        }
                    },
                    {
                        new ColorCombination(Colors.green, Colors.red),
                        new string[]
                        {
                            "    x",
                            "    x",
                            " x   ",
                            "    x",
                            " x   "
                        }
                    },
                    {
                        new ColorCombination(Colors.red, Colors.blue),
                        new string[]
                        {
                            "x x  ",
                            "   xx",
                            " x   ",
                            "     ",
                            "     "
                        }
                    },
                    {
                        new ColorCombination(Colors.blue, Colors.blue),
                        new string[]
                        {
                            "     ",
                            " x   ",
                            "     ",
                            "x x x",
                            "x    "
                        }
                    },
                    {
                        new ColorCombination(Colors.green, Colors.blue),
                        new string[]
                        {
                            "x    ",
                            "     ",
                            "x x  ",
                            "     ",
                            " x x "
                        }
                    },
                    {
                        new ColorCombination(Colors.red, Colors.green),
                        new string[]
                        {
                            "     ",
                            "xx   ",
                            " x   ",
                            "x x  ",
                            "     "
                        }
                    },
                    {
                        new ColorCombination(Colors.blue, Colors.green),
                        new string[]
                        {
                            "  x  ",
                            "  x  ",
                            " x  x",
                            "     ",
                            "  x  "
                        }
                    },
                    {
                        new ColorCombination(Colors.green, Colors.green),
                        new string[]
                        {
                            " x   ",
                            "xx x ",
                            "     ",
                            "  x  ",
                            "     "
                        }
                    }
                };
            }
        }

        private static Dictionary<Spelling, String[]> Walls
        {
            get 
            {
                return new Dictionary<Spelling, String[]>
                {
                    {
                        Spelling.Dog,
                        new String[]
                        {
                            new String("+---------+"),
                            new String("|R        |"),
                            new String("| + + + + |"),
                            new String("| |    Y| |"),
                            new String("| + + + + |"),
                            new String("|O|  G  |B|"),
                            new String("| + + + + |"),
                            new String("| |     | |"),
                            new String("| + + + + |"),
                            new String("|P        |"),
                            new String("+---------+")
                        }
                    },
                    {
                        Spelling.Dawg,
                        new String[]
                        {
                            new String("+---------+"),
                            new String("|        B|"),
                            new String("| + + + + |"),
                            new String("|  R| |Y  |"),
                            new String("| +-+ +-+ |"),
                            new String("|         |"),
                            new String("| +-+ +-+ |"),
                            new String("|  O| |G  |"),
                            new String("| + + + + |"),
                            new String("|P        |"),
                            new String("+---------+")
                        }
                    },
                    {
                        Spelling.Doge,
                        new String[]
                        {
                            new String("+---------+"),
                            new String("|R       B|"),
                            new String("+-+ + + +-+"),
                            new String("|         |"),
                            new String("| + + + + |"),
                            new String("|   |O|G  |"),
                            new String("| + + + + |"),
                            new String("|         |"),
                            new String("+-+ + + +-+"),
                            new String("|P       Y|"),
                            new String("+---------+")
                        }
                    },
                    {
                        Spelling.Dag,
                        new String[]
                        {
                            new String("+---+-+---+"),
                            new String("|G  |R|  B|"),
                            new String("+-+ + + + |"),
                            new String("|O|       |"),
                            new String("| +-+ + +-+"),
                            new String("|   |    Y|"),
                            new String("| + +-+ +-+"),
                            new String("|     |   |"),
                            new String("+-+ + +-+ |"),
                            new String("|P        |"),
                            new String("+---------+")
                        }
                    },
                    {
                        Spelling.Dogg,
                        new String[]
                        {
                            new String("+---------+"),
                            new String("|R        |"),
                            new String("+-+-+-+-+ |"),
                            new String("|Y  |     |"),
                            new String("| + + + +-+"),
                            new String("|    O    |"),
                            new String("+-+ +-+ + |"),
                            new String("|       |B|"),
                            new String("| +-+-+-+-+"),
                            new String("|G       P|"),
                            new String("+---------+")
                        }
                    },
                    {
                        Spelling.Dage,
                        new String[]
                        {
                            new String("+-----+---+"),
                            new String("|     |R  |"),
                            new String("+-+-+ +-+ |"),
                            new String("|  Y|     |"),
                            new String("| +-+ + + |"),
                            new String("|     |O| |"),
                            new String("+-+-+ +-+-+"),
                            new String("|  G|    B|"),
                            new String("| +-+ +-+-+"),
                            new String("|        P|"),
                            new String("+---------+")
                        }
                    }
                };
            }
        }

        internal static MazeTuple Get(WordTuple word, Color colorA, Color colorB)
        {
            return new MazeTuple(
                Walls[word.Item1],
                Bones[new ColorCombination(colorA, colorB)]);
        }

        internal static IntTuple Find(this MazeTuple maze, Color color)
        {
            char toFind = Colors.GetAll[color].FirstOrDefault();

            for (int i = 0; i < maze.Item1.Length; i++)
                for (int j = 0; j < maze.Item1[i].Length; j++)
                    if (maze.Item1[i][j] == toFind)
                        return new IntTuple(i, j);

            throw new IndexOutOfRangeException("maze.Item1: " + maze.Item1.Join("\n"));
        }
    }
}
