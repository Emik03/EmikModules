using KModkit;
using SortingModule;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;

public class Sorting : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public TextMesh Screen;
    public Component background;
    public KMSelectable[] btn;
    public TextMesh[] txt;
    public Transform[] pos;

    internal SortingAlgorithms algorithms = new SortingAlgorithms();

    bool isSolved = false;
    byte swapButtons = 0, swapIndex = 1;
    string _currentAlgorithm = "";

    private Color32 _buttonColor = new Color32(96, 176, 176, 255),
                    _blinkingColor = new Color32(144, 255, 255, 255),
                    _highlightColor = new Color32(255, 255, 255, 255),
                    _backgroundColor = new Color32(48, 128, 144, 255),
                    _strikeColor = new Color32(255, 128, 128, 255),
                    _strikeBackgroundColor = new Color32(128, 64, 64, 255);

    private bool _lightsOn = false, _buttonDelay = false, _bogoSort = false, _doAction = false, _piano = false;
    private byte _frames = 0, _pushTimes = 0;
    private int _moduleId = 0;
    private float _animate = 1;
    
    private List<byte> _selected = new List<byte>();
    private byte[] _buttons = new byte[5], _initialButtons = new byte[5];
    private readonly string[] _algorithms = new string[15]
    {
        "BUBBLE", "SELECTION", "INSERTION", "RADIX", "MERGE", "COMB", "HEAP", "COCKTAIL", "ODDEVEN", "CYCLE", "FIVE", "QUICK", "SLOW", "SHELL", "STOOGE"
    };

    private static bool _playSound = true;
    private static int _moduleIdCounter = 1;

    /// <summary>
    /// Animation for clearing the module.
    /// </summary>
    private void FixedUpdate()
    {
        if (_animate < 1)
        {
            //buttons move towards positions
            btn[_selected[0]].transform.localPosition = Vector3.Lerp(pos[0].localPosition, pos[1].localPosition, QuarticOut(_animate));
            btn[_selected[1]].transform.localPosition = Vector3.Lerp(pos[1].localPosition, pos[0].localPosition, QuarticOut(_animate));

            _animate += 0.05f;

            //commit an actual swap
            if (_animate > 1)
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    //swapping positions
                    btn[_selected[i]].transform.localPosition = pos[i].localPosition;
                }

                ResetButtons();
            }
        }

        //force question marks
        if (_bogoSort)
        {
            //makes all numbers question marks
            for (int i = 0; i < btn.Length; i++)
            {
                txt[i].text = "??";
            }
        }

        _frames = (byte)((_frames + 1) % 64);

        //fun effect for clearing it, flashes colors back and forth
        for (int i = 0; i < btn.Length; i++)
        {
            if (btn[i].GetComponent<MeshRenderer>().material.color == _highlightColor || btn[i].GetComponent<MeshRenderer>().material.color == _strikeColor)
                continue;

            if (!isSolved)
            {
                if (_frames >= i * 8 && _frames <= (i + 3) * 8)
                    btn[i].GetComponent<MeshRenderer>().material.color = _blinkingColor;

                else
                    btn[i].GetComponent<MeshRenderer>().material.color = _buttonColor;
            }

            else if (_frames % 8 == 0)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    btn[i].GetComponent<MeshRenderer>().material.color = _blinkingColor;
                    btn[4 - i].GetComponent<MeshRenderer>().material.color = _blinkingColor;
                }

                else
                {
                    btn[i].GetComponent<MeshRenderer>().material.color = _buttonColor;
                    btn[4 - i].GetComponent<MeshRenderer>().material.color = _buttonColor;
                }
            }
        }
    }

    /// <summary>
    /// Code that runs when bomb is loading.
    /// </summary>
    private void Start()
    {
        Module.OnActivate += Activate;
        _moduleId = _moduleIdCounter++;

        SFX.LogVersionNumber(Module, _moduleId);
    }

    /// <summary>
    /// Lights get turned on.
    /// </summary>
    void Activate()
    {
        Init();
        _lightsOn = true;
    }

    /// <summary>
    /// Initalising buttons.
    /// </summary>
    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            int j = i;
            btn[i].OnInteract += delegate ()
            {
                HandlePress(j);
                return false;
            };
        }
    }

    /// <summary>
    /// Generates the numbers of the buttons and the sorting algorhithm needed.
    /// </summary>
    private void Init()
    {
        if (_playSound)
        {
            Audio.PlaySoundAtTransform(SFX.Srt.Bogosort, Module.transform);
            _playSound = false;
        }

        byte sorted = 0;

        //loop if the scramble happens to be already sorted
        do
        {
            sorted = 0;

            //generates new scramble
            for (int i = 0; i < 5; i++)
                GenerateNumber(i);

            //checks to see how many are sorted
            for (int i = 0; i < _buttons.Length - 1; i++)
            {
                if (_buttons[i] <= _buttons[i + 1])
                    sorted++;
            }
        } while (sorted == _buttons.Length - 1);

        Debug.LogFormat("[Sorting #{0}] The buttons are laid out as follows: {1}, {2}, {3}, {4}, {5}", _moduleId, _initialButtons[0], _initialButtons[1], _initialButtons[2], _initialButtons[3], _initialButtons[4]);

        //get random algorithm
        _currentAlgorithm = _algorithms[Random.Range(0, _algorithms.Length)];
        //_currentAlgorithm = "SHELL";
        Screen.text = _currentAlgorithm;

        Debug.LogFormat("[Sorting #{0}] Algorithm recieved: {1}", _moduleId, Screen.text);

        //_initialButtons = new byte[5] { 1, 2, 3, 4, 5 };
        if (System.DateTime.Now.Month != 4 || System.DateTime.Now.Day != 1)
            return;

        Screen.text = "BOGO";

        Debug.LogFormat("[Sorting #{0}] BogoSort activated!", _moduleId);
        Debug.LogFormat("[Sorting #{0}] All logs from this module are now disabled to prevent spam during BogoSort.", _moduleId);

        _bogoSort = true;

        for (int i = 0; i < _buttons.Length; i++)
        {
            byte rng = (byte)Random.Range(10, 100);

            //get random numbers
            if (!_buttons.Contains(rng))
            {
                _buttons[i] = rng;

                Debug.LogFormat("[Sorting #{0}] Button {1} has the label \"{2}\".", _moduleId, i + 1, rng);
            }

            //duplicate number prevention
            else
                i--;
        }
    }

    /// <summary>
    /// Generates a new problem from scratch.
    /// </summary>
    /// <param name="num">The index of the buttons array.</param>
    private void GenerateNumber(int num)
    {
        //resets required swap count before breaking
        swapIndex = 1;

        byte rng = (byte)Random.Range(0, 100);

        //get random numbers
        if (!_buttons.Contains(rng))
        {
            _initialButtons[num] = rng;
            _buttons[num] = rng;
        }

        //duplicate number prevention
        else
            GenerateNumber(num);

        ResetButtons();
    }

    /// <summary>
    /// Resets all numbers back to their original state, call this when striked.
    /// </summary>
    private void ResetNumber()
    {
        //resets required swap count before breaking
        swapIndex = 1;

        //get initial buttons
        for (int i = 0; i < _initialButtons.Length; i++)
        {
            _buttons[i] = _initialButtons[i];
            txt[i].text = _initialButtons[i].ToString();

            while (txt[i].text.Length < 2)
                txt[i].text = txt[i].text.Insert(0, "0");
        }

        Screen.text = _currentAlgorithm;
    }

    /// <summary>
    /// Handles pressing of all buttons and screens (aside from submit)
    /// </summary>
    /// <param name="num">The index for the 5 buttons so the program can differentiate which button was pushed.</param>
    private void HandlePress(int num)
    {
        //plays button sound effect
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn[num].transform);
        btn[num].AddInteractionPunch();
        Audio.PlaySoundAtTransform(SFX.Srt.Tick, Module.transform);

        //if lights are off, the buttons should do 
        if (!_lightsOn || isSolved || _buttonDelay || _piano)
        {
            Audio.PlaySoundAtTransform(SFX.Srt.Button(num + 1), Module.transform);

            if (_piano)
                btn[num].GetComponent<MeshRenderer>().material.color = btn[num].GetComponent<MeshRenderer>().material.color == _highlightColor
                                                                     ? btn[num].GetComponent<MeshRenderer>().material.color = _buttonColor
                                                                     : btn[num].GetComponent<MeshRenderer>().material.color = _highlightColor;
            return;
        }

        //selecting a button
        if (!_selected.Contains((byte)num))
        {
            Audio.PlaySoundAtTransform(SFX.Srt.Select, Module.transform);
            _selected.Add((byte)num);
            btn[num].GetComponent<MeshRenderer>().material.color = _highlightColor;
            _pushTimes++;
        }

        //unselecting a button
        else
        {
            Audio.PlaySoundAtTransform(SFX.Srt.Deselect, Module.transform);
            _selected.Remove((byte)num);
            btn[num].GetComponent<MeshRenderer>().material.color = _buttonColor;
        }

        //if you selected 2 buttons
        if (_selected.Count == 2)
        {
            //block inputs from user temporarily
            _buttonDelay = true;

            Audio.PlaySoundAtTransform(SFX.Srt.Swap, Module.transform);
            CheckSwap();
        }

        //bogosort easter egg activation
        if (_pushTimes == 55)
        {
            Audio.PlaySoundAtTransform(SFX.Srt.Bogosort, Module.transform);
            _pushTimes = 0;

            //regenerates the numbers to prevent memorization
            for (int i = 0; i < btn.Length; i++)
            {
                GenerateNumber(i);
            }

            Debug.LogFormat("[Sorting #{0}] The buttons are laid out as follows: {1}, {2}, {3}, {4}", _moduleId, _initialButtons[0], _initialButtons[1], _initialButtons[2], _initialButtons[3]);

            //bogosort
            if (!_bogoSort)
            {
                Debug.LogFormat("[Sorting #{0}] BogoSort activated!", _moduleId);
                Debug.LogFormat("[Sorting #{0}] All logs from this module are now disabled to prevent spam during BogoSort.", _moduleId);

                _bogoSort = true;

                //set screen text to bogosort and run bogosort method
                Screen.text = "BOGO";

                for (int i = 0; i < _buttons.Length; i++)
                {
                    byte rng = (byte)Random.Range(10, 100);

                    //get random numbers
                    if (!_buttons.Contains(rng))
                    {
                        _buttons[i] = rng;

                        Debug.LogFormat("[Sorting #{0}] Button {1} has the label \"{2}\".", _moduleId, i + 1, rng);
                    }

                    //duplicate number prevention
                    else
                        i--;
                }
            }

            else
            {
                Debug.LogFormat("[Sorting #{0}] BogoSort deactivated!", _moduleId);
                Debug.LogFormat("[Sorting #{0}] All logs from this module are now reenabled.", _moduleId);

                _bogoSort = false;

                ResetNumber();
            }
        }
    }

    /// <summary>
    /// Swaps the two buttons selected.
    /// </summary>
    private void CheckSwap()
    {
        //reset bogosort easter egg progress
        _pushTimes = 0;

        //information dump, bogosort should not state this information due to potential spam with the amount of swaps you have to make
        if (!_bogoSort)
            Debug.LogFormat("[Sorting #{0}] Swapping buttons {1} to {2}", _moduleId, _buttons[_selected[0]], _buttons[_selected[1]]);

        //ensures that the highest number is the least significant digit
        if (_selected[0] < _selected[1])
        {
            //lower number translates to most significant digit
            swapButtons = (byte)((_selected[0] + 1) * 10);
            swapButtons += (byte)(_selected[1] + 1);
        }

        else
        {
            //lower number translates to most significant digit
            swapButtons = (byte)((_selected[1] + 1) * 10);
            swapButtons += (byte)(_selected[0] + 1);
        }

        //checks if the swap is valid
        if (algorithms.IfValid(Screen.text, _initialButtons, swapButtons, swapIndex, _moduleId, Info.GetSerialNumberNumbers))
            DoSwap();

        else
        {
            Debug.LogFormat("[Sorting #{0}] Swap was invalid! Strike! The buttons have been reorganized back into their original state.", _moduleId);
            Audio.PlaySoundAtTransform(SFX.Srt.Strike, Module.transform);
            Module.HandleStrike();
            _doAction = false;
            _bogoSort = false;

            background.GetComponent<MeshRenderer>().material.color = _strikeBackgroundColor;

            //resets the buttons
            for (int i = 0; i < btn.Length; i++)
            {
                btn[i].GetComponent<MeshRenderer>().material.color = _strikeColor;
            }

            //reset buttons so that they are ready to be pressed again
            ResetNumber();
            Invoke("ResetButtons", 0.2f);
        }
    }

    private void DoSwap()
    {
        if (!_bogoSort)
            Audio.PlaySoundAtTransform(SFX.Srt.SuccessfulSwap, Module.transform);

        _buttonDelay = true;
        swapIndex++;
        //Debug.LogFormat("Swap index is now {0}", swapIndex);

        //gets the positions of both buttons
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i].localPosition = btn[_selected[i]].transform.localPosition;
        }

        //the update function will animate this for however many frames this is set to
        _animate = 0;

        //swapping labels
        byte temp = _buttons[_selected[0]];
        _buttons[_selected[0]] = _buttons[_selected[1]];
        _buttons[_selected[1]] = temp;

        string debugList = "";

        //get current information about buttons
        for (int i = 0; i < _buttons.Length; i++)
        {
            debugList += _buttons[i].ToString() + " ";
        }

        //information dump, bogosort should not state this information due to potential spam with the amount of swaps you have to make
        if (!_bogoSort)
            Debug.LogFormat("[Sorting #{0}] Swap was valid! Both buttons have switched positions. Current position: {1}", _moduleId, debugList);

        //check if module has been solved
        CheckSolved();
    }

    /// <summary>
    /// Reset the button colors.
    /// </summary>
    private void ResetButtons()
    {
        //clear lists and allow button registries
        _buttonDelay = false;

        background.GetComponent<MeshRenderer>().material.color = _backgroundColor;

        //resets the buttons
        for (int i = 0; i < btn.Length; i++)
        {
            btn[i].GetComponent<MeshRenderer>().material.color = _buttonColor;

            //if it's bogosort, it should remain as ??
            if (!_bogoSort)
            {
                txt[i].text = _buttons[i].ToString();

                while (txt[i].text.Length < 2)
                    txt[i].text = txt[i].text.Insert(0, "0");
            }
        }

        //resets selected
        _selected = new List<byte>();
    }

    private void CheckSolved()
    {
        byte sorted = 0;

        //checks to see how many are sorted
        for (int i = 0; i < _buttons.Length - 1; i++)
        {
            if (_buttons[i] <= _buttons[i + 1])
                sorted++;
        }

        //checks if everything is sorted
        if (sorted == _buttons.Length - 1)
        {
            Screen.text = "SORTED!";
            isSolved = true;
            Audio.PlaySoundAtTransform(SFX.Srt.Solve, Module.transform);

            Debug.LogFormat("[Sorting #{0}] All buttons sorted, module solved!", _moduleId);
            Module.HandlePass();
        }
    }

    protected static float QuarticOut(float k)
    {
        return 1f - ((k -= 1f) * k * k * k);
    }

    /// <summary>
    /// Determines whether the input from the TwitchPlays chat command is valid or not.
    /// </summary>
    /// <param name="par">The string from the user.</param>
    private bool IsValid(string par)
    {
        string[] validNumbers = { "1", "2", "3", "4", "5" };
        char[] c = par.ToCharArray();

        if (_piano)
            return c.Length == 2 && (validNumbers.Contains(c[0].ToString()) || validNumbers.Contains(c[1].ToString()));

        else
            for (int i = 0; i < c.Length; i++)
                if (validNumbers.Contains(c[i].ToString()))
                    return false;

        return true;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} swap <##> (Swaps the labels in position '#' | valid numbers are from 1-5 | example: !swap 24 13 swaps 2 & 4 then 1 & 3)";
