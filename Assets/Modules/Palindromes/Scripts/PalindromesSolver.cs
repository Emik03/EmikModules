using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Solver Algorithm originally written in Python by mythers45. https://repl.it/@AnonKTANE/PalindromeSum
/// C# conversion done primarily by Emik, with some help from Xmaster.
/// </summary>
namespace PalindromesModule
{
    internal class PalindromesSolver
    {
        /// <summary>
        /// Runs through the algorithm to extract 3 palindromic numbers that add up to 'd'.
        /// </summary>
        /// <param name="d">The sum of three palindromes. The screen's displayed number.</param>
        public static List<string> Get(string d, int moduleId)
        {
            List<string> l = new List<string>();
            for (int i = 0; i < d.Length; i++)
            {
                while (d.Length < 9)
                    d = "0" + d;

                l = new List<string> {
                d,
                ".........",
                "........",
                "......."
            };

                Calculate(l, 0);
            }

            return Validate(l, moduleId);
        }

        private static List<string> Validate(List<string> l, int moduleId)
        {
            //in case if it somehow returned 4 elements
            if (l.Count != 4)
            {
                Debug.LogFormat("[Palindromes #{0}]: Algorithm > Failed to return a list of size 4: {1}", moduleId, l.Join(", "));
                return new List<string>(0);
            }

            int[] ans = new int[4];

            //in case if any of them have periods (unfilled numbers)
            for (byte i = 0; i < ans.Length; i++)
            {
                if (!int.TryParse(l[i], out ans[i]))
                {
                    Debug.LogFormat("[Palindromes #{0}]: Algorithm > Failed to parse an element in the list: {1}", moduleId, l.Join(", "));
                    return new List<string>(0);
                }
            }

            //in case if it doesn't actually add up to the screen
            if (ans[1] + ans[2] + ans[3] != ans[0])
            {
                Debug.LogFormat("[Palindromes #{0}]: Algorithm > Gave incorrect numbers: {1}", moduleId, l.Join(", "));
                return new List<string>(0);
            }

            if (l.Count != 0)
                Debug.LogFormat("[Palindromes #{0}]: Example solution > {1}.", moduleId, l.Skip(1).Join(" & "));

            return l;
        }

        // Assign n to an index and its palindromic index
        private static string Assign(string v, int i, int n)
        {
            // Case where nothing's on the ends
            if (i == 0)
                return Math.Max(n, 0).ToString() + Range(v, 1, v.Length - 1) + Math.Max(n, 0).ToString();

            // Case where nothing's in the middle
            if (i + 1 == v.Length - i)
                return Range(v, 0, i) + Math.Max(n, 0).ToString() + Range(v, v.Length - i, v.Length);

            // Case where there are ends and a middle
            return Range(v, 0, i) + Math.Max(n, 0).ToString() + Range(v, i + 1, v.Length - i - 1) + Math.Max(n, 0).ToString() + Range(v, v.Length - i, v.Length);
        }

        private static string Range(string v, int start, int end)
        {
            string str = "";

            for (int i = start; i < end; i++)
                str += v[i];

            return str;
        }

