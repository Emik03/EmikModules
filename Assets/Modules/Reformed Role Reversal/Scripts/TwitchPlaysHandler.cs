using ReformedRoleReversalModule;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Handles incoming commands in Twitch Plays. Having this be a game object allows the mod to get this script component.
/// </summary>
public class TwitchPlaysHandler : MonoBehaviour
{
    public ReformedRoleReversal Reversal;

    private Init init;
    private Interact interact;

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} cut <#> (Range 1-9) -- !{0} manual <#> <#> (1st '#' Range 3-9 or 'help', 2nd '#' Range 1-8) Start with 'manual help 2', 'manual help 3', 'manual help 4'... and find the seed with !{0} zoom tilt left/down/right";
#pragma warning restore 414

    private void Start()
    {
        init = Reversal.Init;
        interact = init.Interact;
    }

    /// <summary>
    /// Scans the command and does actions in the module accordingly. 
    /// </summary>
    /// <param name="command">The command sent by the user.</param>
    /// <returns>TwitchPlays errors.</returns>
    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');

        // If the initial command is formatted correctly.
        if (Regex.IsMatch(parameters[0], @"^\s*cut\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            // If the command has incorrect amount of parameters.
            if (parameters.Length != 2)
                yield return parameters.Length < 2 ? "sendtochaterror Please specify the wire to cut!"
                                                   : "sendtochaterror Only 1 can be cut at a time!";

            // If the command has an invalid parameter.
            else if (parameters[1].Length != 1 || !char.IsDigit(parameters[1][0]) || parameters[1][0] == '0')
                yield return "sendtochaterror Invalid number! Only wires 1-9 can be cut.";

            // If the command is valid, cut wire accordingly.
            else
            {
                yield return null;

                Reversal.Buttons[interact.ButtonOrder.IndexOf(3)].OnInteract();

                byte num = (byte)char.GetNumericValue(parameters[1][0]);
                while (num != interact.WireSelected)
                {
                    Reversal.Buttons[interact.ButtonOrder.IndexOf(3)].OnInteract();
                    yield return new WaitForSeconds(0.2f);
                }

                Reversal.Screen.OnInteract();
            }
        }

        // If the initial command is formatted correctly.
        if (Regex.IsMatch(parameters[0], @"^\s*manual\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            // If the command has incorrect amount of parameters.
            if (parameters.Length != 3)
                yield return parameters.Length < 3 ? "sendtochaterror Not enough parameters provided!"
                                                   : "sendtochaterror Too many parameters specified!";

            // If the command has an invalid parameter.
            else if (parameters[1] != "help" && ( parameters[1].Length != 1 || !char.IsDigit(parameters[1][0]) || !(char.GetNumericValue(parameters[1][0]) >= 3 && char.GetNumericValue(parameters[1][0]) <= 9)))
                yield return "sendtochaterror Invalid first instruction!";

            // If the command has an invalid parameter.
            else if (parameters[2].Length != 1 || !char.IsDigit(parameters[2][0]) || !(char.GetNumericValue(parameters[2][0]) >= 1 && char.GetNumericValue(parameters[2][0]) <= 8))
                yield return "sendtochaterror Invalid second instruction!";

            // If the command is valid, go to the respective part of the manual accordingly.
            else
            {
                yield return null;

                int length = init.Conditions.GetLength(1), 
                    c1 = parameters[1] == "help" ? 0 : (int)char.GetNumericValue(parameters[1][0]) - 2, 
                    c2 = (int)char.GetNumericValue(parameters[2][0]) - 1;

                while (c1 != interact.Instruction / length)
                {
                    Reversal.Screen.OnInteract();
                    yield return new WaitForSeconds(0.2f);
                }

                while (c2 != interact.Instruction % length)
                {
                    Reversal.Buttons[c2 < interact.Instruction % length ? interact.ButtonOrder.IndexOf(1) : interact.ButtonOrder.IndexOf(2)].OnInteract();
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }

    /// <summary>
    /// Force the module to be solved in TwitchPlays.
    /// </summary>
    /// <returns>Nothing.</returns>
    private IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Debug.LogFormat("[Reformed Role Reversal #{0}] A forced solve has been initiated...", init.ModuleId);

        Reversal.Buttons[interact.ButtonOrder.IndexOf(3)].OnInteract();

        while (interact.CorrectAnswer != interact.WireSelected)
        {
            Reversal.Buttons[interact.ButtonOrder.IndexOf(3)].OnInteract();
            yield return new WaitForSeconds(0.2f);
        }

        Reversal.Screen.OnInteract();
    }
}
