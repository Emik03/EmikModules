using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using TheOctadecayotton;
using UnityEngine;

public class TheOctadecayottonScript : MonoBehaviour
{
    public InteractScript Interact;
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMModSettings ModSettings;
    public KMSelectable ModuleSelectable, SubModuleSelectable;
    public MeshRenderer SelectableRenderer;

    [Range(3, 12)]
    public byte DimensionOverride;
    public string ForceRotation, ForceStartingSphere;

    internal bool isUsingBounce, stretchToFit, colorAssist;
    internal static int ModuleIdCounter { get; private set; }
    internal static int Activated { get; set; }
    internal int moduleId, dimensionOverride, dimension, rotation, stepRequired;
    internal bool IsSolved { get; set; }
    internal bool ZenModeActive, TwitchPlaysActive;
    internal string souvenirSphere, souvenirRotations;

    private void Start()
    {
        Activated = 0;
        moduleId = ++ModuleIdCounter;
        ModSettingsJSON.Get(this, out dimension, out rotation, out colorAssist, out isUsingBounce, out stretchToFit);

        ModuleSelectable.OnInteract += Interact.Init(this, true, dimension - Info.GetSolvableModuleNames().Where(i => i == "The Octadecayotton").Count());
        SubModuleSelectable.OnInteract += Interact.OnInteract(this, false, dimension - Info.GetSolvableModuleNames().Where(i => i == "The Octadecayotton").Count());
        SubModuleSelectable.OnHighlight += () => SelectableRenderer.enabled = true;
        SubModuleSelectable.OnHighlightEnded += () => SelectableRenderer.enabled = false;
    }

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} succumb (Activate module/Enter submission) | !{0} submit <#> <#> <#>... (Submits on digit #) | !{0} settings (Shows additional settings)";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.Split();

        if (Regex.IsMatch(split[0], @"^\s*succumb\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            SubModuleSelectable.OnInteract();
        }

        else if (Regex.IsMatch(split[0], @"^\s*settings\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return @"sendtochat NOTE, SETTINGS CANNOT BE CHANGED AFTER MODULE INITIATION! | !{1} set 3-12 (Sets dimension count, in Normal/Time mode you may not start with less than 9) | !{1} spin <#> (Sets rotation count, in Normal/Time mode you may not start with less than 3, sets the amount of rotations) | !{1} stretch (Toggle: Only affects dimensions not divisible by 3, stretches the X, Y, and Z planes to fit the module, however it causes some axes to become exaggerated) | !{1} springiness (Toggle: Uses InOutBounce ease, do not use on serious TP bombs)";
        }

        else if (Regex.IsMatch(split[0], @"^\s*spin\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            split = split.Skip(1).ToArray();
            int n;

            if (Interact.Dimension != 0)
                yield return "sendtochaterror Since the module has been activated at least once, this value can no longer change.";

            else if (split.Length != 1)
                yield return "sendtochaterror " + (split.Length == 0 ? "No arguments are specified. Expected: 0-100." : "Too many arguments are specified. Expected: 0-100.");

            else if (!int.TryParse(split[0], out n))
                yield return "sendtochaterror The argument must be a number.";

            else if (n < 0 || n > 100)
                yield return "sendtochaterror The number of rotations specified is not supported.";

            else if (!ZenModeActive && n < 3 && !Application.isEditor)
                yield return "sendtochaterror You cannot change the module to be lower than 3 rotations on Normal/Time mode.";

            else
            {
                rotation = n;
                yield return "sendtochat This module now activates with " + n + (n == 1 ? " rotation." : " rotations.");
            }
        }

        else if (Regex.IsMatch(split[0], @"^\s*springiness\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (Interact.Dimension != 0)
                yield return "sendtochaterror Since the module has been activated at least once, this value can no longer change.";

            else
            {
                stretchToFit = !stretchToFit;
                yield return "sendtochat The module will" + (stretchToFit ? " " : " not ") + "stretch the X, Y, and Z planes individually.";
            }
        }

        else if (Regex.IsMatch(split[0], @"^\s*stretch\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (Interact.Dimension != 0)
                yield return "sendtochaterror Since the module has been activated at least once, this value can no longer change.";

            else
            {
                isUsingBounce = !isUsingBounce;
                Interact.isUsingBounce = isUsingBounce;
            }
        }

        else if (Regex.IsMatch(split[0], @"^\s*set\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            split = split.Skip(1).ToArray();
            int n;

            if (Interact.Dimension != 0)
                yield return "sendtochaterror Since the module has been activated at least once, this value can no longer change.";

            else if (split.Length != 1)
                yield return "sendtochaterror " + (split.Length == 0 ? "No arguments are specified. Expected: 3-12." : "Too many arguments are specified. Expected: 3-12.");

            else if(!int.TryParse(split[0], out n))
                yield return "sendtochaterror The argument must be a number.";

            else if (n < 3 || n > 12)
                yield return "sendtochaterror The number of dimensions specified is not supported.";

            else if (!ZenModeActive && n < 9 && !Application.isEditor)
                yield return "sendtochaterror You cannot change the module to be lower than 9 dimensions on Normal/Time mode.";

            else
            {
                dimensionOverride = n;
                yield return "sendtochat This module now activates with " + n + " dimensions.";
            }
        }

        else if (Regex.IsMatch(split[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            split = split.Skip(1).ToArray();
            int n;

            if (!Interact.isActive)
                yield return "sendtochaterror The module isn't active. Use the \"succumb\" command.";

            else if (!Interact.isSubmitting)
                yield return "sendtochaterror The module isn't in submission. Use the \"succumb\" command.";

            else if (split.Length == 0)
                yield return "sendtochaterror Digits are expected to be provided as well. Expected: 0 to " + (Interact.Dimension - 1) + ".";

            else if (split.Any(s => !int.TryParse(s, out n)))
                yield return "sendtochaterror At least one of the arguments are not digits. Expected: 0 to " + (Interact.Dimension - 1) + ".";

            else if (split.Any(s => int.Parse(s) >= Interact.Dimension))
                yield return "sendtochaterror At least one of the arguments exceeded the amount of dimensions. Expected: 0 to " + (Interact.Dimension - 1) + ".";

            else
            {
                int[] times = split.Select(s => int.Parse(s)).ToArray();
                for (int i = 0; i < times.Length; i++)
                {
                    while (Interact.GetLastDigitOfTimer != times[i])
                        yield return true;
                    SubModuleSelectable.OnInteract();
                }
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (!Interact.isActive)
            SubModuleSelectable.OnInteract();
        while (!Interact.isActive)
            yield return true;

        if (!Interact.isSubmitting)
            SubModuleSelectable.OnInteract();
        while (!Interact.isSubmitting || Interact.isRotating || (Interact.Dimension == 10 && Interact.GetPreciseLastDigitOfTimer > 9.75f))
            yield return true;

        int[][] answer = Interact.GetAnswer(ZenModeActive);
        for (int i = 0; i < answer.Length; i++)
        {
            while (Interact.GetLastDigitOfTimer != (Interact.Dimension > 10 ? 19 : 9) || (Interact.GetPreciseLastDigitOfTimer > 9.125f && Interact.Dimension == 10))
                yield return true;

            for (int j = 0; j < answer[i].Length; j++)
            {
                while (Interact.GetLastDigitOfTimer != answer[i][j])
                    yield return true;
                SubModuleSelectable.OnInteract();
            }
        }

        while (!IsSolved)
            yield return true;
    }
}
