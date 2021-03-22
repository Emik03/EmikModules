using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

namespace ForgetTheColors
{
    /// <summary>
    /// Generates random variables for the module, then adds it to temporary lists.
    /// </summary>
    sealed class Generate
    {
        private readonly Calculate calculate = new Calculate();
        private readonly ModuleRender moduleRender = new ModuleRender();

        public IEnumerator NewStage(bool solved, KMAudio Audio, KMBombModule Module, int[] colorValues, int currentStage, int maxStage, sbyte solution, TextMesh[] DisplayTexts, TextMesh GearText, TextMesh[] NixieTexts, bool canSolve, Renderer[] ColoredObjects, Texture[] ColorTextures, Transform[] CylinderDisks, bool colorblind, int moduleId, List<int> calculatedValues, float sum, List<byte> gear, List<string> gearColor, List<short> largeDisplay, List<byte> nixies, List<byte> cylinder, Rule[][] rules, KMBombInfo BombInfo, List<string> ruleColor, List<int> sineNumber)
        {
            //if solved, don't generate
            if (solved)
                yield break;

            //plays sound
            if (currentStage != 0)
                Audio.PlaySoundAtTransform(Sounds.Ftc.NextStage, Module.transform);

            //if this is the submission/final stage
            if (currentStage == maxStage)
            {
                //runs 25 times
                for (byte i = 0; i < 25; i++)
                {
                    DisplayTexts[0].text = Rnd.Range(0, 991).ToString();
                    DisplayTexts[1].text = Rnd.Range(0, 100).ToString();
                    GearText.text = Rnd.Range(0, 10).ToString();

                    for (byte j = 0; j < NixieTexts.Length; j++)
                        NixieTexts[j].text = Rnd.Range(0, 10).ToString();

                    for (byte j = 0; j < colorValues.Length; j++)
                        colorValues[j] = Rnd.Range(0, 10);

                    moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref currentStage);

                    yield return new WaitForSecondsRealtime(0.07f);
                }

                //reset visuals
                NixieTexts[0].text = "0";
                NixieTexts[1].text = "0";

                for (byte i = 0; i < colorValues.Length; i++)
                    colorValues[i] = 10;

                moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref currentStage);
                yield break;
            }

            //if it's supposed to be randomising
            if (!solved && currentStage < maxStage && solution == -1)
            {
                //stage 0: runs 25 times, stage 1+: runs 5 times
                for (byte i = 0; i < 5 + ((Mathf.Clamp(currentStage, 0, 1) - 1) * -20); i++)
                {
                    DisplayTexts[0].text = Rnd.Range(0, 991).ToString();
                    DisplayTexts[1].text = Rnd.Range(0, 100).ToString();
                    GearText.text = Rnd.Range(0, 10).ToString();

                    for (byte j = 0; j < NixieTexts.Length; j++)
                        NixieTexts[j].text = Rnd.Range(0, 10).ToString();

                    for (byte j = 0; j < colorValues.Length; j++)
                        colorValues[j] = Rnd.Range(0, 10);

                    moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref currentStage);

                    yield return new WaitForSecondsRealtime(0.07f);
                }

                //set stage number to display
                DisplayTexts[1].text = currentStage.ToString();

                //souvenir
                gear[currentStage] = byte.Parse(GearText.text[Convert.ToByte(colorblind)].ToString());
                gearColor[currentStage] = Strings.ColorLog[colorValues[3]];
                largeDisplay[currentStage] = short.Parse(DisplayTexts[0].text);

                for (byte i = 0; i < 2; i++)
                    nixies[(currentStage * 2) + i] = byte.Parse(NixieTexts[i].text);
                for (byte i = 0; i < 4; i++)
                    cylinder[(currentStage * 4) + i] = (byte)colorValues[i];

                moduleRender.Update(ref canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref colorValues, ref colorblind, ref maxStage, ref currentStage);

                //if it's not last stage
                if (currentStage < maxStage && solution == -1)
                    calculate.NextStage(ref moduleId, ref currentStage, ref DisplayTexts, ref NixieTexts, ref GearText, ref colorblind, ref rules, ref colorValues, ref BombInfo, ref maxStage, ref ruleColor, ref calculatedValues, ref sineNumber);
            }
        }
    }
}