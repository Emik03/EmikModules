using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Rnd = System.Random;

/// <summary>
/// Generates the module and caches the answer.
/// </summary>
namespace ReformedRoleReversalModule
{
    internal class HandleManual
    {
        internal HandleManual(ReformedRoleReversalCoroutineHandler coroutines, Init init)
        {
            this.coroutines = coroutines;
            this.init = init;
            reversal = init.Reversal;
            interact = init.Interact;
        }

        internal int[] SouvenirWires;
        internal readonly int[] SouvenirIndex = new int[2];

        protected internal string Seed = rnd.Next(0, 1000000000).ToString();

        private readonly ReformedRoleReversalCoroutineHandler coroutines;
        private readonly Init init;
        private readonly Interact interact;
        private readonly ReformedRoleReversal reversal;

        private Condition[] tutorial;
        private static readonly Rnd rnd = new Rnd();
        private int generated;

        private static readonly List<MethodInfo> 
            _conditionMethods = new List<MethodInfo>(),
            _firstConditionMethods = new List<MethodInfo>(),
            _lastConditionMethods = new List<MethodInfo>();

        /// <summary>
        /// Converts the random number generated into wires, and a seed for the module to display.
        /// </summary>
        protected internal void Generate()
        {
            // Display loading screen.
            coroutines.LoadingScreen(0);

            // Generate random parameters as rules.
            bool left = rnd.NextDouble() > 0.5, leftmost = rnd.NextDouble() > 0.5, discard = rnd.NextDouble() > 0.5, append = rnd.NextDouble() > 0.5;

            // Random lookup table, which is simply the default with all values added to this variable.
            int lookup = rnd.Next(0, 10), mod = rnd.Next(3, 8), add = rnd.Next(3, 11 - mod);

            // Get random base. Base 20 is minimum because it never displays more than 7 characters.
            char[] baseN = Algorithms.SubArray(Arrays.Base62, 0, rnd.Next(20, 63));

            // Assign the seed to the wires.
            int[] wires = GetWires(ref left, ref leftmost, ref lookup, ref mod, ref add, ref baseN);

            if (Application.isEditor)
                Override(out wires, out lookup, out mod, out add, out left, out leftmost, out discard, out append, out baseN);

            int i2 = init.Conditions.GetLength(0);

            // Formats the tutorial, this needs to run before the conditions are generated because it assigns the first set using this variable.
            tutorial = new Arrays(reversal.Info).GetTutorial(interact.ButtonOrder, baseN.Length, ref left, ref mod, ref add, ref leftmost, ref lookup, ref discard, ref append);

            // If the list of methods is unassigned, generate new ones. This is in case there are multiple Reformed Role Reversals.
            if (_conditionMethods.Count == 0)
                GetManualMethods();

            // Runs through the entire 2-dimensional array and assign a condition to each and every single one.
            for (int i = 0; i < i2; i++)
                coroutines.GenerateSetOfConditions(i, wires, ref lookup, ref discard, ref append);
        }

        private void Override(out int[] wires, out int lookup, out int mod, out int add, out bool left, out bool leftmost, out bool discard, out bool append, out char[] baseN)
        {
            lookup = 4;
            mod = 5;
            add = 4;
            left = false;
            leftmost = false;
            discard = false;
            append = false;
            Seed = "000010000";
            baseN = Algorithms.SubArray(Arrays.Base62, 0, 41);
            wires = GetWires(ref left, ref leftmost, ref lookup, ref mod, ref add, ref baseN);
        }

