using Logging;
using KeepCoding;
using UnityEngine;
using System.Linq;
using System;
using System.Collections;

public class LoggingScript : ModuleScript
{
    public TextMesh[] Texts;

    internal int pageIndex, lastPageIndex;
    internal int? look;
    internal Logs logs;

    private int _solves;

    private const float Black = 1 / 15f;

    public override void OnAwake()
    {
        logs = new Logs();

        Application.logMessageReceived += (c, s, l) => Push(c);

        Log("The logs are now being read.");

        StartCoroutine(WaitOneFrame());
    }

    public override void OnDestruct()
    {
        Application.logMessageReceived -= (c, s, l) => Push(c);

        Log("The logs are no longer being read.");
    }

    private IEnumerator WaitOneFrame()
    {
        yield return new WaitWhile(() => Modules == null);

        var solvable = Modules
            .Where(m => m.IsSolvable)
            .ToArray();

        solvable.ForEach(m =>
        {
            Action action = () => Log("Solve ({0}/{1}): {2}.", ++_solves, solvable.Length, m.Name);

            action += () => m.Solve.Remove(action);

            m.Solve.Add(action);
        });

        Modules.ForEach(m => m.Strike.Add(() => Log("Strike: {0}.", m.Name)));

        Texts.Select(t => t.GetComponentInChildren<KMSelectable>()).Take(7).ToArray().Assign(
            onInteract: InteractHandler,
            onHighlight: i =>
            {
                Texts[i].color = new Color(1, Black, Black);
                Texts[i].fontSize = 90;
                Texts[i].fontStyle = FontStyle.Bold;
            },
            onHighlightEnded: i =>
            {
                Texts[i].color = new Color(Black, Black, Black);
                Texts[i].fontSize = 100;
                Texts[i].fontStyle = FontStyle.Normal;
            });
    }

    private void InteractHandler(int i)
    {
        bool isPageContentDisabled = look.HasValue || logs.Keys.ElementAtOrDefault((pageIndex * 5) + i) == null;

        if (i >= 5 || !isPageContentDisabled)
            PlaySound(SFX.Log.Select, KMSoundOverride.SoundEffect.PageTurn);

        switch (i)
        {
            case 5: pageIndex--; break;
            case 6: pageIndex++; break;

            default:
                if (isPageContentDisabled)
                    break;

                lastPageIndex = pageIndex;
                look = (pageIndex * 5) + i;
                pageIndex = 0;
                Log("Viewing {0}.".Form(logs.Keys[look.Value]));
                break;
        }

        if (!pageIndex.IsBetween(0, GetPageCount() - 1))
        {
            if (look.HasValue)
            {
                pageIndex = lastPageIndex;
                look = null;
            }

            else
            {
                pageIndex = pageIndex.Modulo(GetPageCount());

                if (!IsSolved)
                    PlaySound(SFX.Log.Solve, KMSoundOverride.SoundEffect.Stamp);

                Solve("Solved!");
            }
        }

        Bump();
    }

    private void Push(string condition = null)
    {
        string newKey = null;

        if (condition != null)
            newKey = logs.Add(condition);

        if (newKey != null && look.HasValue && string.CompareOrdinal(newKey, logs.Keys[look.Value]) < 0)
            look++;

        if (logs.Count == 0)
            return;

        Bump();
    }

    internal void Bump()
    {
        try
        {
            Texts[7].text = "Page {0} of {1}{2}".Form(pageIndex + 1, GetPageCount(), look.HasValue ? " <{0}>".Form(logs.Keys[look.Value]) : "");
            Texts[7].fontSize = 100 - (look.HasValue ? (Math.Max(logs.Keys[look.Value].Length - 12, 0) * 3) : 0);

            for (int i = 0; i < 5; i++)
                Texts[i].text = look.HasValue ? i == 2 ? logs.Values[look.Value].Slice(pageIndex * Logs.RowLength, Logs.RowLength).Join("\n") : "" : logs.Keys.ElementAtOrDefault((pageIndex * 5) + i);
        }
        catch (NullReferenceException) { }
    }

    private int GetPageCount()
    {
        return (look.HasValue ? (logs.Values[look.Value].Count - 1) / Logs.RowLength : (logs.Count - 1) / 5) + 1;
    }
}
