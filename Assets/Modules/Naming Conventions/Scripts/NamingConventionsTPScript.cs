using KeepCoding;
using System.Collections;
using System.Linq;
using UnityEngine;

public class NamingConventionsTPScript : TPScript<NamingConventionsScript>
{
    public override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        int[] numbers = command.ToCharArray().ToNumbers(min: 1, max: 6, minLength: 1);

        yield return Evaluate(numbers == null,
            SendToChatError("One of the characters is not a valid button press. Expected only numbers 1-6."),
            FlipCommand(numbers));
    }

    public override IEnumerator TwitchHandleForcedSolve()
    {
        int[] answer = Enumerable.Range(1, 6).Where(i => Module.textStates[i] != Module.Solutions[Module.DataType][i - 1]).ToArray();
        yield return FlipCommand(answer);
    }

    private IEnumerator FlipCommand(int[] btns)
    {
        for (int i = 0; i < btns.Length; i++)
        {
            Module.Buttons[btns[i]].OnInteract();
            yield return new WaitForSecondsRealtime(0.125f);
        }

        while (!Module.IsSolved)
        {
            yield return null;

            if (Module.textStates[1] == Module.Solutions[Module.DataType][0])
            {
                Module.Buttons[0].OnInteract();
                break;
            }
        }
    }
}