#pragma warning restore 414

    /// <summary>
    /// TwitchPlays Compatibility, detects every chat message and clicks buttons accordingly.
    /// </summary>
    /// <param name="command">The twitch command made by the user.</param>
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] buttonSwapped = command.Split(' ');

        if (Regex.IsMatch(buttonSwapped[0], @"^\s*piano\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            _piano = !_piano;
            for (int i = 0; i < btn.Length; i++)
                btn[i].GetComponent<MeshRenderer>().material.color = _buttonColor;
        }

        //if command is formatted correctly
        else if (Regex.IsMatch(buttonSwapped[0], @"^\s*swap\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            if (_piano)
            {
                for (int i = 0; i < buttonSwapped[1].Length; i++)
                {
                    byte seq = 0;
                    if (byte.TryParse(buttonSwapped[1][i].ToString(), out seq) && seq <= 5 && seq != 0)
                        btn[seq - 1].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }

            else
            {
                _doAction = true;
                for (int i = 1; i < buttonSwapped.Length; i++)
                {
                    string[] validNumbers = { "1", "2", "3", "4", "5" };
                    char[] c = buttonSwapped[i].ToCharArray();
                    if (c.Length != 2 || !validNumbers.Contains(c[0].ToString()) || !validNumbers.Contains(c[1].ToString()))
                        _doAction = false;
                }

                if (!_doAction)
                    yield return "sendtochaterror Invalid number! Only label positions 1-5 can be swapped. Expected a two-digit number.";

                else
                {
                    for (int i = 1; i < buttonSwapped.Length; i++)
                    {
                        if (!_doAction)
                            break;

                        char[] c = buttonSwapped[i].ToCharArray();
                        byte seq1 = 0, seq2;

                        byte.TryParse(c[0].ToString(), out seq1);
                        byte.TryParse(c[1].ToString(), out seq2);

                        btn[seq1 - 1].OnInteract();

                        yield return new WaitForSeconds(0.2f);

                        btn[seq2 - 1].OnInteract();

                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Force the module to be solved in TwitchPlays
    /// </summary>
    IEnumerator TwitchHandleForcedSolve()
    {
        Module.HandlePass();
        isSolved = true;
        yield return null;
    }
}