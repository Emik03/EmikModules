using KeepCodingAndNobodyExplodes;
using KModkit;
using Linq;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class LinqTPScript : TPScript<LinqScript>
{
    private bool _isRunningTwitchCommand;

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

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;

        for (int i = Module.select.currentStage; i < LinqSelect.MaxStage; i++)
        {
            _isRunningTwitchCommand = true;

            bool[] answer = LinqValidate.Run(Module.Get<KMBombInfo>().GetSerialNumber(), Module.select.initialButtonStates, Module.select.functions[i], Module.select.parameter);
            string answerIndexes = string.Empty;

            for (int j = 0; j < answer.Length; j++)
                if (answer[j] != Module.select.buttonStates[j])
                    answerIndexes += (j + 1).ToString();

            StartCoroutine(TwitchSelect(new string[] { "submit", answerIndexes }));
            yield return new WaitForSecondsRealtime(0.2f);

            while (_isRunningTwitchCommand)
                yield return true;
        }
    }

    private IEnumerator TwitchHighlight()
    {
        yield return null;

        for (int i = 0; i < Module.Buttons.Length; i++)
        {
            Module.Buttons[i].OnHighlight();
            yield return new WaitForSecondsRealtime(1);
            Module.Buttons[i].OnHighlightEnded();
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
                Module.Buttons[!Module.select.isInverted ? (int)char.GetNumericValue(split[i][j]) - 1 : new[] { 0, 2, 4, 1, 3, 5 }[(int)char.GetNumericValue(split[i][j]) - 1]].OnInteract();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        if (isSubmit)
            Module.TextSelectable.OnInteract();

        _isRunningTwitchCommand = false;
    }
}