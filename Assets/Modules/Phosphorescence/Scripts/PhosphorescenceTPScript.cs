using KeepCoding;
using PhosphorescenceModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// TwitchPlays support for Phosphorescence. Contains an autosolver.
/// </summary>
public class PhosphorescenceTPScript : MonoBehaviour
{
    public PhosphorescenceScript Pho;

    private Init _init;
    private Render _render;
    private Select _select;

    private bool _isSubmitRunning;

    /// <summary>
    /// Converts user input to the corresponding index of the buttons array, which is in reading order.
    /// </summary>
    private static readonly Dictionary<string, int> _posToIndex = new Dictionary<string, int>
    {
        { "tl", 0 },
        { "tm", 1 },
        { "tr", 2 },
        { "ml", 3 },
        { "mm", 4 },
        { "mr", 5 },
        { "bl", 6 },
        { "br", 7 }
    };

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} pressdisplay | !{0} next | !{0} nextsequence | !{0} submit <TL/TM/TR/ML/MM/MR/BL/BR>... (refers to position, tl = top left)";
#pragma warning restore 414

    internal void Activate(Init init)
    {
        _init = init;
        _render = init.render;
        _select = init.select;
    }

    /// <summary>
    /// Parses the user command and interacts with the module accordingly.
    /// </summary>
    /// <param name="command">The user's command, trimming off the initial "!{0}" part.</param>
	private IEnumerator ProcessTwitchCommand(string command)
    {
        if (_init == null)
            yield break;

        string[] split = command.Split();

        // Wait until the animations aren't playing.
        while (_init.isAnimated)
            yield return true;

        // Display command: no parameters, a command so simple it doesn't need its own method.
        if (Regex.IsMatch(split[0], @"^\s*pressdisplay\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return Pho.Number.OnInteract();
        }

        // Next command: no parameters, needs to be in an active non-submission state.
        else if (Regex.IsMatch(split[0], @"^\s*next\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (!_init.isCountingDown)
                yield return "sendtochaterror The module isn't active right now. This command is unavailable until the display is pressed.";

            else if (_init.isInSubmission)
                yield return "sendtochaterror The module is currently in submission. This command is unavailable until the next activation.";

            else
                yield return NextCommand();
        }

        // Next command: no parameters, needs to be in an active non-submission state. An extension of the next command.
        else if (Regex.IsMatch(split[0], @"^\s*nextsequence\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (!_init.isCountingDown)
                yield return "sendtochaterror The module isn't active right now. This command is unavailable until the display is pressed.";

            else if (_init.isInSubmission)
                yield return "sendtochaterror The module is currently in submission. This command is unavailable until the next activation.";

            else
                yield return NextSequenceCommand();
        }

        // Submit command: 1 parameter, needs to be in an active submission state. Additional parsing is needed to make sure the command is formatted correctly.
        else if (Regex.IsMatch(split[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            int i;

            if (!_init.isCountingDown)
                yield return "sendtochaterror The module isn't active right now. This command is unavailable until the display is pressed.";

            else if (split.Length < 2)
                yield return "sendtochaterror There is no sequence provided. Example submission: \"submit tl TR\" submits the top left, then the top right button.";

            else if (split.Any(s => s.Length == 0))
                yield return "sendtochaterror One of the parameters are empty. Make sure that your whitespace is correct. Example submission: \"submit tl TR\" submits the top left, then the top right button.";

            else if (split.Skip(1).Any(s => !_posToIndex.TryGetValue(s.ToLower(), out i)))
                yield return "sendtochaterror One of the button presses provided are invalid. The only valid parameters are TL, TM, TR, ML, MM, MR, BL, and BR";

            else
            {
                StartCoroutine(SubmitCommand(PositionToLetters(split.Skip(1).ToArray())));
                while (_isSubmitRunning)
                    yield return true;
            }
        }
    }

    private IEnumerator NextCommand()
    {
        // A release is done prior to a press instead of the other way around to keep the screen lit after the command.
        ColorOnRelease();
        yield return new WaitForSecondsRealtime(0.2f);
        yield return Pho.Color.OnInteract();
    }

    private IEnumerator NextSequenceCommand()
    {
        string temp = _render.letters;

        // Mashes the screen until a new sequence is made.
        while (temp == _render.letters)
        {
            ColorOnRelease();
            yield return new WaitForSecondsRealtime(0.05f);
            yield return Pho.Color.OnInteract();
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    private IEnumerator SubmitCommand(string submit)
    {
        _isSubmitRunning = true;

        while (_init.isAnimated)
            yield return true;

        // If inactive, active it.
        if (!_init.isCountingDown)
        {
            yield return Pho.Number.OnInteract();
            while (_init.isAnimated)
                yield return true;
        }

        // If not in submission, active it.
        if (!_init.isInSubmission)
        {
            yield return Pho.Number.OnInteract();
            while (_init.isAnimated)
                yield return true;
        }

        // Reset submission, just in case it had any button presses.
        while (_init.submission != string.Empty)
        {
            Pho.Buttons[Rnd.Range(0, Pho.Buttons.Length)].OnInteract();
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // For each character in the user's submission.
        foreach (char s in submit)
        {
            ButtonType button;

            // Converts the first character to lowercase, making it case-insensitive.
            Function.charToButton.TryGetValue(s, out button);

            // Gets the index of the button to press.
            int buttonIndex = Array.IndexOf(_select.buttons, button);

            // Failsafe, in case it isn't found.
            if (buttonIndex == -1)
            {
                yield return "sendtochaterror Button \"" + s + "\" was unable to be found.";
                yield break;
            }

            // Presses the corresponding button.
            yield return Pho.Buttons[buttonIndex].OnInteract();
            yield return new WaitForSecondsRealtime(0.2f);
        }

        while (_init.isAnimated)
            yield return true;
        yield return Pho.Number.OnInteract();
        _isSubmitRunning = false;
    }

    private string PositionToLetters(string[] command)
    {
        string output = string.Empty;
        int[] indexes = new int[command.Length];

        for (int i = 0; i < command.Length; i++)
            indexes[i] = _posToIndex[command[i].ToLower()];

        for (int i = 0; i < indexes.Length; i++)
            output += _select.buttons[indexes[i]].ToString()[0].ToLower();

        return output;
    }

    private void ColorOnRelease()
    {
        if (ModuleScript.IsVR)
            Pho.Color.OnInteractEnded();
        else
            Pho.Color.OnDefocus();
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (_init == null)
            yield break;

        // If inactive, active it.
        if (!_init.isCountingDown)
        {
            yield return Pho.Number.OnInteract();
            while (_init.isAnimated || !_init.isCountingDown)
                yield return true;
        }

        // If not in submission, active it.
        if (!_init.isInSubmission)
        {
            yield return Pho.Number.OnInteract();
            while (_init.isAnimated || !_init.isInSubmission)
                yield return true;
        }

        StartCoroutine(SubmitCommand(_select.buttons.GetAllAnswers(_init.solution, _init.index).PickRandom().ToLower()));

        while (_isSubmitRunning || !_init.isSolved)
            yield return true;
    }
}
