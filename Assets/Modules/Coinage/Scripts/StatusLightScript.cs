using UnityEngine;

namespace Coinage
{
    public class StatusLightScript : MonoBehaviour
    {
        private float Position { get; set; }
        private const int Speed = 32;

        private void FixedUpdate()
        {
            Position += 1f / Speed;
            transform.localPosition = new Vector3(0, (Mathf.Sin(Position) / 100) - 0.03f, 0);
        }
    }
}
