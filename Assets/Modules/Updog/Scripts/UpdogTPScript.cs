using KeepCodingAndNobodyExplodes;
using System.Collections;
using System.Linq;
using UnityEngine;

public class UpdogTPScript : TPScript<UpdogScript>
{
    private bool _isWait;

    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.Split();

        const string validFirst = "ndb",
            validSecond = "ldurb";

        if (command == "colorblind")
        {
            yield return null;
            Module.isColorblind = !Module.isColorblind;
        }

        else if (split.Any(s => s.Length != 2))
            yield return SendToChatError("All arguments must be of length 2!");

        else if (split.Any(s => !validFirst.Contains(s[0].ToLower())))
            yield return SendToChatError("All arguments must start with either \"D\" for Dog, or \"N\" for Normal.");

        else if (split.Any(s => !validSecond.Contains(s[1].ToLower())))
            yield return SendToChatError("All arguments must end with \"L\" for Left, \"D\" for Down, \"U\" for Up, or \"R\" for Right.");

        else
        {
            yield return null;
            yield return PushButtons(split, validFirst, validSecond);
        }
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        while (!Module.isStrike && Module.IsSolved)
        {
            _isWait = true;
            StartCoroutine(AutoSolver());
            while (_isWait)
                yield return null;
        }
    }

    private IEnumerator PushButtons(string[] strs, string validFirst, string validSecond)
    {
        foreach (string str in strs)
        {
            // If the module has struck, the command should interupt to prevent further strikes.
            if (Module.isStrike) 
                break;

            // Specifies dog/normal.
            int index = validFirst.IndexOf(str[0].ToLower()) * 4;
            // Specifies left/down/up/right.
            index += validSecond.IndexOf(str[1].ToLower());

            // Specifies picking up bones or pressing a button.
            if (index < Module.Arrows.Length)
                Module.Arrows[index].OnInteract();
            else
                Module.Center[0].OnInteract();

            yield return new WaitForSecondsRealtime(0.2f);
        }

        Module.isStrike = false;
    }

    private IEnumerator AutoSolver()
    {
        bool[] validMoves = Module.ValidMoves;

        int[] randomArray = Enumerable.Range(0, validMoves.Length)
            .ToArray()
            .Shuffle();

        for (int i = 0; i < validMoves.Length; i++)
            if (validMoves[randomArray[i]])
            {
                _isWait = true;
                Module.Arrows[randomArray[i] + Module.OrderOffset].OnInteract();
                yield return new WaitForSecondsRealtime(0.05f);
                break;
            }

        if (Module.IsOnBone)
        {
            _isWait = true;
            Module.Center[0].OnInteract();
            yield return new WaitForSecondsRealtime(0.05f);
        }

        _isWait = false;
    }
}
