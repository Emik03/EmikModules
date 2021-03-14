using System.Collections;
using UnityEngine;

public class StatusLightScript : MonoBehaviour
{
    private float _position;

    private void FixedUpdate()
    {
        _position += 1 / 32f;
        transform.localPosition = new Vector3(0, (Mathf.Sin(_position) / 100) - 0.03f, 0);
    }

    internal IEnumerator Solve()
    {
        enabled = false;

        var pos = transform.localPosition;

        for (float f = 0; f <= 1; f += 1 / 512f)
        {
            float ease = Easing.OutQuint(f, 0, 1, 1);
            float y = (pos.y * (1 - ease)) + (0.01f * ease);

            transform.localPosition = new Vector3(0, y, 0);

            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
