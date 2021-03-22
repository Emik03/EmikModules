using ForgetAnyColor;
using UnityEngine;

/// <summary>
/// Forget Any Color - A Keep Talking and Nobody Explodes Modded Module by Emik.
/// </summary>
public class FACScript : MonoBehaviour
{
    public ForgetAnyColorCoroutineScript Coroutine;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBossModule Boss;
    public KMBombInfo Info;
    public KMColorblindMode Colorblind;
    public KMRuleSeedable Rule;
    public KMSelectable[] Selectables;
    public Renderer[] ColoredObjects;
    public ForgetAnyColorTPScript TP;
    public Transform Gear, Key;
    public Transform[] CylinderDisks;
    public TextMesh DisplayText, GearText;
    public TextMesh[] NixieTexts;
    public Texture[] ColorTextures;

    internal Init init;

    private void Awake()
    {
        Module.OnActivate += (init = new Init(Coroutine, this, TP)).Start;
	}
}
