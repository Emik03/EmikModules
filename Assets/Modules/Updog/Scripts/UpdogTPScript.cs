using KeepCoding;
using System.Collections;
using System.Linq;
using UnityEngine;

public class UpdogTPScript : TPScript<UpdogScript>
{
    //private bool _isWait;

    public override IEnumerator Process(string command)
    {
        string[] split = command.Split();

        const string validFirst = "ndb",
            validSecond = "ldurb";

        if (split.Any(s => s.Length != 2))
            yield return SendToChatError("All arguments must be of length 2!");

        else if (split.Any(s => !validFirst.Contains(s[0].ToLower())))
            yield return SendToChatError("All arguments must start with either \"D\" for Dog, or \"N\" for Normal.");

        else if (split.Any(s => !validSecond.Contains(s[1].ToLower())))
            yield return SendToChatError("All arguments must end with \"L\" for Left, \"D\" for Down, \"U\" for Up, or \"R\" for Right.");

        else
            yield return PushButtons(split, validFirst, validSecond);
    }

    public override IEnumerator ForceSolve()
    {
        while (!Module.IsSolved && !Module.HasStruck)
        {
            bool[] validMoves = Module.ValidMoves;

            int[] randomArray = Enumerable.Range(0, validMoves.Length)
                .ToArray()
                .Shuffle();

            for (int i = 0; i < validMoves.Length; i++)
                if (validMoves[randomArray[i]])
                {
                    Module.Arrows[randomArray[i] + Module.OrderOffset].OnInteract();
                    yield return null;
                    break;
                }

            if (Module.IsOnBone)
            {
                Module.Center[0].OnInteract();
                yield return null;
            }
        }
    }

    private IEnumerator PushButtons(string[] strs, string validFirst, string validSecond)
    {
        foreach (string str in strs)
        {
            // If the module has struck, the command should interupt to prevent further strikes.
            if (Module.HasStruck) 
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

        Module.HasStruck = false;
    }
}
