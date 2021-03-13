using EmikBaseModules;
using System.Collections;
using System.Linq;
using UnityEngine;

public class NamingConventionsTPScript : TPScript
{
    public NamingConventionsScript Module;

#pragma warning disable 414
#pragma warning disable IDE0051 // Remove unused private members
    new private const string TwitchHelpMessage = @"!{0} <###...> | Flips each button on <#>, then presses the first button to submit | Example: !{0} 123";
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore 414

    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        int[] numbers = command.ToCharArray().ToNumbers();

        yield return Evaluate(numbers == null,
            SendToChatError("One of the characters is not a valid button press. Expected only numbers 2-7."),
            FlipCommand(numbers));
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        int[] answer = Enumerable.Range(2, 6).Where(i => Module.textStates[i - 1] != Module.Solutions[Module.DataType][i - 2]).ToArray();
        yield return FlipCommand(answer);
    }

    private IEnumerator FlipCommand(int[] btns)
    {
        for (int i = 0; i < btns.Length; i++)
        {
            Module.Buttons[btns[i] - 1].OnInteract();
            yield return new WaitForSecondsRealtime(0.125f);
        }

        while (!Module.IsSolved)
        {
            yield return true;
            if (Module.textStates[1] == Module.Solutions[Module.DataType][0])
            {
                Module.Buttons[0].OnInteract();
                break;
            }
        }
    }
}
