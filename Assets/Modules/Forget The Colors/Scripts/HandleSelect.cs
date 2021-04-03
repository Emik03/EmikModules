using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

namespace ForgetTheColors
{
    /// <summary>
    /// Handles selecting components.
    /// </summary>
    sealed class HandleSelect
    {
        private readonly Calculate calculate = new Calculate();
        private readonly ModuleRender moduleRender = new ModuleRender();

        public void Press(byte btn, ref KMAudio Audio, ref KMSelectable[] Selectables, ref bool solved, ref bool canSolve, ref bool allowCycleStage, ref int stage, ref int maxStage, ref TextMesh[] DisplayTexts, ref List<short> largeDisplay, ref TextMesh[] NixieTexts, ref List<byte> nixies, ref TextMesh GearText, ref List<byte> gear, ref bool colorblind, ref List<string> gearColor, ref Renderer[] ColoredObjects, ref Texture[] ColorTextures, ref List<byte> cylinder, ref Transform[] CylinderDisks, ref int[] colorValues, ref sbyte debugPointer, ref int moduleId, ref Rule[][] rules, ref KMBombInfo BombInfo, ref List<string> ruleColor, ref List<int> calculatedValues, ref List<int> sineNumber, ref float sum, ref sbyte solution, ref bool isRotatingGear, ref int currentAngle, int angleIncreasePerSolve, ref float ease, ref KMBombModule Module, ref bool isRotatingKey)
        {
            //adds interaction punch
            if (btn != 2)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Selectables[btn].transform);
                Selectables[btn].AddInteractionPunch();
            }

            //if solved, buttons and key should do nothing
            if (solved || (!canSolve && !FTCScript.isEditor))
                return;

