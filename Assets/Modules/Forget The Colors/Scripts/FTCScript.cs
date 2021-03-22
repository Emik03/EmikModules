using ForgetTheColors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Forget The Colors - A Keep Talking and Nobody Explodes Modded Module by Emik and Cooldoom.
/// </summary>
public class FTCScript : MonoBehaviour
{
    //import assets
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBossModule Boss;
    public KMBombInfo BombInfo;
    public KMColorblindMode Colorblind;
    public KMRuleSeedable Rule;
    public KMSelectable[] Selectables;
    public Renderer[] ColoredObjects;
    public Transform Gear, Key;
    public Transform[] CylinderDisks;
    public TextMesh GearText;
    public TextMesh[] DisplayTexts, NixieTexts;
    public Texture[] ColorTextures;

    //large souvenir dump
    bool solved;
    int stage, maxStage = ForgetAnyColor.Arrays.EditorMaxStage;
    List<byte> gear = new List<byte>(0);
    List<short> largeDisplay = new List<short>(0);
    List<int> sineNumber = new List<int>(0);
    List<string> gearColor = new List<string>(0), ruleColor = new List<string>(0);

    //variables for solving
    private bool _canSolve, _allowCycleStage, _isRotatingGear, _isRotatingKey, _colorblind;
    private sbyte _solution = -1, _debugPointer;
    private const int _angleIncreasePerSolve = 2;
    private static int _moduleIdCounter = 1;
    private int _moduleId, _currentAngle;
    private int[] _colorValues = new int[4];
    private float _ease, _sum;
    private List<byte> _cylinder = new List<byte>(0), _nixies = new List<byte>(0);
    private List<int> _calculatedValues = new List<int>(0);

    private readonly Calculate calculate = new Calculate();
    private readonly Generate generate = new Generate();
    private readonly HandleSelect handleSelect = new HandleSelect();
    private readonly Init init = new Init();
    private readonly ModuleRender moduleRender = new ModuleRender();
    private static Rule[][] _rules;

    private void Start()
    {
        init.Start(ref Boss, ref _colorblind, ref Colorblind, ref Rule, ref _moduleId, ref _rules, ref maxStage, ref BombInfo, ref _cylinder, ref _nixies, ref gear, ref largeDisplay, ref _calculatedValues, ref sineNumber, ref gearColor, ref ruleColor, ref NixieTexts, ref Audio, ref Module);
        StartCoroutine(generate.NewStage(solved, Audio, Module, _colorValues, 0, maxStage, _solution, DisplayTexts, GearText, NixieTexts, _canSolve, ColoredObjects, ColorTextures, CylinderDisks, _colorblind, _moduleId, _calculatedValues, _sum, gear, gearColor, largeDisplay, _nixies, _cylinder, _rules, BombInfo, ruleColor, sineNumber));
    }

    private void Awake()
    {
        _moduleId = _moduleIdCounter++;

        //establish buttons
        for (byte i = 0; i < Selectables.Length; i++)
        {
            byte j = i;
            Selectables[j].OnInteract += delegate
            {
                handleSelect.Press(j, ref Audio, ref Selectables, ref solved, ref _canSolve, ref _allowCycleStage, ref stage, ref maxStage, ref DisplayTexts, ref largeDisplay, ref NixieTexts, ref _nixies, ref GearText, ref gear, ref _colorblind, ref gearColor, ref ColoredObjects, ref ColorTextures, ref _cylinder, ref CylinderDisks, ref _colorValues, ref _debugPointer, ref _moduleId, ref _rules, ref BombInfo, ref ruleColor, ref _calculatedValues, ref sineNumber, ref _sum, ref _solution, ref _isRotatingGear, ref _currentAngle, _angleIncreasePerSolve, ref _ease, ref Module, ref _isRotatingKey);
                return false;
            };
        }
    }

