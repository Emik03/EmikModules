using ReformedRoleReversalModule;
using UnityEngine;

/// <summary>
/// Reformed Role Reversal - A Keep Talking and Nobody Explodes mod by Emik. (@Emik#0001)
/// </summary>
public class ReformedRoleReversal : MonoBehaviour
{
    public ReformedRoleReversalCoroutineHandler Coroutines;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMSelectable[] Buttons;
    public KMSelectable Screen;
    public TextMesh ScreenText, SeedText;
    public Transform SeedPosition;

    internal Init Init;

    private void Awake()
    {
        SeedPosition.transform.localRotation = Quaternion.Euler(0, 90 * Random.Range(0, 3), 0);
        Module.OnActivate += (Init = new Init(Coroutines, this)).Activate;
    }
}
