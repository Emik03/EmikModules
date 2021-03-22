using QuaverModule;
using UnityEngine;

public class QuaverScript : MonoBehaviour
{
    public GameObject Arrow, ArrowBase;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMSelectable[] Buttons;
    public RenderScript Render;
    public Renderer DifficultyRenderer, PerColumnRenderer;
    public TextMesh ReceptorTotalText;
    public TextMesh[] ReceptorTexts;

    internal Init init;

    private void Awake()
    {
        Module.OnActivate += (init = new Init(this)).OnActivate;
    }
}
