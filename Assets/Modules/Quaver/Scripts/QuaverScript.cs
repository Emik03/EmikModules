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
    public KMSelectable ModuleSlectable;

    internal Init init;

    private bool _focused;

    private void Awake()
    {
        Module.OnActivate += (init = new Init(this)).OnActivate;
        ModuleSlectable.OnFocus += () => _focused = true;
        ModuleSlectable.OnDefocus += () => _focused = false;
    }

    private void Update()
    {
        if (!_focused)
            return;

        var keys = new KeyCode[] { KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.Space };

        for (int btn = 0; btn < keys.Length; btn++)
            if (Input.GetKeyDown(keys[btn]))
                init.Press(btn);
    }
}
