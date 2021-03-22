using ForgetAnyColor;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// TwitchPlays script for Forget Any Color. This will award points based on the number of successful inputs multiplied by the constant in 'Arrays'.
/// </summary>
public class ForgetAnyColorTPScript : MonoBehaviour
{
    public ForgetAnyColorCoroutineScript Coroutine;
    public FACScript FAC;

    private Calculate calculate;
    private Init init;
    private Render render;
    private Selectable selectable;

    private int stagesRewarded;

    private void Start()
    {
        init = FAC.init;

        calculate = init.calculate;
        render = init.render;
        selectable = init.selectable;
    }

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} key | !{0} press <...> (Example: !{0} press RLLR)";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] buttonPressed = command.Split(' ');

        // Debug command.
        if (Application.isEditor && Regex.IsMatch(buttonPressed[0], @"^\s*next\s*$", RegexOptions.IgnoreCase))
        {
            yield return null;
            init.fakeStage++;
        }

        // Colorblind mode command.
        else if (Regex.IsMatch(buttonPressed[0], @"^\s*colorblind\s*$", RegexOptions.IgnoreCase))
        {
            yield return null;
            render.Colorblind(render.colorblind = !render.colorblind);
        }

        else if (Regex.IsMatch(buttonPressed[0], @"^\s*key\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            FAC.Selectables[2].OnInteract();

            if (init.currentStage == 0)
                yield return "sendtochat Amount of modules required per stage is now " + Init.modulesPerStage + ".";
        }

        // Submit command.
        else if (Regex.IsMatch(buttonPressed[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (buttonPressed.Length < 2)
                yield return "sendtochaterror Please specify the buttons to press! (Valid: L/R)";

            else if (buttonPressed.Length > 2)
                yield return "sendtochaterror Too many parameters specified! If you are trying to input multiple buttons, do not use seperators!";

            else if (!Functions.TPCommandValidity(buttonPressed[1]))
                yield return "sendtochaterror Invalid submission! Only L's and R's are valid.";

            // Valid command.
            else
            {
                // Resets the strike, since it only is meant to break the for-loop once.
                selectable.strike = false;

                // This for loop breaks when either the user runs out of inputs, or strikes.
                for (int i = 0; i < buttonPressed[1].Length && !selectable.strike; i++)
                {
                    // Presses the left button if the string matches L/l, otherwise right.
                    FAC.Selectables["Ll".Contains(buttonPressed[1][i]) ? 0 : 1].OnInteract();
                    yield return new WaitForSecondsRealtime(0.1f);
                }

                // Awards points based on the multiplier and amount of stages complete.
                yield return "awardpoints " + Math.Floor((selectable.stagesCompleted - stagesRewarded) * Arrays.TPAwardPerStage);
                stagesRewarded = selectable.stagesCompleted;
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[Forget Any Color #{0}]: An auto-solve has been issued. This module resigned at stage {1}.", init.moduleId, init.stage + 1);

        while (!Coroutine.animating || init.currentStage / Init.modulesPerStage < init.finalStage / Init.modulesPerStage)
            yield return true;

        while (calculate.modifiedSequences.Count > 0)
        {
            FAC.Selectables[Convert.ToByte(calculate.modifiedSequences[0])].OnInteract();
            yield return new WaitForSecondsRealtime(0.1f);
        }

        FAC.Selectables[2].OnInteract();
    }
}
