using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ForgetAnyColor
{
    /// <summary>
    /// Handles calculating and computing the module's answer based on its appearance when it's called for.
    /// </summary>
    public class Calculate
    {
        public Calculate(FACScript FAC, Init init)
        {
            this.FAC = FAC;
            this.init = init;

            figureSequences = new List<int>();
            sequences = new List<bool?>();
            modifiedSequences = new List<bool>();
        }

        internal List<int> figureSequences;
        internal List<bool?> sequences;
        internal List<bool> modifiedSequences;

        private readonly FACScript FAC;
        private readonly Init init;
        private bool lastInput;

        internal void Current()
        {
            string display;
            string[] figure;
            IEnumerable<string> unique;

            GetFigures(out display, out unique, out figure);

            string random = unique.PickRandom();

            if (random.Length != 3)
                throw new ArgumentOutOfRangeException("random", random);

            FAC.NixieTexts[0].text = random[0].ToString();
            FAC.NixieTexts[1].text = random[1].ToString();
            FAC.GearText.text = random[2].ToString();

            int figureUsed = figure.ToList().IndexOf(random);
            figureSequences.Add(figureUsed);

            bool? input = new bool?[] { false, null, true }[figureUsed % 3];
            sequences.Add(input);

            bool modifiedInput = input == null ? !lastInput : (bool)input;

            modifiedSequences.Add(lastInput = modifiedInput);

            Debug.LogFormat("[Forget Any Color #{0}]: Stage {1} = {2} => {3}. Press {4}.",
                init.moduleId,
                init.stage + 1,
                display,
                new[] { "LLLMR", "LMMMR", "LMRRR", "LMMRR", "LLMRR", "LLMMR" }[figureUsed],
                modifiedInput ? "Right" : "Left");
        }

        private void GetFigures(out string display, out IEnumerable<string> unique, out string[] figure)
        {
        startOver:
            int edgework = Init.rules.GetLength(0) != 0 ? Arrays.GetEdgework(Init.rules[1][Functions.GetColorIndex(3, FAC)].Number, FAC)
                                                        : Edgework(Functions.GetColorIndex(3, FAC));

            display = FAC.DisplayText.text.Remove(edgework == 0 ? 5 : --edgework % 6, 1);

            if (display.Length != 5)
                throw new ArgumentOutOfRangeException("display", display);

            int[] decimals = new int[5], temp = Array.ConvertAll(display.ToCharArray(), c => (int)char.GetNumericValue(c));
            Array.Copy(temp, decimals, temp.Length);

            figure = new string[6];

            for (int i = 0; i < 6; i++)
            {
                int[][] cylinders = Figure.Create(decimals, ref i);
                int[] sums = Figure.Apply(cylinders, FAC);

                figure[i] = sums.Join("");
            }

            unique = figure.GroupBy(x => x).Where(g => g.Count() == 1).Select(y => y.Key);

            if (unique.Count() == 0)
            {
                init.render.AssignRandom(true);
                goto startOver;
            }
        }

        private int Edgework(int index)
        {
            if (Init.rules.GetLength(0) != 0)
                return Arrays.GetEdgework(Init.rules[1][init.render.GetGear()[1]].Number, FAC);

            switch (index)
            {
                case 0: return FAC.Info.GetBatteryCount();
                case 1: return FAC.Info.GetIndicators().Count();
                case 2: return FAC.Info.GetPortPlateCount();
                case 3: return FAC.Info.GetSerialNumberNumbers().First();
                case 4: return FAC.Info.GetBatteryHolderCount();
                case 5: return FAC.Info.GetOffIndicators().Count();
                case 6: return FAC.Info.GetPorts().Count();
                case 7: return FAC.Info.GetSerialNumberLetters().Count();

                default: throw new IndexOutOfRangeException("Calculate.Edgework recieved an out-of-range number: " + index + ".");
            }
        }
    }
}
