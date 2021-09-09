using QuaverModule;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class QuaverTPScript : MonoBehaviour
{
    public QuaverScript Quaver;

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} set <speed> <difficulty> <per column> | !{0} start <#> (Number not required, adjusts scroll speed) | !{0} submit <#> <#> <#> <#>";
#pragma warning restore 414

    private bool IsSubmitValid(string[] split)
    {
        ushort j, max = Quaver.init.select.difficulty == 3 ? Quaver.init.select.perColumn ? (ushort)100 : (ushort)500 : (ushort)250;
        for (int i = 1; i < split.Length; i++)
            if (!ushort.TryParse(split[i], out j) || j >= max)
                return false;
        return true;
    }

    private int[] CommandToIndex(string[] split)
    {
        const int length = 3;

        string[][] parameters = new string[length][]
        {
            new[] { "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0" },
            new[] { "normal", "hard", "insane", "expert" },
            new[] { "off", "on" }
        };

        int[] indexes = new int[length];

        for (int i = 0; i < length; i++)
            indexes[i] = Array.IndexOf(parameters[i], split[i + 1].ToLowerInvariant());

        return indexes;
    }

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.Split();

        if (Regex.IsMatch(split[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (!Quaver.init.ready)
                yield return "sendtochaterror The module isn't in submission mode!";

            else if ((split.Length != 2 && !Quaver.init.select.perColumn) || (split.Length != 5 && Quaver.init.select.perColumn))
                yield return "sendtochaterror Incorrect amount of values!";

            else if (!IsSubmitValid(split))
                yield return "sendtochaterror At least one value is invalid!";

            else
                yield return Input(split);
        }
        else if (Regex.IsMatch(split[0], @"^\s*start\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            int scrollSpeed = 0;

            if (Quaver.init.gameplay)
                yield return "sendtochaterror This command cannot run during gameplay!";

            else if (Init.anotherQuaverReady)
                yield return "sendtochaterror You cannot start another instance of Quaver, please exit out of gameplay from the other one first!";

            else if (split.Length > 2)
                yield return "sendtochaterror Too many parameters!";

            else if (split.Length == 2 && !int.TryParse(split[1], out scrollSpeed) && (scrollSpeed == 0 || (scrollSpeed >= 10 && scrollSpeed <= 30)))
                yield return "sendtochaterror Parameter specified is invalid! Scroll speed's range is 10-30.";

            else
            {
                Quaver.Buttons[4].OnInteract();
                yield return new WaitForSecondsRealtime(1);

                while (scrollSpeed != 0 && scrollSpeed != ArrowScript.scrollSpeed)
                {
                    Quaver.Buttons[scrollSpeed < ArrowScript.scrollSpeed ? 1 : 2].OnInteract();
                    yield return new WaitForSecondsRealtime(0.05f);
                }
            }
        }
        else if (Regex.IsMatch(split[0], @"^\s*set\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            int[] parameters;

            if (Quaver.init.gameplay)
                yield return "sendtochaterror This command cannot run during gameplay.";

            else if (split.Length != 4)
                yield return "sendtochaterror " + (split.Length < 4 ? "Too few parameters!" : "Too many parameters!");

            else if ((parameters = CommandToIndex(split)).Any(i => i == -1))
                yield return "sendtochaterror At least one parameter is invalid!";

            else
            {
                while (Quaver.init.select.ui != 0)
                {
                    Quaver.Buttons[1].OnInteract();
                    yield return new WaitForSecondsRealtime(0.05f);
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    while (parameters[i] != new[] { Quaver.init.select.speed, Quaver.init.select.difficulty, System.Convert.ToByte(Quaver.init.select.perColumn) }[i])
                    {
                        Quaver.Buttons[3].OnInteract();
                        yield return new WaitForSecondsRealtime(0.05f);
                    }

                    Quaver.Buttons[1].OnInteract();
                    yield return new WaitForSecondsRealtime(0.05f);
                }
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
    Retry:
        yield return new WaitForSecondsRealtime(1);

        if (!Quaver.init.gameplay)
        {
            Quaver.init.select.speed = 10;
            Quaver.init.select.difficulty = 3;
            Quaver.init.select.perColumn = true;
            Quaver.Buttons[4].OnInteract();
        }

        while (!Quaver.init.ready)
            yield return true;

        yield return new WaitForSecondsRealtime(1);

        string[] correct = new string[5];
        Array.Copy(ArrowScript.arrowsPerColumn.Select(x => x.ToString()).ToArray(), 0, correct, 1, 4);

        yield return Input(correct);

        while (Quaver.init.gameplay)
            yield return true;

        if (!Quaver.init.solved)
            goto Retry;
    }

    private IEnumerator Input(string[] array)
    {
        int offset = 0;

        if (Quaver.init.select.perColumn)
            for (int i = 0; i < new[] { int.Parse(array[1]), int.Parse(array[2]), int.Parse(array[3]), int.Parse(array[4]) }.Min(); i++)
            {
                Quaver.Buttons[4].OnInteract();
                offset++;
                yield return new WaitForSecondsRealtime(0.05f);
            }

        for (int i = 1; i < array.Length; i++)
        {
            int value = int.Parse(array[i]);
            for (int j = 0; j < value - offset; j++)
            {
                Quaver.Buttons[i - 1].OnInteract();
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }
    }
}
