using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ForgetTheColors
{
    /// <summary>
    /// Calculates and logs the answer based on the information given.
    /// </summary>
    sealed class Calculate
    {
        public void FinalStage(ref int moduleId, ref int maxStage, ref List<int> calculatedValues, ref float sum, ref TextMesh[] NixieTexts, out bool canSolve, out sbyte solution)
        {
            Debug.LogFormat("[Forget The Colors #{0}]: FINAL STAGE", moduleId);
            Debug.LogFormat("[Forget The Colors #{0}]: <-------=-------> FINALE ~ ARCCOSINE <-------=------->", moduleId);

            //prevents out of array exceptions in editor
            if (Application.isEditor)
                maxStage = calculatedValues.Count;

            //adds all of the values
            for (byte i = 0; i < calculatedValues.Count; i++)
            {
                sum += calculatedValues[i];
                Debug.LogFormat("[Forget The Colors #{0}]: Adding stage {1}'s {2}, the total is now {3}.", moduleId, i, calculatedValues[i], sum);
            }

            //allow inputs in the module
            NixieTexts[0].text = "0";
            NixieTexts[1].text = "0";

            //turns into decimal number
            Debug.LogFormat("[Forget The Colors #{0}]: First five digits of cos-1({1}) is {2}.", moduleId, Ease.Modulo(Mathf.Abs(sum) / 100000, 1), Math.Truncate(Mathf.Acos((float)Ease.Modulo(Mathf.Abs(sum) / 100000, 1)) * Mathf.Rad2Deg));
            solution = (sbyte)(Mathf.Acos((float)Ease.Modulo(Mathf.Abs(sum) / 100000, 1)) * Mathf.Rad2Deg);

            Debug.LogFormat("[Forget The Colors #{0}]: The expected answer is {1}.", moduleId, solution);
            Debug.LogFormat("[Forget The Colors #{0}]: USER INPUT", moduleId);
            Debug.LogFormat("[Forget The Colors #{0}]: <-------=-------> LET'S SEE HOW THE USER DOES <-------=------->", moduleId);

            canSolve = true;
        }

        public void NextStage(ref int moduleId, ref int currentStage, ref TextMesh[] DisplayTexts, ref TextMesh[] NixieTexts, ref TextMesh GearText, ref bool colorblind, ref Rule[][] rules, ref int[] colorValues, ref KMBombInfo BombInfo, ref int maxStage, ref List<string> ruleColor, ref List<int> calculatedValues, ref List<int> sineNumber)
        {
            Debug.LogFormat("[Forget The Colors #{0}]: STAGE {1}", moduleId, currentStage);
            Debug.LogFormat("[Forget The Colors #{0}]: <-------=-------> MODULE APPEARANCE <-------=------->", moduleId);
            Debug.LogFormat("[Forget The Colors #{0}]: Large Display: {1}. Cylinders (left-to-right): {2}, {3}, and {4}. Nixies: {5} and {6}. Gear: {7} and {8}.", moduleId, DisplayTexts[0].text, Strings.ColorLog[colorValues[0]], Strings.ColorLog[colorValues[1]], Strings.ColorLog[colorValues[2]], NixieTexts[0].text, NixieTexts[1].text, GearText.text[Convert.ToByte(colorblind)], Strings.ColorLog[colorValues[3]]);
            Debug.LogFormat("[Forget The Colors #{0}]: <-------=-------> NIXIES ~ FIRST TABLE <-------=------->", moduleId);
            short nixie1 = sbyte.Parse(NixieTexts[0].text), nixie2 = sbyte.Parse(NixieTexts[1].text);

            if (rules == null)
            {
                //this will run through the changes applied to both nixie tubes during step 1 of second page on manual
                for (byte i = 0; i < colorValues.Length - 1; i++)
                {
                    //each digit rule
                    switch (colorValues[i])
                    {
                        case 0: nixie1 += 5; nixie2 -= 1; break;
                        case 1: nixie1 -= 1; nixie2 -= 6; break;
                        case 2: nixie1 += 3; break;
                        case 3: nixie1 += 7; nixie2 -= 4; break;
                        case 4: nixie1 -= 7; nixie2 -= 5; break;
                        case 5: nixie1 += 8; nixie2 += 9; break;
                        case 6: nixie1 += 5; nixie2 -= 9; break;
                        case 7: nixie1 -= 9; nixie2 += 4; break;
                        case 8: nixie2 += 7; break;
                        case 9: nixie1 -= 3; nixie2 += 5; break;
                    }
                    Debug.LogFormat("[Forget The Colors #{0}]: Applying the {1}-colored cylinder on the first table, the nixies are now {2} and {3}.", moduleId, Strings.ColorLog[colorValues[i]], nixie1, nixie2);
                }
            }

            else
            {
                for (byte i = 0; i < colorValues.Length - 1; i++)
                {
                    Rule rule = rules[0][colorValues[i]];

                    switch (rule.Operator)
                    {
                        case 0: nixie1 += (sbyte)rule.Cylinder; break;
                        case 1: nixie1 -= (sbyte)rule.Cylinder; break;
                        case 2: nixie1 *= (sbyte)rule.Cylinder; break;
                        case 3: if (nixie1 != 0) nixie1 = (sbyte)(nixie1 / rule.Cylinder); break;
                        case 4: if (rule.Cylinder != 0) nixie1 = (sbyte)Ease.Modulo(nixie1, rule.Cylinder); break;
                    }

                    rule = rules[0][colorValues[i] + 10];

                    switch (rule.Operator)
                    {
                        case 0: nixie2 += (sbyte)rule.Cylinder; break;
                        case 1: nixie2 -= (sbyte)rule.Cylinder; break;
                        case 2: nixie2 *= (sbyte)rule.Cylinder; break;
                        case 3: if (nixie2 != 0) nixie2 = (sbyte)(nixie2 / rule.Cylinder); break;
                        case 4: if (rule.Cylinder != 0) nixie2 = (sbyte)Ease.Modulo(nixie2, rule.Cylinder); break;
                    }

                    Debug.LogFormat("[Forget The Colors #{0}]: Applying the {1}-colored cylinder on the first table, the nixies are now {2} and {3}.", moduleId, Strings.ColorLog[colorValues[i]], nixie1, nixie2);
                }
            }

            //modulo
            nixie1 = (sbyte)Ease.Modulo(nixie1, 10);
            nixie2 = (sbyte)Ease.Modulo(nixie2, 10);
            Debug.LogFormat("[Forget The Colors #{0}]: Modulo 10, their values are now {1} and {2}.", moduleId, nixie1, nixie2);

            Debug.LogFormat("[Forget The Colors #{0}]: <-------=-------> GEAR NUMBER ~ SECOND TABLE <-------=------->", moduleId);

            //new gear = calculated nixies + gear
            int lsd = (byte)Ease.Modulo(nixie1 + nixie2 + int.Parse(GearText.text[Convert.ToByte(colorblind)].ToString()), 10);
            Debug.LogFormat("[Forget The Colors #{0}]: Combine both nixies ({1}&{2}) and the gear number {3}. The sum is {4}. Modulo 10, its value is {5}.", moduleId, nixie1, nixie2, GearText.text[Convert.ToByte(colorblind)], nixie1 + nixie2 + int.Parse(GearText.text[Convert.ToByte(colorblind)].ToString()), lsd);

            //move the index up and down according to calculated nixies
            Debug.LogFormat("[Forget The Colors #{0}]: Start on gear color ({1}), move up left nixie ({2}) which lands on {3}, then move down right nixie ({4}) which lands us on {5}.", moduleId, Strings.ColorLog[colorValues[3]], nixie1, Strings.ColorLog[(int)Ease.Modulo(colorValues[3] - nixie1, 10)], nixie2, Strings.ColorLog[(int)Ease.Modulo(colorValues[3] - nixie1 + nixie2, 10)]);

            if (rules == null)
            {
                //this will run through the changes applied to the gear during step 2 of second page on manual
                switch ((int)Ease.Modulo(colorValues[3] - nixie1 + nixie2, 10))
                {
                    case 0: lsd += BombInfo.GetBatteryCount(); break;
                    case 1: lsd -= BombInfo.GetPortCount(); break;
                    case 2: lsd += BombInfo.GetSerialNumberNumbers().Last(); break;
                    case 3: lsd -= BombInfo.GetSolvedModuleNames().Count(); break;
                    case 4: lsd += BombInfo.GetPortPlateCount(); break;
                    case 5: lsd -= BombInfo.GetModuleNames().Count(); break;
                    case 6: lsd += BombInfo.GetBatteryHolderCount(); break;
                    case 7: lsd -= BombInfo.GetOnIndicators().Count(); break;
                    case 8: lsd += BombInfo.GetIndicators().Count(); break;
                    case 9: lsd -= BombInfo.GetOffIndicators().Count(); break;
                }
            }

            else
            {
                string[] ports = new string[BombInfo.GetPorts().Count()];
                for (ushort i = 0; i < BombInfo.GetPorts().Count(); i++)
                    ports[i] = BombInfo.GetPorts().ElementAt(i);

                int ignoredCount = 0;
                foreach (string module in BombInfo.GetModuleNames())
                    if (Strings.Ignore.Contains(module))
                        ignoredCount++;

                Rule rule = rules[1][(int)Ease.Modulo(colorValues[3] - nixie1 + nixie2, 10)];

                //the smaller display is used as a debug displayer in the editor, which cannot be parsed
                int[] edgework = new int[22] { BombInfo.GetBatteryCount(), BombInfo.GetBatteryCount(Battery.AA) + BombInfo.GetBatteryCount(Battery.AAx3) + BombInfo.GetBatteryCount(Battery.AAx4), BombInfo.GetBatteryCount(Battery.D), BombInfo.GetBatteryHolderCount(), BombInfo.GetIndicators().Count(), BombInfo.GetOnIndicators().Count(), BombInfo.GetOffIndicators().Count(), BombInfo.GetPortPlateCount(), BombInfo.GetPorts().Distinct().Count(), BombInfo.GetPorts().Count() - BombInfo.GetPorts().Distinct().Count(), BombInfo.GetPortCount(), BombInfo.GetSerialNumberNumbers().First(), BombInfo.GetSerialNumberNumbers().Last(), BombInfo.GetSerialNumberNumbers().Count(), BombInfo.GetSerialNumberLetters().Count(), BombInfo.GetSolvedModuleNames().Count(), maxStage, BombInfo.GetModuleNames().Count(), BombInfo.GetSolvableModuleNames().Count() - BombInfo.GetSolvedModuleNames().Count(), ignoredCount, Application.isEditor ? 0 : int.Parse(DisplayTexts[1].text), int.Parse(DisplayTexts[0].text) };

                switch (rule.Operator)
                {
                    case 0: lsd += edgework[rule.Edgework]; break;
                    case 1: lsd -= edgework[rule.Edgework]; break;
                    case 2: lsd *= edgework[rule.Edgework]; break;
                    case 3: if (edgework[rule.Edgework] != 0) lsd = nixie2 / edgework[rule.Edgework]; break;
                    case 4: if (edgework[rule.Edgework] != 0) lsd = (int)Ease.Modulo(nixie2, edgework[rule.Edgework]); break;
                }
            }

            ruleColor[currentStage] = Strings.ColorLog[(int)Ease.Modulo(colorValues[3] - nixie1 + nixie2, 10)];
            Debug.LogFormat("[Forget The Colors #{0}]: Apply the color rule {1} to the sum of the first nixie ({2}) + the second nixie ({3}) + the gear number ({4}). This gives us {5}. Modulo 10, its value is {6}.", moduleId, Strings.ColorLog[(int)Ease.Modulo(colorValues[3] - nixie1 + nixie2, 10)], nixie1, nixie2, GearText.text, nixie1 + nixie2 + int.Parse(GearText.text.Last().ToString()), Ease.Modulo(lsd, 10));

            //modulo
            lsd = (int)Ease.Modulo(lsd, 10);

            Debug.LogFormat("[Forget The Colors #{0}]: <-------=-------> STAGE VALUE ~ SINE/COSINE) <-------=------->", moduleId);

            //get the sine degrees
            Debug.LogFormat("[Forget The Colors #{0}]: The nixies are {1} and {2}, and the number obtained before is {3}, combining all of them gives us {4}.", moduleId, nixie1, nixie2, lsd, string.Concat(nixie1, nixie2, lsd));
            int sin = (int)(Math.Sin(int.Parse(string.Concat(nixie1, nixie2, lsd)) * Mathf.Deg2Rad) * 100000 % 100000);

            //floating point rounding fix, ensuring that it adds/subtracts 1 depending if it's a positive or negative number
            if (Ease.Modulo(Math.Abs(sin), 1000) == 999)
                if (sin > 0)
                    sin = (sin + 1) % 100000;
                else
                    sin = (sin - 1) % 100000;

            //get stage number
            int cos = (int)(Math.Abs(Math.Cos(int.Parse(DisplayTexts[0].text) * Mathf.Deg2Rad) * 100000) % 100000);

            //floating point rounding fix
            if (Ease.Modulo(cos, 1000) == 999)
                cos = (int)Ease.Modulo(cos + 1, 100000);

            Debug.LogFormat("[Forget The Colors #{0}]: The first five decimals of sin({1}) is {2}. The absolute of the first five decimals of cos({3}) is {4}.", moduleId, string.Concat(nixie1, nixie2, lsd), sin, DisplayTexts[0].text, cos);

            //get final value for the stage
            calculatedValues[currentStage] = cos + sin;
            Debug.LogFormat("[Forget The Colors #{0}]: Stage {1}'s value is sine's {2} + cosine's {3} which is {4}.", moduleId, currentStage, sin, cos, (cos + sin).ToString("F0"));

            sineNumber[currentStage] = sin;
        }
    }
}