        private static bool Calculate(List<string> l, int i)
        {
            var d = l[0];

            // Calculate an index of the first palindrome
            // (+ set other palindromes to match up well)
            if (i == 0)
            {
                l[1] = Assign(l[1], 0, (int)(char.GetNumericValue(d[0]) - 1));
                //Debug.Log("First comment. l = " + l.Join(", ") + " i = " + i.ToString());
                while (!Calculate(l, 1))
                {
                    // While next calc fails...
                    // Increment value by 1 (if possible), retry
                    // Final failure case: exit
                    l[1] = Assign(l[1], 0, (int)char.GetNumericValue(l[1][0]) + 1);
                    if (l[1][0] > char.GetNumericValue(d[0]))
                        return false;
                    //Debug.Log("Second comment. l = " + l.Join(", ") + " i = " + i.ToString());
                }
            }
            else if (i < 5)
            {
                var m = char.GetNumericValue(d[i]) + 9;

                // Subtract third digit from target
                if (i > 1)
                    m -= (int)char.GetNumericValue(l[3][i - 2]);

                if ((i == 1 && char.GetNumericValue(l[1][0]) == char.GetNumericValue(d[0])) || (i == 2 && Modulo(char.GetNumericValue(l[1][1]) + char.GetNumericValue(l[2][0]), 10) == char.GetNumericValue(d[1])) || (i > 2 && Modulo(char.GetNumericValue(l[1][i - 1]) + char.GetNumericValue(l[2][i - 2]) + char.GetNumericValue(l[3][i - 3]), 10) == char.GetNumericValue(d[i - 1])))
                    m -= 10;

                // Failure case:
                if (m < -1)
                    return false;

                l[1] = Assign(l[1], i, (int)Math.Max(m - 9, 0));
                l[2] = Assign(l[2], i - 1, (int)Math.Max(m - char.GetNumericValue(l[1][i]), 0));

                var s = char.GetNumericValue(d[d.Length - i]) - char.GetNumericValue(l[1][l[1].Length - i]) - char.GetNumericValue(l[2][l[2].Length - i]);

                // Secondary target- subtract carry in from this target
                if (i > 1)
                    s -= Math.Floor((Convert.ToInt32(Range(l[1], 10 - i, 9)) + Convert.ToInt32(Range(l[2], 9 - i, 8)) + Convert.ToInt32(Range(l[3], 8 - i, 7))) / Math.Pow(10, i - 1));

                l[3] = Assign(l[3], i - 1, (int)Modulo(s, 10));
                //Debug.Log("Third comment. l = " + l.Join(", ") + ", m = " + m.ToString() + ", i = " + i.ToString() + ", s = " + s.ToString());
                while (!Calculate(l, i + 1))
                {
                    // While next calc fails...
                    // Increment value by 1 (if possible), retry
                    if (l[1][i] == '9')
                    {
                        // Special case: 1 over, no carry in
                        // Failure case:
                        if (char.GetNumericValue(l[1][i]) + char.GetNumericValue(l[2][i]) > m || l[2][i - 1] == '9')
                            return false;

                        l[2] = Assign(l[2], i - 1, (int)char.GetNumericValue(l[2][i - 1]) + 1);
                    }
                    else
                    {
                        // Normal case: increment and adjust
                        l[1] = Assign(l[1], i, (int)char.GetNumericValue(l[1][i]) + 1);
                        l[2] = Assign(l[2], i - 1, (int)m - (int)char.GetNumericValue(l[1][i]));
                        s = (int)char.GetNumericValue(d[d.Length - i]) - (int)char.GetNumericValue(l[1][l[1].Length - i]) - (int)char.GetNumericValue(l[2][l[2].Length - i]);

                        // Secondary target- subtract carry in from this target
                        if (i > 1)
                            s -= Math.Floor((Convert.ToInt32(Range(l[1], 10 - i, 9)) + Convert.ToInt32(Range(l[2], 9 - i, 8)) + Convert.ToInt32(Range(l[3], 8 - i, 7))) / Math.Pow(10, i - 1));

                        l[3] = Assign(l[3], i - 1, (int)Modulo(s, 10));
                        // All slots set- verify sum and return all the way
                    }
                    //Debug.Log("Fourth comment. l = " + l.Join(", ") + ", m = " + m.ToString() + ", i = " + i.ToString() + ", s = " + s.ToString());
                }
            }
            return Modulo(Convert.ToInt32(l[1]) + Convert.ToInt32(l[2]) + Convert.ToInt32(l[3]), 1000000000) == Convert.ToInt32(d);
        }

        private static double Modulo(double num, int mod)
        {
            //modulation for negatives
            if (num < 0)
            {
                num += mod;
                num = Modulo(num, mod);
            }

            //modulation for positives
            else if (num >= mod)
            {
                num -= mod;
                num = Modulo(num, mod);
            }

            //once it reaches here, we know it's modulated and can return it
            return num;
        }
    }
}