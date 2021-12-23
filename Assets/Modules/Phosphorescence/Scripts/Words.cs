using PhosphorescenceModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// Contains information of all the possible words, and methods to generate code, in the plan of new potential words.
/// </summary>
namespace PhosphorescenceModule
{
    internal static class Words
    {
        /// <summary>
        /// This determines how long the sequences are.
        /// </summary>
        internal const int SequenceLength = 8;

        /// <summary>
        /// This determines the amount of words needed in the array for the array to be considered usable.
        /// </summary>
        internal const int MinAcceptableWordSet = 5;

        /// <summary>
        /// Contains all distinct characters within the button colors enum.
        /// </summary>
        internal static string ValidAlphabet { get; private set; }

        /// <summary>
        /// Contains the characters submitted for all indexes.
        /// </summary>
        internal static string[] ValidChars { get; private set; }

        /// <summary>
        /// Contains all words. Indexes represent the same indexes in _validChars.
        /// This means that index 1 should contain words like "ad", from _validChars' "erlaeyhl" -> "deuglaia" (index 1-2).
        /// </summary>
        internal static string[][] ValidWords { get; private set; }

        /// <summary>
        /// Contains the same value as ValidWords, with only unique entries, truncated to a 1-dimensional array.
        /// </summary>
        internal static string[] ValidDistinctWords { get; private set; }

        /// <summary>
        /// Returns the array of all colors from ButtonType in a Color32 equivalent datatype.
        /// </summary>
        internal static Color32[] Colors { get; private set; }

        private static bool _isThreadReady = false;
        private static string[][] _resultsFromThread = null;

        /// <summary>
        /// Sets the valid words property to all words that are valid.
        /// </summary>
        internal static IEnumerator Init(this TextAsset words)
        {
            PhosphorescenceModule.Init.isFirstToGenerate = false;

            // If the method has run at least once, we do not need to initalize this again.
            if (ValidWords != null)
                yield break;

            ValidChars = Enum.GetNames(typeof(ButtonType)).GetAllChars();
            ValidAlphabet = ValidChars.Join("").Distinct().OrderBy(c => c).Join("");
            Colors = Enum.GetValues(typeof(ButtonType)).Cast<ButtonType>().IterateColors();

            yield return words.StartThread();

            if (Application.isEditor)
            {
                var thread = new Thread(() => Log());
                thread.Start();
            }
        }

        internal static IEnumerator StartThread(this TextAsset file)
        {
            _isThreadReady = false;
            string[] words = file.text.Split('\n');

            var thread = new Thread(() => words.GetAllWords(ValidChars));
            thread.Start();
            yield return new WaitUntil(() => _isThreadReady);

            ValidDistinctWords = _resultsFromThread.Flatten().Distinct().ToArray();
            ValidWords = _resultsFromThread;
        }

