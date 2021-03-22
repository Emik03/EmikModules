using PhosphorescenceModule;
using UnityEngine;

/// <summary>
/// On the Subject of Phosphorescence - A module created by Emik, with the model by Aero.
/// </summary>
public class PhosphorescenceScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMHighlightable[] MarkerHighlightables;
    public KMModSettings ModSettings;
    public KMSelectable Color, Number;
    public KMSelectable[] Buttons, Markers;
    public Transform ButtonGroupRenderer;
    public Renderer[] Tiles, ButtonRenderers, MarkerRenderers;
    public TextMesh[] ScreenText, ButtonText;
    public TextAsset WordList;
    public Texture TileTexture;
    public PhosphorescenceTPScript TP;
    public Transform Panel, Screen;

    internal Init init;

    private void Awake()
    {
        init = new Init(this);
    }
}