        /// <summary>
        /// Transforms the string seed into an integer array, with the rules based on the parameters described.
        /// </summary>
        /// <param name="left">Whether 0's are appended to the left.</param>
        /// <param name="leftmost">Whether wires get grabbed by leftmost.</param>
        /// <param name="lookup">The offset applied before changing numbers to colors.</param>
        /// <param name="baseN">Partial subarray of 0-9A-Za-z that indicates the base.</param>
        /// <returns></returns>
        private int[] GetWires(ref bool left, ref bool leftmost, ref int lookup, ref int mod, ref int add, ref char[] baseN)
        {
            // 10% of the time, the string is less than 9 characters long. Append accordingly.
            while (Seed.Length < 9)
                Seed = left ? '0' + Seed : Seed + '0';

            // The amount of wires is calculated with modulo 3 to 7, and then add 3 to 10-modulo (inclusive).
            int[] wires = new int[(int.Parse(Seed) % mod) + add];

            for (int i = 0; i < wires.Length; i++)
                wires[i] = (int)(char.GetNumericValue(Seed[leftmost ? i : i + (9 - wires.Length)]) + lookup) % 10;

            // Converts the seed from base 10 to the random base chosen.
            reversal.SeedText.text = "Seed: " + Algorithms.ConvertFromBase10(value: int.Parse(Seed), baseChars: baseN);

            Debug.LogFormat("[Reformed Role Reversal #{0}]: {1}: Base-{2} Seed: {3}. Base-10 Seed: {4}. Mod: {5}. Add: {6}. # Wires: {7}. Place {8} 0's. Take {9}. Lookup: #{10}.", init.ModuleId, Arrays.Version, baseN.Length, reversal.SeedText.text.Substring(6, reversal.SeedText.text.Length - 6), Seed, mod, add, wires.Length, left ? "left" : "right", leftmost ? "leftmost" : "rightmost", lookup);

            // Log the list of all wires, converting each index to the respective string.
            string[] log = new string[wires.Length];

            for (int i = 0; i < wires.Length; i++)
                log[i] += i == wires.Length - 1 ? "and " + Arrays.Colors[wires[i]] : Arrays.Colors[wires[i]];

            Debug.LogFormat("[Reformed Role Reversal #{0}]: The wires are {1}.", init.ModuleId, log.Join(", "));

            return wires;
        }

        /// <summary>
        /// Retrieves all methods from the 'Manual' class file, and appends it to method lists.
        /// </summary>
        private static void GetManualMethods()
        {
            foreach (MethodInfo method in typeof(Manual).GetMethods())
            {
                if (method.ReturnType != typeof(Condition))
                    continue;
                if (method.Name.StartsWith("First"))
                    _firstConditionMethods.Add(method);
                else if (method.Name.StartsWith("Last"))
                    _lastConditionMethods.Add(method);
                else if (method.Name != "ReturnEmptyCondition")
                    _conditionMethods.Add(method);
            }
        }

        /// <summary>
        /// Generates a condition and sets the currently assigned variable to it.
        /// </summary>
        /// <param name="i">The index of the first dimension.</param>
        /// <param name="j">The index of the second dimension.</param>
        /// <param name="wires">The list of wires.</param>
        /// <param name="Seed">The seed converted to a string, very similar to wires.</param>
        /// <param name="lookup">This variable is needed in case if the lookup offset needs to be reverted.</param>
        /// <param name="isCorrectIndex">To prevent having the user find out the amount of wires by carefully reading the conditions, the wires specified are adjusted per section.</param>
        /// <returns>This is meant for multithreading, and therefore only returns null.</returns>
        protected internal IEnumerator GenerateCondition(int i, int j, int[] wires, int lookup, bool discard, bool append, bool isCorrectIndex)
        {
            // In case they get replaced with fake ones.
            int[] realWires = Algorithms.Clone(wires);

            // If the current condition is in the tutorial section, assign it to the tutorial already generated before.
            if (i == 0)
            {
                // Clones the tutorial.
                init.Conditions[i, j] = tutorial[j];

                // Theoretically generateCondition++ could run before the previous instruction has finished running.
                yield return new WaitWhile(() => init.Conditions[i, j] == null);
                coroutines.LoadingScreen(++generated);

                yield break;
            }

            // Generates fake wires for sections with incorrect amount of wires to obfuscate real ones based on the conditions recieved.
            if (!isCorrectIndex)
                wires = Enumerable.Repeat(0, i + 2).Select(k => rnd.Next(0, 10)).ToArray();

            MethodInfo methodInfo;

            object[] variables = new object[] { wires, lookup, reversal.Info, isCorrectIndex },
                     specialVariables = new object[] { wires, Seed, lookup, discard, reversal.Info, j == 0, isCorrectIndex };

            bool isSecondSpecial = discard ? (j == 1 && i != 1) : (j == 1 && i != 7);

            switch (j)
            {
                case 0: methodInfo = _firstConditionMethods[rnd.Next(0, _firstConditionMethods.Count)]; break;

                case 1:
                    methodInfo = isSecondSpecial ? _firstConditionMethods[rnd.Next(0, _firstConditionMethods.Count)]
                                                 : _conditionMethods[rnd.Next(0, _conditionMethods.Count)]; break;

                case 7: methodInfo = _lastConditionMethods[rnd.Next(0, _lastConditionMethods.Count)]; break;

                default: methodInfo = _conditionMethods[rnd.Next(0, _conditionMethods.Count)]; break;
            }

            // Invoke the random method obtained and assign it into the current variable.
            init.Conditions[i, j] = (Condition)methodInfo.Invoke(this, j == 0 || isSecondSpecial ? specialVariables : variables);

            // Wait until the method has finished running.
            yield return new WaitWhile(() => init.Conditions[i, j] == null);

            // If the conditions are regenerating, it shouldn't give a tell by flickering the screen.
            yield return new WaitForSeconds((float)rnd.NextDouble() / 10 * Convert.ToByte(!(generated % 8 == 0 && generated >= init.Conditions.GetLength(0) * init.Conditions.GetLength(1))));
            coroutines.LoadingScreen(++generated);

            // Reset it.
            bool hasGenerated = generated % 8 == 0 && generated >= init.Conditions.GetLength(0) * init.Conditions.GetLength(1);

            // If this is the last time the coroutine is running, get the answer, and consider the module ready.
            if (hasGenerated)
                interact.CorrectAnswer = GetAnswer(realWires, ref lookup, ref discard, ref append);
        }