            //strike mode
            if (allowCycleStage)
            {
                switch (btn)
                {
                    case 0: stage = (int)Ease.Modulo(stage - 1, maxStage); moduleRender.UpdateCycleStage(ref DisplayTexts, ref largeDisplay, ref stage, ref NixieTexts, ref nixies, ref GearText, ref gear, ref colorblind, ref maxStage, ref gearColor, ref ColoredObjects, ref ColorTextures, ref cylinder, ref CylinderDisks); break;
                    case 1: stage = (int)Ease.Modulo(stage + 1, maxStage); moduleRender.UpdateCycleStage(ref DisplayTexts, ref largeDisplay, ref stage, ref NixieTexts, ref nixies, ref GearText, ref gear, ref colorblind, ref maxStage, ref gearColor, ref ColoredObjects, ref ColorTextures, ref cylinder, ref CylinderDisks); break;
                    case 2: allowCycleStage = false; stage = maxStage; moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref stage); break;
                }
                return;
            }

            //NOT the key
            else if (btn != 2)
            {
                //increments respective nixie by 1
                if (!FTCScript.isEditor)
                    NixieTexts[btn].text = Ease.Modulo(int.Parse(NixieTexts[btn].text) + 1, 10).ToString();

                //complete debugging
                else
                {
#pragma warning disable CS0162 // Unreachable code detected
                    //right nixie changes value selected by one
                    if (btn == 1)
                        switch (debugPointer)
                        {
                            case 0: DisplayTexts[0].text = Ease.Modulo(int.Parse(DisplayTexts[0].text) + 100, 1000).ToString(); break;
                            case 1: DisplayTexts[0].text = Ease.Modulo(int.Parse(DisplayTexts[0].text) + 10, 1000).ToString(); break;
                            case 2: DisplayTexts[0].text = Ease.Modulo(int.Parse(DisplayTexts[0].text) + 1, 1000).ToString(); break;
                            case 3: colorValues[0] = (int)Ease.Modulo(colorValues[0] + 1, 10); break;
                            case 4: colorValues[1] = (int)Ease.Modulo(colorValues[1] + 1, 10); break;
                            case 5: colorValues[2] = (int)Ease.Modulo(colorValues[2] + 1, 10); break;
                            case 6: GearText.text = Ease.Modulo(int.Parse(GearText.text[Convert.ToByte(colorblind)].ToString()) + 1, 10).ToString(); break;
                            case 7: colorValues[3] = (int)Ease.Modulo(colorValues[3] + 1, 10); break;
                            case 8: NixieTexts[0].text = Ease.Modulo(int.Parse(NixieTexts[0].text) + 1, 10).ToString(); break;
                            case 9: NixieTexts[1].text = Ease.Modulo(int.Parse(NixieTexts[1].text) + 1, 10).ToString(); break;
                            case 10: stage++; break;

                            case 11:
                                //souvenir
                                gear[stage] = byte.Parse(GearText.text.Last().ToString());
                                gearColor[stage] = Strings.ColorLog[colorValues[3]];
                                largeDisplay[stage] = short.Parse(DisplayTexts[0].text);

                                for (byte i = 0; i < 2; i++)
                                    nixies[(stage * 2) + i] = byte.Parse(NixieTexts[i].text);
                                for (byte i = 0; i < 4; i++)
                                    cylinder[(stage * 4) + i] = (byte)colorValues[i];

                                calculate.NextStage(ref moduleId, ref stage, ref DisplayTexts, ref NixieTexts, ref GearText, ref colorblind, ref rules, ref colorValues, ref BombInfo, ref maxStage, ref ruleColor, ref calculatedValues, ref sineNumber);
                                calculate.FinalStage(ref moduleId, ref maxStage, ref calculatedValues, ref sum, ref NixieTexts, out canSolve, out solution);
                                break;
                        }

                    //left nixie changes which value is selected
                    else
                        debugPointer = (sbyte)Ease.Modulo(++debugPointer, 12);

                    DisplayTexts[1].text = Strings.DebugText[debugPointer];

                    moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref stage);
#pragma warning restore CS0162 // Unreachable code detected
                }
            }

            //key
            else
            {
                //debugging
                if (FTCScript.isEditor)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    //souvenir
                    gear[stage] = byte.Parse(GearText.text[Convert.ToByte(colorblind)].ToString());
                    gearColor[stage] = Strings.ColorLog[colorValues[3]];
                    largeDisplay[stage] = short.Parse(DisplayTexts[0].text);

                    //in case of strikes
                    for (byte i = 0; i < 2; i++)
                        nixies[(stage * 2) + i] = byte.Parse(NixieTexts[i].text);
                    for (byte i = 0; i < 4; i++)
                        cylinder[(stage * 4) + i] = (byte)colorValues[i];

                    calculate.NextStage(ref moduleId, ref stage, ref DisplayTexts, ref NixieTexts, ref GearText, ref colorblind, ref rules, ref colorValues, ref BombInfo, ref maxStage, ref ruleColor, ref calculatedValues, ref sineNumber);

                    if (!isRotatingGear)
                    {
                        currentAngle += angleIncreasePerSolve;
                        ease = 0;
                    }
#pragma warning restore CS0162 // Unreachable code detected
                }

                //if both correct
                else if (int.Parse(string.Concat(NixieTexts[0].text, NixieTexts[1].text)) == solution)
                {
                    solved = true;
                    ease = 0;

                    Audio.PlaySoundAtTransform(Sounds.Ftc.KeySuccess, Selectables[2].transform);
                    Audio.PlaySoundAtTransform(Sounds.Ftc.Solve, Module.transform);

                    Debug.LogFormat("[Forget The Colors #{0}]: {1}; module solved!", moduleId, Strings.WinPhrases[Rnd.Range(0, Strings.WinPhrases.Length)]);
                    Module.HandlePass();
                }

                //if either incorrect
                else
                {
                    allowCycleStage = true;
                    isRotatingKey = true;
                    ease = 0;

                    Audio.PlaySoundAtTransform(Sounds.Ftc.Key, Selectables[2].transform);

                    //if submitting a possible answer
                    if (int.Parse(string.Concat(NixieTexts[0].text, NixieTexts[1].text)) <= 90)
                        Debug.LogFormat("[Forget The Colors #{0}]: {1}; you submitted {2} when I expected {3}.", moduleId, Strings.FailPhrases[Rnd.Range(0, Strings.FailPhrases.Length)], NixieTexts[0].text + NixieTexts[1].text, solution);

                    //if submitting an impossible answer
                    else
                    {
                        Audio.PlaySoundAtTransform(Sounds.Fac.Strike, Selectables[2].transform);
                        Debug.LogFormat("[Forget The Colors #{0}]: Wh- why would you submit {1}? Why would you submit {1}?! Do you even know how I work?! That answer isn't even possible... You submitted {1} when I expected {2}.", moduleId, NixieTexts[0].text + NixieTexts[1].text, solution);
                    }

                    Module.HandleStrike();
                }
            }

            moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref stage);
        }
    }
}