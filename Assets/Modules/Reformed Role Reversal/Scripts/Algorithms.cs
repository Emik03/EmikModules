using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Contains all algorithms for use in manual creation.
/// </summary>
namespace ReformedRoleReversalModule
{
    static class Algorithms
    {
        /// <summary>
        /// Finds and returns the index of the wires that match the method used.
        /// </summary>
        /// <param name="method">The method used to locate a match.</param>
        /// <param name="key">The key that is used as comparison for methods with the word 'Key'.</param>
        /// <param name="wires">The array to search with.</param>
        /// <returns>The index of the wire to cut.</returns>
        internal static int? Find(string method, ref int key, int[] wires)
        {
            switch (method)
            {
                // Returns the position of the first index of the array that is equal to the key provided.
                case "firstInstanceOfKey":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] == key)
                            return ++i;
                    break;

                // Returns the position of the first index of the array that is NOT equal to the key provided.
                case "firstInstanceOfNotKey":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] != key)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is equal to the key provided.
                case "lastInstanceOfKey":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] == key)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is NOT equal to the key provided.
                case "lastInstanceOfNotKey":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] != key)
                            return ++i;
                    break;

                // Returns the position of the first index of the array that is equal to the opposite value of the key.
                case "firstInstanceOfOppositeKey":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] == (key + 5) % 10)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is equal to the opposite value of the key.
                case "lastInstanceOfOppositeKey":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] == (key + 5) % 10)
                            return ++i;
                    break;

                // Returns the position of the first index of the array that is equal to the range that is blue in Arrays.GroupedColors.
                case "firstInstanceOfBlue":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] < 5)
                            return ++i;
                    break;

                // Returns the position of the first index of the array that is equal to the range that is purple in Arrays.GroupedColors.
                case "firstInstanceOfPurple":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] >= 5)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is equal to the range that is blue in Arrays.GroupedColors.
                case "lastInstanceOfBlue":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] < 5)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is equal to the range that is purple in Arrays.GroupedColors.
                case "lastInstanceOfPurple":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] >= 5)
                            return ++i;
                    break;

                // Returns the position of the first index of the array that is an even number.
                case "firstEven":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] % 2 == 0)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is an even number.
                case "lastEven":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] % 2 == 0)
                            return ++i;
                    break;

                // Returns the position of the first index of the array that is an odd number.
                case "firstOdd":
                    for (int i = 0; i < wires.Length; i++)
                        if (wires[i] % 2 == 1)
                            return ++i;
                    break;

                // Returns the position of the last index of the array that is an odd number.
                case "lastOdd":
                    for (int i = wires.Length - 1; i >= 0; i--)
                        if (wires[i] % 2 == 1)
                            return ++i;
                    break;

                // Failsafe: If the programmer misspells the algorithm, or hadn't implemented it.
                default: throw new NotImplementedException("Could not find '" + method + "' for Algorithms.Find(), did you misspell the string?");
            }

            // The wire was unable to be found, and the condition is skipped as a result.
            return null;
        }

        /// <summary>
        /// Returns the earliest index of an array that isn't null.
        /// </summary>
        /// <param name="array">The array used to locate the smallest number.</param>
        /// <returns>The smallest number that isn't null.</returns>
        internal static int? First(int?[] array)
        {
            // Any wire that is found will be lower than 10.
            int? min = 10;

            for (int i = 0; i < array.Length; i++)
                if (min > array[i] && array[i] != null)
                    min = array[i];

            // If no wire is found, return null.
            return min == 10 ? null : min;
        }

        /// <summary>
        /// Returns the earliest index of a list that isn't 0.
        /// </summary>
        /// <param name="array">The array used to locate the smallest number.</param>
        /// <returns>The smallest number not equal to 0.</returns>
        internal static int First(List<int> array)
        {
            // Any wire that is found will be lower than 10.
            int min = 10;

            for (int i = 0; i < array.Count; i++)
                if (min > array[i] && array[i] != 0)
                    min = array[i];

            // If no wire is found, return 0.
            return min == 10 ? 0 : min;
        }

        /// <summary>
        /// Returns the values not matching the string and int array.
        /// </summary>
        /// <param name="wires">The wires of the module.</param>
        /// <param name="seed">The seed, which contains external wires.</param>
        /// <param name="leftmost">Whether to check leftmost.</param>
        /// <param name="amount">The amount to find.</param>
        /// <param name="lookup">The lookup, since it has to be reverted.</param>
        /// <returns></returns>
        internal static int[] AppendFromArray(int[] wires, ref string seed, bool leftmost, int amount, int lookup)
        {
            int[] revertedWires = RevertLookup(wires, ref lookup);
            List<int> wiresFound = new List<int>();
            StringBuilder newSeed = new StringBuilder(seed);

            for (int i = 0; i < seed.Length; i++)
            {
                if (char.IsDigit(leftmost ? seed[i] : seed[seed.Length - i - 1]) &&
                    !revertedWires.Contains((int)char.GetNumericValue(seed[leftmost ? i : seed.Length - i - 1])))
                {
                    newSeed.Remove(i - (seed.Length - newSeed.Length), 1);
                    wiresFound.Add((int)(char.GetNumericValue(seed[leftmost ? i : seed.Length - i - 1]) + lookup) % 10);
                }

                if (wiresFound.Count == amount)
                    break;
            }

            // We need it in reading order, and this has been extracted right-to-left.
            if (!leftmost)
                wiresFound.Reverse();

            seed = newSeed.ToString();
            return wiresFound.Count == 0 ? null : wiresFound.ToArray();
        }

        /// <summary>
        /// Adds vertical bar characters which are placeholders for line breaks to the submitted string.
        /// </summary>
        /// <param name="text">The text to add line breaks with.</param>
        /// <returns>A modified string containing vertical bars.</returns>
        internal static string Format(string text)
        {
            // 27 is the most amount of characters that can be fit inside the screen.
            const byte jump = 27;
            ushort index = jump;
            StringBuilder sb = new StringBuilder(text);

            while (index < text.Length)
            {
                if (text[index] == ' ')
                {
                    sb[index] = '\n';
                    index += jump;
                }
                else
                    index--;
            }

            return sb.ToString();
        }

        /// <summary>
        /// An optimized method using an array as buffer instead of string concatenation. 
        /// This is faster for return values having a length > 1.
        /// </summary>
        internal static string ConvertFromBase10(int value, char[] baseChars)
        {
            // 32 is the worst cast buffer size for base 2 and int.MaxValue.
            int i = 32;
            char[] buffer = new char[i];

            do
            {
                buffer[--i] = baseChars[value % baseChars.Length];
                value = value / baseChars.Length;
            }
            while (value > 0);

            char[] result = new char[32 - i];
            Array.Copy(buffer, i, result, 0, 32 - i);

            return new string(result);
        }

        /// <summary>
        /// Returns the amount of colors for each group.
        /// </summary>
        /// <param name="grouped">Whether or not to return blueish against purpleish, or every color individually.</param>
        /// <param name="wires">The array to search with.</param>
        /// <returns>The list of colors, with size 2 or 10 depending on the 'grouped' boolean.</returns>
        internal static int[] GetColors(bool grouped, int[] wires)
        {
            int[] colors = grouped ? new int[2] { 0, 0 } : new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // Arrays.GroupedColors
            if (grouped)
                for (int i = 0; i < wires.Length; i++)
                    colors[wires[i] / 5]++;

            // Arrays.Colors
            else
                for (int i = 0; i < wires.Length; i++)
                    colors[wires[i]]++;

            return colors;
        }

        /// <summary>
        /// Returns an array of random unique numbers with a few parameters.
        /// </summary>
        /// <param name="length">The length of the array.</param>
        /// <param name="min">The included minimum value.</param>
        /// <param name="max">The excluded maximum value.</param>
        /// <returns>A random integer array.</returns>
        internal static int[] Random(int length, int min, int max)
        {
            // Create a range from min to max, and initalize an array with the specified size.
            int[] range = Enumerable.Range(min, --max).ToArray().Shuffle();

            // Failsafe: Should never happen, and will return unnatural values otherwise.
            if (range.Length < length)
                throw new ArgumentOutOfRangeException("range: " + range.Join(", "), "The length of the returned array (" + length + ") is larger than the range specified (" + range.Length + ")!");

            // Instance can be pulled linearly since the range has been shuffled anyway.
            return SubArray(range, 0, length);
        }

        /// <summary>
        /// Creates and returns a subarray of the given array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="data">The array itself to which to create a subarray of.</param>
        /// <param name="index">The inclusive starting index.</param>
        /// <param name="length">The length of the copy.</param>
        /// <returns></returns>
        internal static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        /// <summary>
        /// Returns a new copy of the array, which will not be referenced.
        /// </summary>
        /// <typeparam name="T">The type the array it is.</typeparam>
        /// <param name="array">The array to copy from.</param>
        /// <returns>A new independant copy of the array provided.</returns>
        internal static T[] Clone<T>(this T[] array)
        {
            var newArray = new T[array.Length];
            for (var i = 0; i < array.Length; i++)
                newArray[i] = array[i];
            return newArray;
        }

        /// <summary>
        /// Creates and returns an array consisting of the same values.
        /// </summary>
        /// <typeparam name="T">The type of array it is.</typeparam>
        /// <param name="value">The value to use.</param>
        /// <param name="length">The length of the array.</param>
        /// <returns>The array consisting of the value provided on each index.</returns>
        internal static T[] ArrayFromInt<T>(this T value, int length)
        {
            T[] array = new T[length];
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
            return array;
        }

        /// <summary>
        /// Undoes the lookup offset by subtracting each index in the wires with the lookup.
        /// </summary>
        /// <param name="array">The array to apply to.</param>
        /// <param name="number">The lookup offset to apply with.</param>
        internal static int[] RevertLookup(int[] array, ref int number)
        {
            int[] newArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                newArray[i] = (array[i] - number + 10) % 10;
            return newArray;
        }
    }
}
