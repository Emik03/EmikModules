using EmikBaseModules;
using KModkit;
using Linq;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class LinqTPScript : TPScript
{
    public LinqScript Linq;

    private bool _isRunningTwitchCommand;

    new private const string TwitchHelpMessage = @"!{0} 6 [Presses the 6th position] | !{0} submit [Presses the text, case-insensitive] | !{0} 6 54 sUbMiT [Presses the 6th, 5th, and 4th position, then the text.]";

    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.Split();

        if (Regex.IsMatch(split[0], @"^\s*highlight\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            _isRunningTwitchCommand = true;

            StartCoroutine(TwitchHighlight());

            while (_isRunningTwitchCommand)
                yield return true;
        }

        else if (split.Any(s => !(s.ToLowerInvariant() == "submit" || s.All(c => "123456".Contains(c.ToString())))))
            yield return "sendtochaterror Invalid command!";

        else
        {
            yield return null;
            _isRunningTwitchCommand = true;

            StartCoroutine(TwitchSelect(split));

            while (_isRunningTwitchCommand)
                yield return true;
        }

    }

    private IEnumerator TwitchHighlight()
    {
        yield return null;

        for (int i = 0; i < Linq.Buttons.Length; i++)
        {
            Linq.Buttons[i].OnHighlight();
            yield return new WaitForSecondsRealtime(1);
            Linq.Buttons[i].OnHighlightEnded();
        }

        _isRunningTwitchCommand = false;
    }

    private IEnumerator TwitchSelect(string[] split)
    {
        yield return null;
        bool isSubmit = false;

        for (int i = 0; i < split.Length; i++)
        {
            if (split[i].ToLowerInvariant() == "submit")
            {
                isSubmit = true;
                continue;
            }

            for (int j = 0; j < split[i].Length; j++)
            {
                Linq.Buttons[!Linq.select.isInverted ? (int)char.GetNumericValue(split[i][j]) - 1 : new[] { 0, 2, 4, 1, 3, 5 }[(int)char.GetNumericValue(split[i][j]) - 1]].OnInteract();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        if (isSubmit)
            Linq.TextSelectable.OnInteract();

        _isRunningTwitchCommand = false;
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;

        for (int i = Linq.select.currentStage; i < LinqSelect.MaxStage; i++)
        {
            _isRunningTwitchCommand = true;

            bool[] answer = LinqValidate.Run(Linq.Get<KMBombInfo>().GetSerialNumber(), Linq.select.initialButtonStates, Linq.select.functions[i], Linq.select.parameter);
            string answerIndexes = string.Empty;

            for (int j = 0; j < answer.Length; j++)
                if (answer[j] != Linq.select.buttonStates[j])
                    answerIndexes += (j + 1).ToString();

            StartCoroutine(TwitchSelect(new string[] { "submit", answerIndexes }));
            yield return new WaitForSecondsRealtime(0.2f);

            while (_isRunningTwitchCommand)
                yield return true;
        }
    }
}