using KeepCoding;
using System.Collections;
using UnityEngine;

public class LightScript : CacheableBehaviour 
{
	private static int _instantiation;

	private IEnumerator Start()
    {
        Get<Light>().enabled = true;
        // 100 corrects for the scaling within the module.
        Get<Light>().range *= transform.lossyScale.x * 100f;

        int use = _instantiation++;

        float f = 0;

        while (f <= 3.3f)
        {
            f += Time.deltaTime;

            float frequency = (Mathf.Pow(f, 4) + (Mathf.PI * use)) / 4;
            float distance = f < 2.8f ? (Easing.OutQuart(f, 2.8f, 0, 2.8f) * 200) + 20 : BackOut((3.3f - f) * 2) * 20;

            transform.localPosition = new Vector3(Mathf.Sin(frequency) * distance, distance / 2, Mathf.Cos(frequency) * distance);

            yield return null;
        }

        Destroy(gameObject);
    }

    public static float ElasticIn(float k)
    {
        return k.Modulo(1) == 0 ? k : -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
    }

    public static float ElasticOut(float k)
    {
        return k.Modulo(1) == 0 ? k : Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
    }

    public static float BackIn(float k)
    {
        return k * k * (((1.70158f + 1f) * k) - 1.70158f);
    }

    public static float BackOut(float k)
    {
        return ((k -= 1f) * k * (((1.70158f + 1f) * k) + 1.70158f)) + 1f;
    }
}
