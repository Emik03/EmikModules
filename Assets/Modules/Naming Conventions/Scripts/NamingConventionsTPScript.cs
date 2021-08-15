using KeepCoding;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class NamingConventionsTPScript : TPScript<NamingConventionsScript>
{
    public override IEnumerator Process(string command)
    {
        yield return null;

        string[] split = command.Split();

        if (split.Length > 0 && IsMatch(split[0], "submit"))
        {
            split = split.Skip(1).ToArray();

            yield return split.Length == 0
                ? SendToChatError("You must specify odd or even!")
                : IsMatch(split[0], "(even|odd)")
                ? (object)FlipCommand(new int[0], () => Module.TimeLeft % 2 == (IsMatch(split[0], "even") ? 0 : 1))
                : SendToChatError("You must specify either odd or even!");

            yield break;
        }

        int[] numbers = command.ToCharArray().ToNumbers(min: 1, max: 6, minLength: 1);

        yield return Evaluate(numbers == null,
            SendToChatError("One of the characters is not a valid button press. Expected only numbers 1-6."),
            FlipCommand(numbers, null));
    }

    public override IEnumerator ForceSolve()
    {
        int[] answer = Enumerable
            .Range(1, 6)
            .Where(i => Module.textStates[i] != Module.Solutions[Module.DataType][i - 1])
            .ToArray();

        yield return FlipCommand(answer, () => Module.textStates[1] == Module.Solutions[Module.DataType][0]);
    }

    private IEnumerator FlipCommand(int[] btns, Func<bool> predicate)
    {
        for (int i = 0; i < btns.Length; i++)
        {
            Module.Buttons[btns[i]].OnInteract();
            yield return new WaitForSecondsRealtime(0.125f);
        }

        if (predicate == null)
            yield break;

        while (!Module.IsSolved)
        {
            yield return null;

            if (predicate())
            {
                Module.Buttons[0].OnInteract();
                break;
            }
        }
    }
}
