using KeepCoding;
using System.Collections;
using UnityEngine;

public class HingeScript : MonoBehaviour
{
    private static readonly Color32 _color = new Color32(20, 29, 47, 255);
    private Renderer _renderer;
    private Routine _routine;
    private Vector3 _scale;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _routine = new Routine(Animate, this);
        _scale = transform.localScale;
    }

    internal void Flash()
    {
        _routine.StartOrRestart();
    }

    private IEnumerator Animate()
    {
        float f = 0;

        while (f < 1)
        {
            f += Time.deltaTime;

            float ease = 1 - Easing.InOutExpo(f, 0, 1, 1);

            _renderer.material.color = new Color32(
                (byte)(_color.r + _color.r * ease * 4), 
                (byte)(_color.g + _color.g * ease * 4), 
                (byte)(_color.b + _color.b * ease * 4), 
                0);
            transform.localScale = _scale + _scale * ease / 2;

            yield return null;
        }

        _renderer.material.color = _color;
        transform.localScale = _scale;
    }
}
