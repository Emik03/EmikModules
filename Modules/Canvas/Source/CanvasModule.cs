// SPDX-License-Identifier: MPL-2.0

/// <summary>On the Subject of The Canvas.</summary>
#pragma warning disable CA1050, MA0047, RCS1110 // ReSharper disable once CheckNamespace
public sealed class CanvasModule : ModdedModule
#pragma warning restore CA1050, MA0047, RCS1110
{
    IEnumerator Start() => CanvasApi.Get(x => LogLower(x), Panic);

    /// <inheritdoc />
    protected override void OnEnable()
    {
        base.OnEnable();
        CanvasApi.OnBoardReceived += BoardReceived;
    }

    /// <inheritdoc />
    protected override void OnDisable()
    {
        base.OnDisable();
        CanvasApi.OnBoardReceived -= BoardReceived;
    }

    void BoardReceived(Sprite sprite) => Child<SpriteRenderer>().sprite = sprite;

    void Panic()
    {
        Log("The module was unable to establish a connection to the server. Press any tile to solve the module.");
        Children.For(x => x.Add(onInteract: () => Solve("Panic solve.")));
    }
}
