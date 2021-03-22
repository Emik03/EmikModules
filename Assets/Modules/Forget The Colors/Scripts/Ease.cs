using UnityEngine;

namespace ForgetTheColors
{
    /// <summary>
    /// Contains all eases that FTC uses, which includes a few not present in the 'Easing' class.
    /// </summary>
    sealed class Ease
    {
        public static double Modulo(double num, int mod)
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

            //once it reaches here, we know it's modulated and we can return it
            return num;
        }

        public static float ElasticIn(float k)
        {
            return Modulo(k, 1) == 0 ? k : -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
        }

        public static float ElasticOut(float k)
        {
            return Modulo(k, 1) == 0 ? k : Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
        }

        public static float BackIn(float k)
        {
            return k * k * ((1.70158f + 1f) * k - 1.70158f);
        }

        public static float BackOut(float k)
        {
            return (k -= 1f) * k * ((1.70158f + 1f) * k + 1.70158f) + 1f;
        }

        public static float CubicOut(float k)
        {
            return 1f + ((k -= 1f) * k * k);
        }
    }
}