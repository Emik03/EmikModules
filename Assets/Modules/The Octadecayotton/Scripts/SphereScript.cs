using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheOctadecayotton;
using UnityEngine;

public class SphereScript : MonoBehaviour 
{
	public GameObject Sphere, Polygon;
    public Light Light;
    public Renderer SphereRenderer;
    public TheOctadecayottonScript Octadecayotton;

    internal Position pos;
    internal InteractScript Interact { private get; set; }

    private bool _isUpdatingValue;

    private void Awake()
    {
        SphereRenderer.material.color = Color.white;
    }

    internal IEnumerator UpdateValue()
    {
        _isUpdatingValue = false;
        yield return new WaitForSecondsRealtime(0.05f);
        _isUpdatingValue = true;
        Light.range = 0.875f / Mathf.Pow(Octadecayotton.Interact.Dimension, 2);

        if (pos.InitialPosition.Where((n, i) => n != Octadecayotton.Interact.startingSphere[(Axis)i]).Count() == 0)
        {
            for (float i = 0; i <= 40 && _isUpdatingValue; i++)
            {
                Light.enabled = true;
                SphereRenderer.material.color = new Color(i / 40, i / 40, i / 40);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }

        else
        {
            for (float i = 40; i >= 0 && _isUpdatingValue; i--)
            {
                Light.enabled = false;
                SphereRenderer.material.color = new Color(i / 40, i / 40, i / 40);
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        _isUpdatingValue = false;
    }
}
