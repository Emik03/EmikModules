using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KModkit;

public class LyingIndicators : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public MeshRenderer Component;
    public KMSelectable[] Buttons;
    public TextAsset EmikModuleList;

    private bool _isSolved = false, _hasStrike = false, _frkLying;
    private bool[] _buttonStates;
    static int _moduleIdCounter = 1;
    int _moduleId;
    private static string[] _emikModules;
    private readonly static Dictionary<char, string> _serToInd = new Dictionary<char, string>(11) { { '0', "BOB" }, { '1', "CAR" }, { '2', "CLR" }, { '3', "FRK" }, { '4', "FRQ" }, { '5', "IND" }, { '6', "MSA" }, { '7', "NSA" }, { '8', "SIG" }, { '9', "SND" }, { 'A', "TRN" } };

    private void Awake()
    {
        _moduleId = _moduleIdCounter++;

        _emikModules = EmikModuleList.text.Split('\n');

        for (byte i = 0; i < Buttons.Length; i++)
        {
            byte j = i;
            Buttons[j].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);

            if (!_isSolved)
                Component.GetComponent<Renderer>().material.color = new Color32(128, 128, 128, 255);

            Buttons[i].OnHighlight += delegate ()
            {
                if (!_isSolved)
                    Component.GetComponent<Renderer>().material.color = new Color32(160, 160, 160, 255);

                if (Buttons[j].GetComponent<Renderer>().material.color.b == 1 && !_isSolved)
                    Buttons[j].GetComponent<Renderer>().material.color = new Color32(153, 255, 255, 255);
            };

            Buttons[i].OnHighlightEnded += delegate ()
            {
                if (!_isSolved)
                    Component.GetComponent<Renderer>().material.color = new Color32(128, 128, 128, 255);

                if (Buttons[j].GetComponent<Renderer>().material.color.b == 1 && !_isSolved)
                    Buttons[j].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
            };

            Buttons[i].OnInteract += delegate ()
            {
                StartCoroutine(HandlePress(j));
                return false;
            };
        }
    }

    private void Start()
    {
        string str;
        _buttonStates = new bool[36];
        Dictionary<string, byte> lyingInd = new Dictionary<string, byte>();

        if (Info.GetIndicators().Count() >= 2 && Info.GetIndicators().Count() <= 5)
            foreach (string ind in Info.GetIndicators())
                lyingInd.Add(ind, 0);

        else
        {
            foreach (char ser in Info.GetSerialNumber())
                if (_serToInd.TryGetValue(ser, out str))
                    if (!lyingInd.ContainsKey(str))
                        lyingInd.Add(str, 0);

            if (Info.GetIndicators().Count() == 1)
                foreach (string ind in Info.GetIndicators())
                    if (!lyingInd.ContainsKey(ind))
                        lyingInd.Add(ind, 0);
        }


        for (int i = 0; i < _serToInd.Count; i++)
        {
            if (i != _serToInd.Count - 1)
                _serToInd.TryGetValue(System.Convert.ToChar(i + 48), out str);

            else
                _serToInd.TryGetValue('A', out str);

            for (int j = 0; j < lyingInd.Count(); j++)
            {
                if (!lyingInd.ContainsKey(str))
                    break;

                Lies(str, lyingInd);
            }
        }

        foreach (KeyValuePair<string, byte> ind in lyingInd)
            AssignButtons(ind.Key, ind.Value > 0);

        Debug.LogFormat("[Lying Indicators #{0}]: To solve this module, press all buttons that display 'F'.", _moduleId);

        for (int i = 0; i < 6; i++)
            Debug.LogFormat("[Lying Indicators #{0}]: {1} {2} {3} {4} {5} {6}", _moduleId, _buttonStates[6 * i].ToString().First(), _buttonStates[6 * i + 1].ToString().First(), _buttonStates[6 * i + 2].ToString().First(), _buttonStates[6 * i + 3].ToString().First(), _buttonStates[6 * i + 4].ToString().First(), _buttonStates[6 * i + 5].ToString().First());
    }

    private void AssignButtons(string index, bool isLying)
    {
        _buttonStates[5] = true;

        Dictionary<bool, string> log = new Dictionary<bool, string>(2) { { false, "truthful" }, { true, "lying" } };
        Debug.LogFormat("[Lying Indicators #{0}]: Indicator {1} is {2}.", _moduleId, index, log[isLying]);
        for (int i = 0; i < _buttonStates.Length; i++)
        {
            if (_buttonStates[i])
                continue;

            switch (index)
            {
                case "BOB":
                    _buttonStates[i] = i % 6 < 3 ^ isLying;
                    break;

                case "CAR":
                    _buttonStates[i] = i >= 18 ^ isLying;
                    break;

                case "CLR":
                    if (i <= 17)
                        _buttonStates[i] = i % 6 < 3 ^ isLying;
                    else
                        _buttonStates[i] = i % 6 >= 3 ^ isLying;
                    break;

                case "FRK":
                    _buttonStates[i] = (i % 6 < 2 || i % 6 >= 4) ^ isLying;
                    break;

                case "FRQ":
                    _buttonStates[i] = (i < 12 || i >= 24) ^ isLying;
                    break;

                case "IND":
                    if (i < 12 || i >= 24)
                        _buttonStates[i] = (i % 6 < 2 || i % 6 >= 4) ^ isLying;
                    else
                        _buttonStates[i] = (i % 6 == 2 || i % 6 == 3) ^ isLying;
                    break;

                case "MSA":
                    if (i < 18)
                        _buttonStates[i] = (i % 6 < 2 || i % 6 >= 4) ^ isLying;
                    else
                        _buttonStates[i] = (i % 6 == 2 || i % 6 == 3) ^ isLying;
                    break;

                case "NSA":
                    if (i < 12 || i >= 24)
                        _buttonStates[i] = (i % 6 < 3) ^ isLying;
                    else
                        _buttonStates[i] = (i % 6 >= 3) ^ isLying;
                    break;

                case "SIG":
                    _buttonStates[i] = (i % 2 == 0) ^ isLying;
                    break;

                case "SND":
                    _buttonStates[i] = (i / 6 % 2 == 1) ^ isLying;
                    break;

                case "TRN":
                    _buttonStates[i] = (i % 2 == i / 6 % 2) ^ isLying;
                    break;
            }
        }
    }

    private void Lies(string str, Dictionary<string, byte> lyingInd)
    {
        switch (str)
        {
            case "BOB":
                if (Info.GetBatteryCount() == 4 || Info.GetBatteryHolderCount() == 2)
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && Info.IsIndicatorOn(Indicator.BOB))
                    for (int k = 1; k <= 9; k++)
                    {
                        string temp;
                        if (_serToInd.TryGetValue(System.Convert.ToChar(k + 48), out temp))
                            if (lyingInd.ContainsKey(temp))
                                lyingInd[temp]++;

                        _frkLying = true;
                    }
                break;

            case "CAR":
                if (lyingInd.ContainsKey("BOB"))
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && Info.IsIndicatorOff(Indicator.CAR))
                {
                    if (lyingInd.ContainsKey("CLR"))
                        lyingInd["CLR"]++;

                    if (lyingInd.ContainsKey("FRK"))
                        lyingInd["FRK"]++;

                    _frkLying = true;

                    if (lyingInd.ContainsKey("FRQ"))
                        lyingInd["FRQ"]++;

                    if (lyingInd.ContainsKey("MSA"))
                        lyingInd["MSA"]++;

                    if (lyingInd.ContainsKey("NSA"))
                        lyingInd["NSA"]++;

                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                }
                break;

            case "CLR":
                if (Info.GetModuleNames().Count() <= 11)
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && (Info.IsPortPresent(Port.PS2) || Info.IsPortPresent(Port.RJ45)))
                {
                    if (lyingInd.ContainsKey("FRK"))
                        lyingInd["FRK"]++;

                    _frkLying = true;

                    if (lyingInd.ContainsKey("FRQ"))
                        lyingInd["FRQ"]++;

                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                }
                break;

            case "FRK":
                if (Info.GetBatteryCount() < 3)
                {
                    lyingInd[str]++;
                    _frkLying = true;
                }

                if (lyingInd[str] == 0 && Info.IsIndicatorOn(Indicator.FRK))
                {
                    if (lyingInd.ContainsKey("FRQ"))
                        lyingInd["FRQ"]++;

                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                }
                break;

            case "FRQ":
                if (lyingInd.ContainsKey("CAR") || lyingInd.ContainsKey("CLR") || lyingInd.ContainsKey("FRK") || lyingInd.ContainsKey("TRN"))
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && !Info.IsPortPresent(Port.Serial))
                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                break;

            case "IND":
                if (Info.IsPortPresent(Port.StereoRCA) ^ Info.IsPortPresent(Port.DVI))
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && Info.CountUniquePorts() >= 3)
                {
                    if (lyingInd.ContainsKey("NSA"))
                        lyingInd["NSA"]++;

                    if (lyingInd.ContainsKey("SIG"))
                        lyingInd["SIG"]++;

                    if (lyingInd.ContainsKey("SND"))
                        lyingInd["SND"]++;

                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                }
                break;

            case "MSA":
                if (_frkLying || Info.GetBatteryCount() < 3)
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && Info.GetSerialNumberNumbers().First() % 2 == 0)
                {
                    if (lyingInd.ContainsKey("NSA"))
                        lyingInd["NSA"]++;

                    if (lyingInd.ContainsKey("SIG"))
                        lyingInd["SIG"]++;
                }
                break;

            case "NSA":
                for (int k = 0; k < _emikModules.Length; k++)
                    if (Info.GetSolvableModuleNames().Contains(_emikModules[k]))
                    {
                        lyingInd[str]++;
                        break;
                    }

                if (lyingInd[str] == 0 && (Info.GetSerialNumberLetters().Contains('S') ||
                    Info.GetSerialNumberLetters().Contains('P') || Info.GetSerialNumberLetters().Contains('A') ||
                    Info.GetSerialNumberLetters().Contains('C') || Info.GetSerialNumberLetters().Contains('E')))
                {
                    if (lyingInd.ContainsKey("SIG"))
                        lyingInd["SIG"]++;

                    if (lyingInd.ContainsKey("SND"))
                        lyingInd["SND"]++;

                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                }
                break;

            case "SIG":
                if (lyingInd.ContainsKey("MSA"))
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && (Info.GetIndicators().Count() < 2 || Info.GetIndicators().Count() > 6))
                {
                    if (lyingInd.ContainsKey("SND"))
                        lyingInd["SND"]++;
                }
                break;

            case "SND":
                if (Info.IsDuplicatePortPresent())
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && Info.GetModuleNames().Count() != Info.GetModuleNames().Distinct().Count())
                    if (lyingInd.ContainsKey("TRN"))
                        lyingInd["TRN"]++;
                break;

            case "TRN":
                if (lyingInd.Count == 2)
                    lyingInd[str]++;

                if (lyingInd[str] == 0 && (Info.GetPortPlateCount() >= 3 || Info.IsPortPresent(Port.Parallel)))
                    for (int k = 1; k < lyingInd.Count - 1; k++)
                    {
                        string temp;
                        if (_serToInd.TryGetValue(System.Convert.ToChar(k + 48), out temp))
                            if (lyingInd.ContainsKey(temp) && lyingInd[temp] != 0)
                                lyingInd[temp]--;

                        string[] indArray = new string[11] { "BOB", "CAR", "CLR", "FRK", "FRQ", "IND", "MSA", "NSA", "SIG", "SND", "TRN" };
                        for (int i = 0; i < indArray.Length; i++)
                            if (lyingInd.ContainsKey(indArray[i]))
                                lyingInd[indArray[i]] = 0;
                    }
                break;
        }
    }

    private IEnumerator Solve()
    {
        float s = 0;
        _isSolved = true;
        Module.HandlePass();
        Audio.PlaySoundAtTransform(Sounds.Lid.Solve, transform);
        Audio.PlaySoundAtTransform(Sounds.Lid.SolveBass, transform);
        Component.GetComponent<Renderer>().material.color = new Color32(160, 160, 160, 255);

        while (s <= 1)
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].transform.localPosition = Vector3.Lerp(Buttons[i].transform.localPosition, new Vector3(Buttons[i].transform.localPosition.x, 0.015f, Buttons[i].transform.localPosition.z), BackOut(s));
                s += 0.001f;
            }

            yield return new WaitForSeconds(0.01f);
        }

        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Buttons[i * 12 + j].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                    yield return new WaitForSeconds(0.05f);
                }

                for (int j = 5; j >= 0; j--)
                {
                    Buttons[i * 12 + j + 6].GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
                    yield return new WaitForSeconds(0.05f);
                }
            }

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 5; j >= 0; j--)
                {
                    if (Buttons[i * 12 + j].GetComponent<Renderer>().material.color.b == 1)
                        Buttons[i * 12 + j].GetComponent<Renderer>().material.color = new Color32(153, 255, 255, 255);

                    yield return new WaitForSeconds(0.05f);
                }

                for (int j = 0; j < 6; j++)
                {
                    if (Buttons[i * 12 + j + 6].GetComponent<Renderer>().material.color.b == 1)
                        Buttons[i * 12 + j + 6].GetComponent<Renderer>().material.color = new Color32(153, 255, 255, 255);

                    yield return new WaitForSeconds(0.05f);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator HandlePress(byte btn)
    {
        if (_isSolved || Buttons[btn].GetComponent<Renderer>().material.color.b == 0.6f)
            yield break;

        for (int i = 0; i < _buttonStates.Length; i++)
        {
            if (!_buttonStates[i])
                break;

            if (i == _buttonStates.Length - 1)
            {
                StartCoroutine(Solve());
                yield break;
            }
        }

        Debug.LogFormat("[Lying Indicators #{0}]: Pressing button {1}:{2}", _moduleId, (btn % 6) + 1, (btn / 6) + 1);

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[btn].transform);
        Buttons[btn].AddInteractionPunch();

        Audio.PlaySoundAtTransform(Sounds.Lid.Press, Buttons[btn].transform);

        if (_buttonStates[btn])
        {
            Debug.LogFormat("[Lying Indicators #{0}]: That button was invalid, strike!", _moduleId);
            Buttons[btn].GetComponent<Renderer>().material.color = new Color32(255, 153, 153, 255);
            Module.HandleStrike();
            _hasStrike = true;

            for (float i = 0; i < 1; i += 0.05f)
            {
                Buttons[btn].transform.localPosition = Vector3.Lerp(Buttons[btn].transform.localPosition, new Vector3(Buttons[btn].transform.localPosition.x, 0.005f, Buttons[btn].transform.localPosition.z), BackOut(i));
                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            _buttonStates[btn] = true;
            Buttons[btn].GetComponent<Renderer>().material.color = new Color32(153, 255, 153, 255);

            if (_buttonStates.Where(c => !c).Count() != 1)
                Debug.LogFormat("[Lying Indicators #{0}]: That button was valid, there are {1} buttons left to press.", _moduleId, _buttonStates.Where(c => !c).Count());

            else
                Debug.LogFormat("[Lying Indicators #{0}]: That button was valid, there is {1} button left to press.", _moduleId, _buttonStates.Where(c => !c).Count());

            for (int i = 0; i < _buttonStates.Length; i++)
            {
                if (!_buttonStates[i])
                {
                    for (float j = 0; j < 1; j += 0.05f)
                    {
                        if (_isSolved)
                            yield break;

                        Buttons[btn].transform.localPosition = Vector3.Lerp(Buttons[btn].transform.localPosition, new Vector3(Buttons[btn].transform.localPosition.x, 0.005f, Buttons[btn].transform.localPosition.z), BackOut(j));
                        yield return new WaitForSeconds(0.01f);
                    }
                    yield break;
                }
            }
            StartCoroutine(Solve());
        }
    }

    private float BackOut(float k)
    {
        float s = 1.70158f;
        return (k -= 1f) * k * ((s + 1f) * k + s) + 1f;
    }

    private bool IsValid(string par)
    {
        string[] validLetters = new string[6] { "A", "B", "C", "D", "E", "F" };
        char[] c = par.ToCharArray();

        if (c.Length != 2)
        {
            Debug.Log(c.Length);
            return false;
        }

        byte b;
        if (!validLetters.Contains(c[0].ToString().ToUpper()) || !byte.TryParse(c[1].ToString(), out b))
        {
            return false;
        }

        if (b == 0 || b > 6)
        {
            return false;
        }

        return true;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <X><#> (Presses specified button, 'X' is horizontal, where A is far left and F is far right, '#' is vertical, where 1 is far up and 6 is far down. Example: !{0} press A1 F6 - presses top-left and bottom-right button.)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] buttonPressed = command.Split(' ');
        bool push = true;
        _hasStrike = false;

        //if command is formatted correctly
        if (Regex.IsMatch(buttonPressed[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            for (int i = 1; i < buttonPressed.Length; i++)
            {
                //if command has an invalid parameter
                if (!IsValid(buttonPressed[i]))
                {
                    push = false;
                    yield return "sendtochaterror Invalid format! Only letters A-F are allowed for the first character, and 1-6 for the second!";
                }

                //if button doesn't exist
                else if (buttonPressed[i] == "F1")
                {
                    push = false;
                    yield return "sendtochaterror Invalid submission! The top-right button doesn't exist.";
                }
            }

            //if command is valid, push button accordingly
            if (push)
                for (int i = 1; i < buttonPressed.Length && !_hasStrike; i++)
                {
                    char[] c = buttonPressed[i].ToCharArray();

                    byte x = System.Convert.ToByte(c[0].ToString().ToUpper().ToCharArray()[0] - 64), y;
                    byte.TryParse(c[1].ToString(), out y);
                    y = (byte)(y - 1);

                    Buttons[(6 * y) + (x - 1)].OnInteract();
                    yield return new WaitForSeconds(0.35f);
                }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;

        for (int i = 0; i < _buttonStates.Length; i++)
        {
            if (!_buttonStates[i])
                break;

            if (i == _buttonStates.Length - 1)
                Buttons[i].OnInteract();
        }

        for (int i = 0; i < _buttonStates.Length; i++)
            if (!_buttonStates[i])
            {
                Buttons[i].OnInteract();
                yield return new WaitForSeconds(0.35f);
            }
    }
}