        /// <summary>
        /// Determines whether another word can be constructed that includes the impostor letter.
        /// </summary>
        /// <param name="impostor">The impostor letter.</param>
        /// <param name="solution">The current solution.</param>
        /// <returns>True if the impostor character cannot create a valid word.</returns>
        internal static bool IsValidImpostor(this char impostor, string solution)
        {
            if (solution.Select(c => ValidAlphabet.IndexOf(c)).Any(i => Math.Abs(i - ValidAlphabet.IndexOf(impostor)) <= 1))
                return false;

            foreach (char potentialImpostor in solution.Distinct())
            {
                if (ValidDistinctWords.Contains(solution.Select(c => c != potentialImpostor).Join("")))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Calculates all possible answers of the module.
        /// </summary>
        /// <param name="solution">The word to reach to.</param>
        /// <param name="index">The offset index that is used for the colors.</param>
        /// <returns>A string array where every element is a valid answer.</returns>
        internal static string[] GetAllAnswers(this ButtonType[] buttons, string solution, int index)
        {
            // Initalize list.
            List<string>[] answers = new List<string>[solution.Length];
            for (int i = 0; i < answers.Length; i++)
                answers[i] = new List<string>();

            // For each character.
            for (int i = 0; i < solution.Length; i++)
            {
                // For each color's word.
                foreach (string button in buttons.Select(b => b.ToString()))
                {
                    // Would pushing the button be valid?
                    if (solution[i] == button[(index + i) % button.Length].ToString().ToLowerInvariant()[0])
                    {
                        // Since Blue and Black both share the same first letter, K is used instead for black.
                        string nextAnswer = button[0].ToString();

                        // If this is not the first iteration, we need to add answers based on all the previous answers as well.
                        if (i == 0)
                            answers[i].Add(nextAnswer);
                        else
                            foreach (string answer in answers[i - 1])
                                answers[i].Add(answer + nextAnswer);
                    }
                }
            }

            return answers[answers.Length - 1].ToArray();
        }

        /// <summary>
        /// Gets all characters that are valid.
        /// </summary>
        /// <returns>String array of all characters that are valid per index.</returns>
        internal static string[] GetAllChars(this string[] colors)
        {
            string[] output = new string[colors.Select(c => c.Length).ToArray().LeastCommonDenominator()];

            for (int i = 0; i < output.Length; i++)
                output[i] = colors.Select(s => s[i % s.Length]).Join("").ToLowerInvariant();

            return output;
        }

        /// <summary>
        /// Gets all words that are valid.
        /// </summary>
        /// <returns>Jagged array of all valid words, indexes correlating with the valid characters array.</returns>
        internal static void GetAllWords(this string[] words, string[] validChars)
        {
            // This is simply the directory of a large word list of strictly nouns.
            string[][] output = new string[ValidChars.Length][];

            // Tests for all offsets in _validChars.
            for (int offset = 0; offset < ValidChars.Length; offset++)
                output[offset] = words.Where(w => w.IsValidWord(offset, validChars)).ToArray();

            _resultsFromThread = output.TrimAll();

            _isThreadReady = true;
        }

        /// <summary>
        /// Logs all information about the variables.
        /// </summary>
        private static void Log()
        {
            Debug.LogWarningFormat("Log method initalized. Dumping all variables... Make sure that you only receive at most 430 warnings! (Press the warning icon in the top-right part of the console to hide this logging!)");

            for (int i = 0; i < ValidWords.Length; i++)
                Debug.LogWarningFormat("{0}: {1}.", i, ValidWords[i].Join(", "));

            Debug.LogWarningFormat("Valid alphabet: {0}.", ValidAlphabet.Join(", "));
            Debug.LogWarningFormat("Valid character sequence: {0}.", ValidChars.Join(", "));

            Debug.LogWarningFormat("The shortest words are: {0}.", GetShortest().Join(", "));
            Debug.LogWarningFormat("The longest words are: {0}.", GetLongest().Join(", "));

            Debug.LogWarningFormat("The smallest length of a given index is: {0}", GetShortestLength());
            Debug.LogWarningFormat("The longest length of a given index is: {0}", GetLongestLength());

            Debug.LogWarningFormat("The indices that don't meet the required {0} length are: {1}", MinAcceptableWordSet, ValidWords.Where(a => a.Length < MinAcceptableWordSet).Select(a => Array.IndexOf(ValidWords, a)).Join(", "));

            Debug.LogWarningFormat("The amount of distinct to total words are: {0}/{1}.", GetCount(distinct: true), GetCount(distinct: false));
            Debug.LogWarningFormat("The words that are completely unique are: {0}.", ValidDistinctWords.GroupBy(x => x).Where(g => g.Count() == 1).Select(y => y.Key).Join(", "));
        }

        /// <summary>
        /// Tests if the word is valid, both in length, and each character being part of the valid string array.
        /// </summary>
        /// <param name="line">The string to test.</param>
        /// <param name="offset">The starting index for _validChars</param>
        /// <returns>True if the word is valid for the ValidWords array.</returns>
        private static bool IsValidWord(this string line, int offset, string[] validChars)
        {
            // Ensures lack of whitespace and capitalization.
            line = line.Trim();

            // This requires words to be 3 to 6 letters long.
            if (line.Length < 3 || line.Length > SequenceLength - 2)
                return false;

            // This requires words to contain only the letters provided in the current index + amount of characters before it in _validChars.
            for (int i = 0; i < line.Length; i++)
            {
                if (!validChars[(offset + i) % validChars.Length].Contains(line[i].ToString()))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Finds the least common denominator of all integers in the array.
        /// </summary>
        /// <param name="array">The list of numbers to find the least common denominator with.</param>
        /// <returns>Integer of least common denominator in the whole array.</returns>
        private static long LeastCommonDenominator(this int[] array)
        {
            int lcm = 1, divisor = 2;

            while (true)
            {
                bool divisible = false;
                int counter = 0;

                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == 0)
                        return 0;
                    else if (array[i] < 0)
                        array[i] = array[i] * (-1);

                    if (array[i] == 1)
                        counter++;

                    if (array[i] % divisor == 0)
                    {
                        divisible = true;
                        array[i] = array[i] / divisor;
                    }
                }

                if (divisible)
                    lcm = lcm * divisor;
                else
                    divisor++;

                if (counter == array.Length)
                    return lcm;
            }
        }

        /// <summary>
        /// Returns all of the shortest words in ValidWords.
        /// </summary>
        /// <returns>The shortest string(s) in ValidWords.</returns>
        private static IEnumerable<string> GetShortest()
        {
            List<string> shortestWord = new List<string>() { ValidWords.First().First() };

            for (int i = 0; i < ValidWords.Length; i++)
            {
                for (int j = 0; j < ValidWords[i].Length; j++)
                {
                    if (shortestWord[0].Length > ValidWords[i][j].Length)
                        shortestWord = new List<string>() { ValidWords[i][j] };

                    else if (shortestWord[0].Length == ValidWords[i][j].Length)
                        shortestWord.Add(ValidWords[i][j]);
                }
            }

            return shortestWord.Distinct();
        }

        /// <summary>
        /// Returns all of the longest words in ValidWords.
        /// </summary>
        /// <returns>The longest string(s) in ValidWords.</returns>
        private static IEnumerable<string> GetLongest()
        {
            List<string> longestWord = new List<string>() { string.Empty };

            for (int i = 0; i < ValidWords.Length; i++)
            {
                for (int j = 0; j < ValidWords[i].Length; j++)
                {
                    if (longestWord[0].Length < ValidWords[i][j].Length)
                        longestWord = new List<string>() { ValidWords[i][j] };

                    else if (longestWord[0].Length == ValidWords[i][j].Length)
                        longestWord.Add(ValidWords[i][j]);
                }
            }

            return longestWord.Distinct();
        }

        /// <summary>
        /// Returns the length of the shortest subarray in ValidWords.
        /// </summary>
        /// <returns>The length of the shortest subarray in ValidWords.</returns>
        private static int GetShortestLength()
        {
            int shortest = ValidWords.First().Length;

            for (int i = 1; i < ValidWords.Length; i++)
            {
                if (shortest > ValidWords[i].Length)
                    shortest = ValidWords[i].Length;
            }

            return shortest;
        }

        /// <summary>
        /// Returns the length of the longest subarray in ValidWords.
        /// </summary>
        /// <returns>The length of the longest subarray in ValidWords.</returns>
        private static int GetLongestLength()
        {
            int longest = ValidWords.First().Length;

            for (int i = 1; i < ValidWords.Length; i++)
            {
                if (longest < ValidWords[i].Length)
                    longest = ValidWords[i].Length;
            }

            return longest;
        }

        /// <summary>
        /// Returns the amount of unique words, flattening the ValidWords array.
        /// </summary>
        /// <returns>The amount of unique entries in ValidWords.</returns>
        private static int GetCount(bool distinct)
        {
            return distinct ? ValidWords.Flatten().Distinct().Count() : ValidWords.Flatten().Count();
        }

        /// <summary>
        /// Flattens a jagged array.
        /// </summary>
        /// <param name="array">The array to flatten.</param>
        /// <returns>Returns a flattened 1-dimensional array of the jagged 2-dimensional array provided.</returns>
        private static string[] Flatten(this string[][] array)
        {
            return array.SelectMany(a => a).ToArray();
        }

        /// <summary>
        /// Tests if the two strings provided are anagrams of each other.
        /// </summary>
        /// <param name="a">The first string to test.</param>
        /// <param name="b">The second string to test.</param>
        /// <returns>True if both strings are anagrams, otherwise false.</returns>
        private static bool IsAnagram(string a, string b)
        {
            return string.Concat(a.OrderBy(c => c)).Equals(string.Concat(b.OrderBy(c => c)));
        }
    }
}