    private void FixedUpdate()
    {
        if (moduleRender.Animate(ref Gear, _angleIncreasePerSolve, ref _currentAngle, ref _allowCycleStage, ref _ease, ref _isRotatingGear, ref _canSolve, ref solved, ref Selectables, ref Key, ref _isRotatingKey) &&
            !solved && !_allowCycleStage && !_isRotatingGear && stage < BombInfo.GetSolvedModuleNames().Where(m => !Strings.Ignore.Contains(m)).Count())
        {
            //generate a stage
            StartCoroutine(generate.NewStage(solved, Audio, Module, _colorValues, ++stage, maxStage, _solution, DisplayTexts, GearText, NixieTexts, _canSolve, ColoredObjects, ColorTextures, CylinderDisks, _colorblind, _moduleId, _calculatedValues, _sum, gear, gearColor, largeDisplay, _nixies, _cylinder, _rules, BombInfo, ruleColor, sineNumber));

            //allows rotation
            _currentAngle += _angleIncreasePerSolve;
            _ease = 0;
        }

        else if (stage == maxStage && _solution == -1)
            calculate.FinalStage(ref _moduleId, ref maxStage, ref _calculatedValues, ref _sum, ref NixieTexts, out _canSolve, out _solution);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit <##> (Cycles through both nixies to match '##', then hits submit. If in strike mode, submitting will get you out of strike mode and back to submission. | Valid numbers are from 0-99) !{0} preview <#> (If the module has struck, you can make # any valid stage number, which will show you what it displayed on that stage.)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] buttonPressed = command.Split(' ');

        //colorblind
        if (Regex.IsMatch(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase))
        {
            yield return null;
            _colorblind = !_colorblind;

            if (!_allowCycleStage)
                moduleRender.Update(ref _canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref _colorValues, ref _colorblind, ref maxStage, ref stage);
            else
                moduleRender.UpdateCycleStage(ref DisplayTexts, ref largeDisplay, ref stage, ref NixieTexts, ref _nixies, ref GearText, ref gear, ref _colorblind, ref maxStage, ref gearColor, ref ColoredObjects, ref ColorTextures, ref _cylinder, ref CylinderDisks);
        }

        //submit command
        else if (Regex.IsMatch(buttonPressed[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            byte n;

            //turn the key to turn off
            if (_allowCycleStage)
                Selectables[2].OnInteract();

            //if command has no parameters
            else if (buttonPressed.Length < 2)
                yield return "sendtochaterror Please specify the value to submit! (Valid: 0-99)";

            //if command has too many parameters
            else if (buttonPressed.Length > 2)
                yield return "sendtochaterror Too many parameters! Please submit only a single 2-digit number.";

            //if command has an invalid parameter
            else if (!byte.TryParse(buttonPressed[1], out n) || n >= 100)
                yield return "sendtochaterror Invalid number! Only values 0-99 are valid.";

            //if command is valid, push button accordingly
            else
            {
                //splits values
                byte[] values = new byte[2] { (byte)(byte.Parse(buttonPressed[1]) / 10), (byte)Ease.Modulo(byte.Parse(buttonPressed[1]), 10) };

                //submit answer only if it's ready
                if (_canSolve)
                    for (byte i = 0; i < Selectables.Length - 1; i++)
                    {
                        //keep pushing until button value is met by player
                        while (int.Parse(NixieTexts[i].text) != values[i])
                        {
                            yield return new WaitForSecondsRealtime(0.05f);
                            Selectables[i].OnInteract();
                        }
                    }

                //key
                yield return new WaitForSecondsRealtime(0.1f);
                Selectables[2].OnInteract();
            }
        }

        else if (Regex.IsMatch(buttonPressed[0], @"^\s*preview\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            ushort n;

            //if not in strike mode
            if (!_allowCycleStage)
                yield return "sendtochaterror This command can only be executed when the module is in strike mode!";

            //if command has no parameters
            else if (buttonPressed.Length < 2)
                yield return "sendtochaterror Please specify the value to submit! (Valid: 0-<Max number of stages>)";

            //if command has too many parameters
            else if (buttonPressed.Length > 2)
                yield return "sendtochaterror Too many parameters! Please submit only 1 stage number.";

            //if command has an invalid parameter
            else if (!ushort.TryParse(buttonPressed[1], out n) || n >= maxStage)
                yield return "sendtochaterror Invalid number! Make sure you aren't exceeding the amount of stages!";

            else
            {
                //keep pushing until button value is met by player
                do
                {
                    yield return new WaitForSecondsRealtime(0.02f);
                    Selectables[1].OnInteract();
                } while (n != stage);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat("[Forget The Colors #{0}]: Admin has initiated an auto-solve. Thank you for attempting FTC. You gave up on stage {1}.", _moduleId, stage);

        while (!_canSolve)
            yield return true;

        yield return new WaitForSecondsRealtime(1f);

        //because playing in the editor forces debug mode, the module can't be solved normally
        if (Application.isEditor)
        {
            Module.HandlePass();
            yield break;
        }

        for (byte i = 0; i < 2; i++)
            while (_solution.ToString().ToCharArray()[i].ToString() != NixieTexts[i].text)
            {
                Selectables[i].OnInteract();
                yield return new WaitForSecondsRealtime(0.05f);
                moduleRender.Update(ref _canSolve, ref DisplayTexts, ref GearText, ref ColoredObjects, ref ColorTextures, ref CylinderDisks, ref _colorValues, ref _colorblind, ref maxStage, ref stage);
            }

        if (int.Parse(string.Concat(NixieTexts[0].text, NixieTexts[1].text)) == _solution)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            Selectables[2].OnInteract();
        }
    }
}