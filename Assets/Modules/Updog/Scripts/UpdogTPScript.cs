using EmikBaseModules;
using System.Collections;
using System.Linq;
using UnityEngine;

public class UpdogTPScript : TPScript 
{
	public UpdogScript Updog;
    
    private bool _isWait;

#pragma warning disable 414
    new private const string TwitchHelpMessage = @"!{0} colorblind - !{0} <??> <??>... (Take Bone: BB - Normal: NL, ND, NU, NR - Dog: DL, DD, DU, DR)";
#pragma warning restore 414

    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.Split();

        const string validFirst = "NDB",
            validSecond = "LDURB";

        if (command == "colorblind")
        {
            yield return null;
            Updog.isColorblind = !Updog.isColorblind;
        }

        else if (split.Any(s => s.Length != 2))
            yield return "sendtochaterror All arguments must be of length 2!";

        else if (split.Any(s => !validFirst.Contains(s[0])))
            yield return "sendtochaterror All arguments must start with either \"D\" for Dog, or \"N\" for Normal.";

        else if (split.Any(s => !validSecond.Contains(s[1])))
            yield return "sendtochaterror All arguments must end with \"L\" for Left, \"D\" for Down, \"U\" for Up, or \"R\" for Right.";

        else
        {
            yield return null;
            yield return PushButtons(split, validFirst, validSecond);
        }
    }

    private IEnumerator PushButtons(string[] strs, string validFirst, string validSecond)
    {
        foreach (var str in strs)
        {
            // If the module has struck, the command should interupt to prevent further strikes.
            if (Updog.isStrike) 
                break;

            // Specifies dog/normal.
            int index = validFirst.IndexOf(str[0]) * 4;
            // Specifies left/down/up/right.
            index += validSecond.IndexOf(str[1]);

            // Specifies picking up bones or pressing a button.
            if (index < Updog.Arrows.Length)
                Updog.Arrows[index].OnInteract();
            else
                Updog.Center[0].OnInteract();

            yield return new WaitForSecondsRealtime(0.2f);
        }

        Updog.isStrike = false;
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        bool isEditor = Application.isEditor;

        while (!Updog.isStrike)
        {
            _isWait = true;
            StartCoroutine(AutoSolver());
            while (_isWait)
                yield return true;
        }
    }

    private IEnumerator AutoSolver()
    {
        bool[] validMoves = Updog.ValidMoves;

        int[] randomArray = Enumerable.Range(0, validMoves.Length)
            .ToArray()
            .Shuffle();

        for (int i = 0; i < validMoves.Length; i++)
            if (validMoves[randomArray[i]])
            {
                _isWait = true;
                Updog.Arrows[randomArray[i] + Updog.OrderOffset].OnInteract();
                yield return new WaitForSecondsRealtime(0.05f);
                break;
            }

        if (Updog.IsOnBone)
        {
            _isWait = true;
            Updog.Center[0].OnInteract();
            yield return new WaitForSecondsRealtime(0.05f);
        }

        _isWait = false;
    }
}