        /// <summary>
        /// Scans through the condition's properties to determine the answer of the module.
        /// </summary>
        /// <param name="Seed">The seed in base 10.</param>
        /// <returns>Returns the answer, if the answer is null then any wire can be cut.</returns>
        private int? GetAnswer(int[] wires, ref int lookup, ref bool discard, ref bool append)
        {
            int wireSelected = 1, wireCount = wires.Length - 2, iMax = init.Conditions.GetLength(1);
            bool isSelectingWire = false;
            string[] discardValues = new[] { "-2", "-1", "1", "2" };

            coroutines.UpdateScreen(instructionX: 0, instructionY: 0, wireSelected: ref wireSelected, isSelectingWire: ref isSelectingWire);

            bool shouldRun = wireCount >= 0 && wireCount <= 7;
            for (int i = 0; i < iMax && shouldRun; i++)
            {
                // If true, set the current index to the Skip property.
                if (init.Conditions[wireCount, i].Skip != null)
                {
                    int? skipValue = init.Conditions[wireCount, i].Skip;
                    if (skipValue < 1 || skipValue > iMax)
                        throw new IndexOutOfRangeException("[Reformed Role Reversal #" + init.ModuleId + "]: Condition [" + wireCount + ", " + i + "] returned " + init.Conditions[wireCount, i].Skip + " for parameter \"Skip\"! This should not happen under normal circumstances, as the specified condition doesn't exist.");

                    Debug.LogFormat("[Reformed Role Reversal #{0}]: <Condition {1}, {2}> \"{3}\" is true, skip to section {4}.", init.ModuleId, wireCount + 2, i + 1, init.Conditions[wireCount, i].Text, init.Conditions[wireCount, i].Skip);
                    i = (int)skipValue - 1;
                }

                // If true, regenerate a set of conditions and refer the index to the new conditions.
                if (init.Conditions[wireCount, i].Discard != null)
                {
                    int? discardValue = init.Conditions[wireCount, i].Discard;
                    if (!discardValues.Contains(discardValue.ToString()))
                        throw new ArgumentOutOfRangeException("[Reformed Role Reversal #" + init.ModuleId + "]: Condition [" + wireCount + ", " + i + "] returned " + discardValue + " for parameter \"Discard\"! This should not happen under normal circumstances, as it should only return -2, -1, 1, or 2.");

                    Debug.LogFormat("[Reformed Role Reversal #{0}]: <Condition {1}, {2}> \"{3}\" is true, discard the {4}{5} wire{6}.", init.ModuleId, wireCount + 2, i + 1, init.Conditions[wireCount, i].Text, Math.Abs((int)discardValue) > 1 ? Math.Abs((int)discardValue) + " " : string.Empty, discardValue < 0 ? "leftmost" : "rightmost", Math.Abs((int)discardValue) > 1 ? "s" : string.Empty);

                    wires = Algorithms.SubArray(wires, discardValue < 0 ? Math.Abs((int)discardValue) : 0, wires.Length - Math.Abs((int)discardValue));

                    // Log the list of all wires, converting each index to the respective string.
                    string[] log = new string[wires.Length];

                    for (int j = 0; j < wires.Length; j++)
                        log[j] += j == wires.Length - 1 ? "and " + Arrays.Colors[wires[j]] : Arrays.Colors[wires[j]];

                    Debug.LogFormat("[Reformed Role Reversal #{0}]: The wires are now {1}.", init.ModuleId, log.Join(", "));

                    coroutines.GenerateSetOfConditions(wires.Length - 2, wires, ref lookup, ref discard, ref append);

                    // This method will run again from the generate set of conditions. An answer has not been determined yet.
                    return null;
                }

                // If true, regenerate a set of conditions and refer the index to the new conditions.
                if (init.Conditions[wireCount, i].Append != null && init.Conditions[wireCount, i].Append != null)
                {
                    int[] appendValue = init.Conditions[wireCount, i].Append;
                    int minValue = 10, maxValue = 0;
                    for (int j = 0; j < appendValue.Length; j++)
                    {
                        if (appendValue[j] < minValue)
                            minValue = appendValue[j];

                        if (appendValue[j] > maxValue)
                            maxValue = appendValue[j];
                    }

                    if (appendValue.Min() < 0 || appendValue.Max() > 9)
                        throw new ArgumentOutOfRangeException("[Reformed Role Reversal #" + init.ModuleId + "]: Condition [" + wireCount + ", " + i + "] returned " + init.Conditions[wireCount, i].Append.Join(", ") + " for parameter \"Append\"! This should not happen under normal circumstances, as all values should be between 0 through 9.");

                    Debug.LogFormat("[Reformed Role Reversal #{0}]: <Condition {1}, {2}> \"{3}\" is true, append the wires to the {4}.", init.ModuleId, wireCount + 2, i + 1, init.Conditions[wireCount, i].Text, append ? "left" : "right");

                    Array.Resize(ref wires, wires.Length + appendValue.Length);

                    while (Seed.Length < 9)
                        Seed = append ? 'X' + Seed : Seed + 'X';

                    // Append right.
                    if (!append)
                        Array.Copy(appendValue, 0, wires, wires.Length - appendValue.Length, appendValue.Length);
                    // Append left.
                    else
                    {
                        Array.Copy(wires, 0, wires, appendValue.Length, wires.Length - appendValue.Length);
                        Array.Copy(appendValue, 0, wires, 0, appendValue.Length);
                    }

                    // Log the list of all wires, converting each index to the respective string.
                    string[] log = new string[wires.Length];

                    for (int j = 0; j < wires.Length; j++)
                        log[j] += j == wires.Length - 1 ? "and " + Arrays.Colors[wires[j]] : Arrays.Colors[wires[j]];

                    Debug.LogFormat("[Reformed Role Reversal #{0}]: The wires are now {1}.", init.ModuleId, log.Join(", "));

                    coroutines.GenerateSetOfConditions(wires.Length - 2, wires, ref lookup, ref discard, ref append);

                    // This method will run again from the generate set of conditions. An answer has not yet been determined.
                    return null;
                }

                // If true, the answer has been reached, and the wire to cut is in the Wire property.
                if (init.Conditions[wireCount, i].Wire != null)
                {
                    SouvenirWires = wires;
                    SouvenirIndex[0] = wireCount;
                    SouvenirIndex[1] = i;

                    int? wireValue = init.Conditions[wireCount, i].Wire;
                    if (wireValue < 1 || wireValue > 9)
                        throw new IndexOutOfRangeException("[Reformed Role Reversal #" + init.ModuleId + "]: Condition [" + (wireCount + 2) + ", " + (i + 1) + "] returned " + wireValue + " for parameter \"Wire\"! This should not happen under normal circumstances, as the wire specified to cut doesn't exist.");

                    Debug.LogFormat("[Reformed Role Reversal #{0}]: <Condition {1}, {2}> \"{3}\" is true, cut the {4} wire.", init.ModuleId, wireCount + 2, i + 1, init.Conditions[wireCount, i].Text, Arrays.Ordinals[(int)wireValue - 1]);
                    init.Ready = true;
                    return (int)wireValue;
                }

                Debug.LogFormat("[Reformed Role Reversal #{0}]: <Condition {1}, {2}> \"{3}\" is false.", init.ModuleId, wireCount + 2, i + 1, init.Conditions[wireCount, i].Text);
            }

            // Failsafe: If the answer isn't found, any wire can be cut.
            Debug.LogWarningFormat("[Reformed Role Reversal #{0}]: An internal error has occured whilst trying to calculate the answer. Any submitted answer will solve the module.", init.ModuleId);
            init.Ready = true;
            return null;
        }
    }
}
