using KeepCodingAndNobodyExplodes;
using Linq;
using UnityEngine;

public class LinqScript : ModuleScript
{
    public KMSelectable TextSelectable;
    public KMSelectable[] Buttons;
    public Renderer ModuleHighlightable, TextHighlightable;
    public Renderer[] ButtonRenderers, ButtonHighlightables;
    public TextMesh Text;

    internal LinqSelect select;

    private void Start()
    {
        select = new LinqSelect(this);

        Get<KMSelectable>().Assign(onHighlight: () => ModuleHighlightable.enabled = true, onHighlightEnded: () => ModuleHighlightable.enabled = false);
        TextSelectable.Assign(onHighlight: () => TextHighlightable.enabled = true, onHighlightEnded: () => TextHighlightable.enabled = false);
        Buttons.Assign(onHighlight: (i) => ButtonHighlightables[i].enabled = true, onHighlightEnded: (i) => ButtonHighlightables[i].enabled = false);

        TextSelectable.Assign(onInteract: select.TextPress);
        Buttons.Assign(onInteract: select.ButtonPress);
    }
}
