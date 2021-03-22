using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ForgetTheColors
{
    /// <summary>
    /// Handles rendering of the module.
    /// </summary>
    sealed class ModuleRender
    {
        public bool Animate(ref Transform Gear, int angleIncreasePerSolve, ref int currentAngle, ref bool allowCycleStage, ref float ease, ref bool isRotatingGear, ref bool canSolve, ref bool solved, ref KMSelectable[] Selectables, ref Transform Key, ref bool isRotatingKey)
        {
            //when solved, do the cool solve animation
            if (solved)
            {
                //expansion
                if (ease <= 1)
                {
                    ease += 0.02f;

                    Selectables[2].transform.localRotation = Quaternion.Euler(0, Ease.BackOut(ease) * 420, 0);
                    Key.localScale = new Vector3(Ease.ElasticOut(ease) * 0.5f, 1, Ease.ElasticOut(ease) * 0.5f);
                }

                //retraction
                else if (ease <= 2)
                {
                    ease += 0.04f;

                    Selectables[2].transform.localPosition = new Vector3(0, (Ease.BackIn(ease - 1) * -3) - 0.91f, 0);
                    Key.localScale = new Vector3((1 - Ease.ElasticIn(ease - 1)) / 2, 1, (1 - Ease.ElasticIn(ease - 1)) / 2);
                }

                //last frame
                else
                    Key.localPosition = new Vector3(0, -0.2f, 0);
            }

            //failed key spin
            else if (isRotatingKey)
            {
                ease += 0.04f;
                Selectables[2].transform.localRotation = Quaternion.Euler(0, (Ease.ElasticOut(ease) - ease) * 69, 0);

                if (ease >= 1)
                {
                    isRotatingKey = false;
                    ease = 0;
                }
            }

            //spin to next destination, every solve will give a new angle clockwise to itself
            else if (isRotatingGear = ease <= 1)
            {
                ease += 0.025f;

                //when finished generating stages, spin counter-clockwise to the nearest neutral position
                if (canSolve)
                {
                    Gear.localRotation = Quaternion.Euler(0, currentAngle % 360 * Math.Abs(Ease.CubicOut(ease) - 1), 0);

                    if (ease > 1)
                        Gear.localRotation = Quaternion.Euler(0, 0, 0);
                }

                //when generating stages, spin clockwise randomly
                else
                {
                    Gear.localRotation = Quaternion.Euler(0, Ease.CubicOut(ease) * angleIncreasePerSolve + currentAngle, 0);

                    if (ease > 1)
                        Gear.localRotation = Quaternion.Euler(0, angleIncreasePerSolve + currentAngle, 0);
                }
            }

            else
                return true;

            return false;
        }

        public void Update(ref bool canSolve, ref TextMesh[] DisplayTexts, ref TextMesh GearText, ref Renderer[] ColoredObjects, ref Texture[] ColorTextures, ref Transform[] CylinderDisks, ref int[] colorValues, ref bool colorblind, ref int maxStage, ref int stage)
        {
            //set everything gray
            if (canSolve)
            {
                DisplayTexts[0].text = "";
                DisplayTexts[1].text = "";
                GearText.text = "0";

                for (byte i = 0; i < ColoredObjects.Length; i++)
                    ColoredObjects[i].material.mainTexture = ColorTextures[10];

                ColoredObjects[3].material.SetTextureScale("_MainTex", new Vector2(0, 0));

                //reinstalls cylinders regardless of colorblind
                for (byte i = 0; i < CylinderDisks.Length; i++)
                    CylinderDisks[i].localRotation = new Quaternion(0, -90, 0, 0);

                //set gear
                GearText.text = GearText.text.Last().ToString();
            }

            else
            {
                //sets leading 0's
                for (byte i = 0; i < DisplayTexts.Length; i++)
                    while (DisplayTexts[i].text.Length < 3 - i)
                        DisplayTexts[i].text = "0" + DisplayTexts[i].text;

                //set colors
                for (byte i = 0; i < ColoredObjects.Length; i++)
                {
                    ColoredObjects[i].material.mainTexture = ColorTextures[colorValues[0]];
                    ColoredObjects[i].material.mainTexture = ColorTextures[colorValues[i]];
                    ColoredObjects[i].material.SetTextureOffset("_MainTex", new Vector2(0.5f * Convert.ToByte(colorblind) * Convert.ToByte(maxStage != stage), -0.04f));
                }

                ColoredObjects[3].material.SetTextureScale("_MainTex", new Vector2(0, 0));

                //deletes cylinders if needed
                for (byte i = 0; i < CylinderDisks.Length; i++)
                    CylinderDisks[i].localRotation = new Quaternion(90 * Convert.ToByte(colorblind), -90, 0, 0);

                //render letter for colorblind
                if (colorblind)
                {
                    //checks for pink, since pink and purple start with the same letter
                    if (colorValues[3] != 7)
                        GearText.text = Strings.ColorLog[colorValues[3]].First() + GearText.text.Last().ToString();
                    else
                        GearText.text = 'I' + GearText.text.Last().ToString();
                }
            }
        }

        public void UpdateCycleStage(ref TextMesh[] DisplayTexts, ref List<short> largeDisplay, ref int stage, ref TextMesh[] NixieTexts, ref List<byte> nixies, ref TextMesh GearText, ref List<byte> gear, ref bool colorblind, ref int maxStage, ref List<string> gearColor, ref Renderer[] ColoredObjects, ref Texture[] ColorTextures, ref List<byte> cylinder, ref Transform[] CylinderDisks)
        {
            //render initial displays
            DisplayTexts[0].text = largeDisplay[stage].ToString();
            DisplayTexts[1].text = Ease.Modulo(stage, 100).ToString();

            //if the large display lacks 3 characters, add 0's
            for (byte i = 0; i < DisplayTexts.Length; i++)
                while (DisplayTexts[i].text.Length < 3 - i)
                    DisplayTexts[i].text = "0" + DisplayTexts[i].text;

            //set nixies
            for (byte i = 0; i < NixieTexts.Length; i++)
                NixieTexts[i].text = nixies[i + (stage * 2)].ToString();

            //set gear number and size
            GearText.text = gear[stage].ToString();

            //render letter for colorblind
            if (colorblind)
            {
                //checks for pink, since pink and purple start with the same letter
                if (gearColor[stage] != "Pink")
                    GearText.text = gearColor[stage].First() + GearText.text.Last().ToString();
                else
                    GearText.text = 'I' + GearText.text.Last().ToString();
            }

            //set colors
            for (byte i = 0; i < ColoredObjects.Length; i++)
            {
                ColoredObjects[i].material.mainTexture = ColorTextures[cylinder[i + (stage * 4)]];
                ColoredObjects[i].material.SetTextureOffset("_MainTex", new Vector2(0.5f * Convert.ToByte(colorblind) * Convert.ToByte(maxStage != stage), -0.04f));
            }
            ColoredObjects[3].material.SetTextureScale("_MainTex", new Vector2(0, 0));

            //deletes cylinders if needed
            for (byte i = 0; i < CylinderDisks.Length; i++)
                CylinderDisks[i].localRotation = new Quaternion(90 * Convert.ToByte(colorblind) * Convert.ToByte(maxStage != stage), -90, 0, 0);
        }
    }
}