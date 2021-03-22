using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains all eases and basic functions that Forget Any Color uses, which includes a few not present in the 'Easing' class.
/// </summary>
namespace ForgetAnyColor
{
    static class Functions
    {
        public static float BackIn(float k)
        {
            return k * k * ((1.70158f + 1f) * k - 1.70158f);
        }

        public static float BackOut(float k)
        {
            return (k -= 1f) * k * ((1.70158f + 1f) * k + 1.70158f) + 1f;
        }

        public static float ElasticIn(float k)
        {
            return k % 1 == 0 ? k : -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
        }

        public static float ElasticOut(float k)
        {
            return k % 1 == 0 ? k : Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
        }

        public static int GetColorIndex(int i, FACScript FAC)
        {
            return Array.IndexOf(FAC.ColorTextures, FAC.ColoredObjects[i].material.mainTexture);
        }

        public static bool TPCommandValidity(string command)
        {
            return command.All(c => "LRlr".Contains(c));
        }
    }
}
