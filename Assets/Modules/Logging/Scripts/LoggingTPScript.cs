using System;
using System.Collections;
using System.Linq;
using KeepCoding;

public class LoggingTPScript : TPScript<LoggingScript>
{
    public override IEnumerator Process(string command)
    {
        string[] split = command.Split();

        if (IsMatch(split[0], "peek"))
        {
            yield return null;

            command = Module.logs.Keys.FirstOrDefault(s => s.ToLowerInvariant().Contains(split.Skip(1).Join().ToLowerInvariant()));

            bool[] errors = new[] { !IsZen, split.Length == 1, command == null };
            string[] reasons = new[] { "You can only run this command on Zen/Training mode!", "You must specify the name of a module!", "The module specified doesn't exist. If the module starts with \"The\", exclude that word from your query." };

            foreach (var item in errors.Select((b, n) => Evaluate(b, reasons[n])))
                yield return item;

            if (errors.Any(b => b))
                yield break;

            Module.lastPageIndex = Module.pageIndex;
            Module.look = Module.logs.Keys.Select(s => s.ToLowerInvariant()).ToArray().IndexOf(command.ToLowerInvariant());
            Module.pageIndex = 0;
            Module.Log("Viewing {0}.".Form(Module.logs.Keys[Module.look.Value]));
            Module.Bump();
        }

        if (IsMatch(split[0], "back"))
        {
            yield return null;
            Module.Texts[5].GetComponentInChildren<KMSelectable>().OnInteract();
        }

        if (IsMatch(split[0], "next"))
        {
            yield return null;
            Module.Texts[6].GetComponentInChildren<KMSelectable>().OnInteract();
        }
    }

    public override IEnumerator ForceSolve()
    {
        yield return null;

        var selectable = Module.Texts[5].GetComponentInChildren<KMSelectable>();

        while (!Module.IsSolved)
            selectable.OnInteract();
    }
}
