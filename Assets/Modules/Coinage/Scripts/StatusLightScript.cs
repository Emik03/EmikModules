using UnityEngine;

public class StatusLightScript : MonoBehaviour
{
    private float _position;

    private void FixedUpdate()
    {
        _position += 1 / 32f;
        transform.localPosition = new Vector3(0, (Mathf.Sin(_position) / 100) - 0.03f, 0);
    }
}
