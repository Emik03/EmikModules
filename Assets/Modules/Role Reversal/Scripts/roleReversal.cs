using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using KModkit;

public class roleReversal : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public KMSelectable[] btn;
    public KMSelectable submit;
    public TextMesh screenText, submitText;
    public Component background;

    bool isSolved = false;

#pragma warning disable 414
    byte souvenir;
#pragma warning restore 414

    List<byte> redWires = new List<byte>(0), orangeWires = new List<byte>(0), yellowWires = new List<byte>(0),
               greenWires = new List<byte>(0), blueWires = new List<byte>(0), purpleWires = new List<byte>(0);
    List<List<byte>> wires;

    private bool _lightsOn = false, _displayWin = false;
    private sbyte _wireSelected = 0, _correctWire = 0, _frames = 0, _instructionsIndex = 1;
    private int _moduleId = 0, _seed = 0;
    private string _currentText = "";

    private List<char> _convertedSeed;

    private char[] _displayText = new char[0];
    private static readonly string[] _completeText = new string[9] { "C", "o", "m", "p", "l", "e", "t", "e", "!" };

    private static int _moduleIdCounter = 1;
    private static short _moduleCount = 0;

    private Color32 mainColor = new Color32(158, 133, 245, 255);

    /// <summary>
    /// Code that runs during the loading period.
    /// </summary>
    void Start()
    {
        _moduleCount = 0;
        _moduleId = _moduleIdCounter++;

        SFX.LogVersionNumber(Module, _moduleId);

        Module.OnActivate += Activate;
        UpdateColor();
    }

    /// <summary>
    /// Primarily used for animation, runs 50 times per second.
    /// </summary>
    private void FixedUpdate()
    {
        //frame counter
        _frames++;

        //update every fourth
        _frames %= 4;

        if (isSolved)
        {
            //if color transition is not complete, do the color transition
            if (mainColor.r != 0)
                UpdateColor();

            //display win message
            if (screenText.text.Length < 9 && _displayWin && _frames == 0)
                screenText.text += _completeText[screenText.text.Length];

            //remove text
            else if (!_displayWin)
            {
                if (screenText.text.Length >= 2)
                    screenText.text = screenText.text.Remove(screenText.text.Length - 2, 2);

                else
                {
                    screenText.text = "";
                    _displayWin = true;
                }
            }
        }

        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 1 && _frames != 0)
                    continue;

                if (screenText.text.Length < _displayText.Count())
                {
                    if (_displayText[screenText.text.Length] == '§')
                        screenText.text = screenText.text.Insert(screenText.text.Length, "\n");

                    else
                        screenText.text = screenText.text.Insert(screenText.text.Length, _displayText[screenText.text.Length].ToString());
                }
            }
        }

    }

    /// <summary>
    /// Button and wire initaliser, runs upon loading.
    /// </summary>
    private void Awake()
    {
        submit.OnInteract += delegate ()
        {
            HandleSubmit();
            return false;
        };

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
    /// Runs when the lights turn on.
    /// </summary>
    void Activate()
    {
        Init();
        _lightsOn = true;
        submitText.text = (_wireSelected + 1).ToString();
    }

    /// <summary>
    /// Generates the seed and runs other methods.
    /// </summary>
    private void Init()
    {
        _moduleCount++;

        //_seed!
        //generate seed
        _seed = Random.Range(0, 279936);

        //meme seed for thumbnail
        //_seed = 279935;
        //_seed = 122852;

        //run this method every time the screen needs to be updated
        DisplayScreen();

        //gives a ton of debug information about the conversion from seed to colored wires
        DisplayDebug();
    }

    /// <summary>
    /// Handles pressing of all buttons and screens (aside from submit)
    /// </summary>
    /// <param name="num">The index for the 5 buttons so the program can differentiate which button was pushed.</param>
    void HandlePress(int num)
    {
        //plays button sound effect
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn[num].transform);
        btn[num].AddInteractionPunch();

        //if lights are off, the buttons should do nothing
        if (!_lightsOn || isSolved) return;

        //0 and 1 are the left and right buttons for top panel that controls wire cutting
        //2 and 3 are the left and right buttons for the bottom panel that controls instructions
        //4 is the bottom panel itself that skips to the next section
        switch (num)
        {
            case 0:
                //top screen, previous selection
                _wireSelected--;
                _wireSelected += 7;
                _wireSelected %= 7;
                submitText.text = (_wireSelected + 1).ToString();
                break;

            case 1:
                //top screen, next selection
                _wireSelected++;
                _wireSelected += 7;
                _wireSelected %= 7;
                submitText.text = (_wireSelected + 1).ToString();
                break;

            case 2:
                //bottom screen, previous instruction
                _instructionsIndex--;
                _instructionsIndex += (sbyte)_instructions.Length;
                _instructionsIndex %= (sbyte)_instructions.Length;
                DisplayScreen();
                break;

            case 3:
                //buttom screen, next instruction
                _instructionsIndex++;
                _instructionsIndex += (sbyte)_instructions.Length;
                _instructionsIndex %= (sbyte)_instructions.Length;
                DisplayScreen();
                break;

            case 4:
                //skip to the correct sections
                _instructionsIndex /= 9;
                _instructionsIndex++;
                _instructionsIndex *= 9;
                _instructionsIndex %= (sbyte)_instructions.Length;

                DisplayScreen();
                break;
        }
    }

    /// <summary>
    /// Registers whether the answer provided by the player is correct or not.
    /// </summary>
    void HandleSubmit()
    {
        //plays button sound effect
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
        submit.AddInteractionPunch(3);

        //if lights are off or it's solved, the buttons should do nothing
        if (!_lightsOn || isSolved) return;

        //play cut wire sound effect
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, submit.transform);

        //calculates the answer
        CalculateAnswer();

        Debug.LogFormat("[Role Reversal #{0}] Time when cut: {1} seconds.", _moduleId, Info.GetTime());
        Debug.LogFormat("[Role Reversal #{0}] Role Reversal module count: {1} on the current bomb.", _moduleId, _moduleCount);

        //if the answer is correct
        if (_wireSelected + 1 == _correctWire)
        {
            Debug.LogFormat("[Role Reversal #{0}] Wire {1} was cut, correct wire was cut! Module solved!", _moduleId, _wireSelected + 1);

            Audio.PlaySoundAtTransform(SFX.Rv.Solve, submit.transform);

            //make module solved
            Module.HandlePass();
            isSolved = true;
        }

        else
        {
            Debug.LogFormat("[Role Reversal #{0}] Wire {1} was cut, incorrect wire was cut! Strike!", _moduleId, _wireSelected + 1);

            Audio.PlaySoundAtTransform(SFX.Rv.Strike, submit.transform);

            //make module strike
            Module.HandleStrike();
        }
    }

    /// <summary>
    /// Calculates the correct answer, logs it and puts it in the _correctWire variable.
    /// </summary>
    private void CalculateAnswer()
    {
        //reset wire calculation
        GetWires();
        _correctWire = 0;

        //counts the amount of wires
        switch (_convertedSeed.Count)
        {
            //2 wires!
            case 2:
                //if first is secondary and both triadic
                if (_convertedSeed[0] % 2 == 0 && Mathf.Abs((float)(char.GetNumericValue(_convertedSeed[0]) - char.GetNumericValue(_convertedSeed[1]))) == 2 || Mathf.Abs((float)(char.GetNumericValue(_convertedSeed[0]) - char.GetNumericValue(_convertedSeed[1]))) == 4)
                {
                    if (_convertedSeed[0] < _convertedSeed[1])
                        _correctWire = 1;

                    else
                        _correctWire = 2;

                    souvenir = 2;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 2 (If both are triadic): True, cut wire {1}.", _moduleId, _correctWire);
                }

                //if second wire is yellow
                else if (_convertedSeed[1] == '2')
                {
                    if (_convertedSeed[0] == '2')
                        _correctWire = 1;
                        
                    else
                        _correctWire = 2;

                    souvenir = 3;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 3 (If second wire is yellow): True, cut wire {1}.", _moduleId, _correctWire);
                }

                //if wire colors are the same
                else if (_convertedSeed[0] == _convertedSeed[1])
                {
                    _correctWire = 2;
                    souvenir = 4;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 4 (If both are the same): True, cut wire 2.", _moduleId);
                }

                //if 0 batteries
                else if (Info.GetBatteryCount() == 0)
                {
                    _correctWire = 1;
                    souvenir = 5;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 5 (If there are 0 batteries): True, cut wire 1.", _moduleId);
                }

                //if the wire colors are complementary
                else if (Mathf.Abs((float)(char.GetNumericValue(_convertedSeed[0]) - char.GetNumericValue(_convertedSeed[1]))) == 3)
                {
                    if (char.GetNumericValue(_convertedSeed[0]) < char.GetNumericValue(_convertedSeed[1]))
                        _correctWire = 2;

                    else
                        _correctWire = 1;

                    souvenir = 6;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 6 (If both are complementary): True, cut wire {1}.", _moduleId, _correctWire);
                }

                else if (_convertedSeed[0] > _convertedSeed[1])
                {
                    _correctWire = 2;
                    souvenir = 7;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 7 (Otherwise): True, cut wire 2.", _moduleId);
                }

                //otherwise
                else
                {
                    if (char.GetNumericValue(_convertedSeed[0]) % 2 == 0)
                        _correctWire = 2;

                    else
                        _correctWire = 1;

                    souvenir = 8;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 8 (Otherwise): True, cut wire {1}.", _moduleId, _correctWire);
                }
                break;

            //3 wires!
            case 3:
                //if serial is even and one purple
                if (purpleWires.Count == 1 && Info.GetSerialNumberNumbers().First() % 2 == 0)
                {
                    _correctWire = (sbyte)(purpleWires[0] + 1);
                    souvenir = 2;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 2 (If there is only one wire matching the module's color): True, cut wire {1}.", _moduleId, _correctWire);
                    break;
                }

                //if all wires share color
                else if (_convertedSeed.Distinct().Count() == 1)
                {
                    _correctWire = 2;
                    souvenir = 3;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 3 (If all wires share color): True, cut wire {1}.", _moduleId, _correctWire);
                    break;
                }

                //if there is only 1 of this module
                else if (_moduleCount != 1)
                {
                    _correctWire = 3;
                    souvenir = 4;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 4 (If only one of this module is present): True, cut wire 3.", _moduleId);
                    break;
                }

                //if warm color is to the left of cold color
                for (int i = 0; i < _convertedSeed.Count - 1; i++)
                    if (char.GetNumericValue(_convertedSeed[i]) <= 2 && char.GetNumericValue(_convertedSeed[i + 1]) >= 3)
                        for (int j = _convertedSeed.Count - 1; j >= 0; j--)
                            if (char.GetNumericValue(_convertedSeed[j]) >= 3)
                            {
                                _correctWire = (sbyte)(j + 1);
                                souvenir = 5;
                                Debug.LogFormat("[Role Reversal #{0}] Condition 5 (If a warm color is to the left of a cold color): True, cut wire {1}.", _moduleId, _correctWire);
                                break;
                            }

                if (_correctWire != 0)
                    break;

                //if serial contains letters found in module name
                if (Info.GetSerialNumberLetters().Any("ROLEVSAL".Contains))
                {
                    _correctWire = 1;
                    souvenir = 6;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 6 (If serial contains module name's letters): True, cut wire 1.", _moduleId);
                    break;
                }

                //if only two wires share color
                else if (_convertedSeed.Count - 1 == _convertedSeed.Distinct().Count())
                    for (int i = 0; i < wires.Count; i++)
                    {
                        if (wires[i].Count == 1)
                        {
                            _correctWire = (sbyte)(wires[i][0] + 1);
                            souvenir = 7;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 7 (If only two wires share color): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }
                    }

                if (_correctWire != 0)
                    break;

                //otherwise
                else
                {
                    _correctWire = 2;
                    souvenir = 8;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 8 (Otherwise): True, cut wire 2.", _moduleId);
                }
                break;

            //4 wires!
            case 4:
                if (Info.GetIndicators().Count() <= 2 && _convertedSeed[0] == '0')
                {
                    //if first wire is red
                    _correctWire = (sbyte)(redWires[redWires.Count - 1] + 1);
                    souvenir = 2;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 2 (If first wire is red): True, cut wire {1}.", _moduleId, _correctWire);
                }

                //if all wires are unique
                else if (_convertedSeed.Count == _convertedSeed.Distinct().Count())
                {
                    for (int i = 0; i < _convertedSeed.Count; i++)
                    {
                        if (_convertedSeed[i] == '1' || _convertedSeed[i] == '4' || _convertedSeed[i] == '5')
                        {
                            _correctWire = (sbyte)(i + 1);
                            souvenir = 3;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 3 (If all wires are unique): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }
                    }
                }

                //if less than 1 minute is left
                else if (Info.GetTime() < 60)
                {
                    _correctWire = 1;
                    souvenir = 4;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 4 (If less than 1 minute is left): True, cut wire 1.", _moduleId);
                    break;
                }

                if (_correctWire != 0)
                    break;

                //if all wires are sorted
                for (int i = 0; i < _convertedSeed.Count - 1; i++)
                {
                    //they aren't sorted
                    if (_convertedSeed[i] > _convertedSeed[i + 1])
                        break;

                    //they are sorted
                    if (i == _convertedSeed.Count - 2)
                    {
                        //linear search
                        for (int j = 0; j < _convertedSeed.Count; j++)
                        {
                            //if cold, or not unique
                            if (char.GetNumericValue(_convertedSeed[j]) >= 3 || wires[(int)char.GetNumericValue(_convertedSeed[j])].Count > 1)
                            {
                                _correctWire = (sbyte)(j + 1);
                                souvenir = 5;
                                Debug.LogFormat("[Role Reversal #{0}] Condition 5 (If wires are sorted): True, cut wire {1}.", _moduleId, _correctWire);
                                break;
                            }
                        }
                    }
                }

                if (_correctWire != 0)
                    break;

                //if 7 or less modules are on the bomb
                if (Info.GetSolvedModuleNames().Count <= 4 && Info.GetSolvedModuleNames().Count != 0)
                {
                    _correctWire = (sbyte)Info.GetSolvedModuleNames().Count;
                    souvenir = 6;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 6 (If 4 or less modules are solved): True, cut wire {1}.", _moduleId, _correctWire);
                }

                //if first wire is a warm color
                else if (_convertedSeed[0] == '0' || _convertedSeed[0] == '1' || _convertedSeed[0] == '2')
                {
                    for (int i = _convertedSeed.Count - 1; i >= 0; i--)
                    {
                        if (char.GetNumericValue(_convertedSeed[i]) <= 2)
                        {
                            _correctWire = (sbyte)(i + 1);
                            souvenir = 7;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 7 (If first wire is a warm color): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }
                    }
                }

                //otherwise
                else
                {
                    _correctWire = 2;
                    souvenir = 8;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 8 (Otherwise): True, cut wire 2.", _moduleId);
                }
                break;

            //5 wires!
            case 5:
                //if both red doesn't exist and orange exists
                if (!_convertedSeed.Contains('0') && _convertedSeed.Contains('1'))
                {
                    _correctWire = (sbyte)(orangeWires[0] + 1);
                    souvenir = 2;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 2 (If there are any orange wires): True, cut wire {1}.", _moduleId, _correctWire);
                }

                else
                {
                    for (int i = 0; i < _convertedSeed.Count - 1; i++)
                    {
                        //if yellow wire is to the left of green wire
                        if (_convertedSeed[i] == '2' && _convertedSeed[i + 1] == '3')
                        {
                            _correctWire = (sbyte)(yellowWires[0] + 1);
                            souvenir = 3;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 3 (If yellow wire to the left of green wire): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }
                    }

                    //if correctWire has been set
                    if (_correctWire != 0)
                        break;

                    for (int i = 0; i < _convertedSeed.Count - 1; i++)
                    {
                        //if yellow wire is to the right of green wire
                        if (_convertedSeed[i] == '3' && _convertedSeed[i + 1] == '2')
                        {
                            _correctWire = (sbyte)(greenWires[0] + 1);
                            souvenir = 4;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 4 (If yellow wire to the right of green wire): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }
                    }

                    //if correctWire has been set
                    if (_correctWire != 0)
                        break;

                    //if there is CAR or FRK label
                    if (Info.GetIndicators().Any("CAR".Contains) || Info.GetIndicators().Any("FRK".Contains))
                    {
                        _correctWire = (sbyte)((Info.GetOnIndicators().Count() % 7) + 1);
                        souvenir = 5;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 5 (If there is CAR or FRK label): True, cut wire {1}.", _moduleId, _correctWire);
                    }

                    //if one purple wire exists
                    else if (purpleWires.Count == 1)
                    {
                        _correctWire = (sbyte)(purpleWires[0] + 1);
                        souvenir = 6;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 6 (If one purple wire exists): True, cut wire {1}.", _moduleId, _correctWire);
                    }

                    //if any indicators are off
                    else if (Info.GetOffIndicators().Count() > 0)
                    {
                        _correctWire = 3;
                        souvenir = 7;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 7 (If any off indicators are off): True, cut wire 3.", _moduleId);
                    }

                    //otherwise
                    else
                    {
                        _correctWire = 2;
                        souvenir = 8;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 8 (Otherwise): True, cut wire 2.", _moduleId);
                    }

                }
                break;

            //6 wires!
            case 6:
                //if there aren't 2 numbers in serial and if there is a vowel
                if (Info.GetSerialNumberNumbers().Count() != 2 && Info.GetSerialNumberLetters().Any("AEIOU".Contains))
                {
                    _correctWire = 6;
                    souvenir = 2;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 2 (If serial has a vowel): True, cut wire 6.", _moduleId);
                    break;
                }
                
                //if all primary colors exist
                if (redWires.Count >= 1 && yellowWires.Count >= 1 && blueWires.Count >= 1)
                    for (int i = 0; i < _convertedSeed.Count; i++)
                        if (_convertedSeed[i] == '1' || _convertedSeed[i] == '4' || _convertedSeed[i] == '5')
                        {
                            _correctWire = (sbyte)(i + 1);
                            souvenir = 3;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 3 (If all primary colors exist): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }

                if (_correctWire != 0)
                    break;

                byte pairs = 0;
                byte triplets = 0;

                //if 2 pairs or 1 triplet
                foreach (List<byte> wire in wires)
                {
                    //check for pairs
                    if (wire.Count == 2)
                        pairs++;

                    //check for triplets
                    else if (wire.Count == 3)
                        triplets++;
                }

                //if seed is divisible by 5
                if (_seed % 5 == 0)
                {
                    _correctWire = 4;
                    souvenir = 4;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 4 (If seed is divisible by 5): True, cut wire 4.", _moduleId);
                }

                //if there is a triplet, or 2 pairs were detected previously
                else if (pairs == 2 || triplets == 1)
                {
                    /*
                     * runs through the order of the wires, based on what numerical value is in each wire,
                     * it runs through the index of wires, which will check whether there is one member
                     * the specified color. if not, it continues through the list until it finds one.
                    */
                    for (int i = 0; i < _convertedSeed.Count; i++)
                        if (wires[(int)char.GetNumericValue(_convertedSeed[i])].Count == 1)
                        {
                            _correctWire = (sbyte)(wires[System.Convert.ToSByte(char.GetNumericValue(_convertedSeed[i]))][0] + 1);
                            souvenir = 5;
                            Debug.LogFormat("[Role Reversal #{0}] Condition 5 (If only 2 pairs or only 1 triplet exist): True, cut wire {1}.", _moduleId, _correctWire);
                            break;
                        }
                }

                //if more than 600 seconds remain
                else if (Info.GetTime() > 600)
                {
                    _correctWire = 2;
                    souvenir = 6;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 6 (If more than 10 minutes remain): True, cut wire 2.", _moduleId);
                }

                //if seed is even
                else if (_seed % 2 == 0)
                {
                    _correctWire = 5;
                    souvenir = 7;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 7 (If seed is even): True, cut wire 5.", _moduleId);
                }

                //otherwise
                else
                {
                    _correctWire = 3;
                    souvenir = 8;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 8 (Otherwise): True, cut wire 3.", _moduleId);
                }
                break;

            //7 wires!
            case 7:
                //if there aren't more unlit than lit
                if (Info.GetOnIndicators().Count() < Info.GetOffIndicators().Count())
                {
                    //if there are less than 2 purple wires
                    if (purpleWires.Count < 2)
                    {
                        _correctWire = 7;
                        souvenir = 2;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 2 (If there are less than 2 purple wires): True, cut wire 7.", _moduleId);
                        break;
                    }
                }

                if (_correctWire != 0)
                    break;

                //if there are 3 blue wires
                if (blueWires.Count >= 3)
                {
                    _correctWire = (sbyte)(blueWires[1] + 2);
                    souvenir = 3;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 3 (If there are 3 blue wires): True, cut wire {1}.", _moduleId, _correctWire);
                    break;
                }

                //if serial has a matching number to red wires
                else if (Info.GetSerialNumber().Contains(System.Convert.ToChar(redWires.Count + 48)))
                {
                    _correctWire = 6;
                    souvenir = 4;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 4 (If the serial has a matching number to the number of red wires present): True, cut wire 6.", _moduleId);
                }

                //if there are less batteries than orange wires
                else if (Info.GetBatteryCount() < orangeWires.Count)
                {
                    _correctWire = (sbyte)(orangeWires[orangeWires.Count - 1] + 1);
                    souvenir = 5;
                    Debug.LogFormat("[Role Reversal #{0}] Condition 5 (If there are less batteries than orange wires): True, cut wire {1}.", _moduleId, _correctWire);
                }

                else
                {
                    //if 4 or more wires share the same color
                    for (int i = 0; i < wires.Count && _correctWire == 0; i++)
                    {
                        if (wires[i].Count >= 4)
                        {
                            for (int j = _convertedSeed.Count - 1; j >= 0 && _correctWire == 0; j--)
                            {
                                if (wires[(int)char.GetNumericValue(_convertedSeed[j])].Count != 1)
                                {
                                    _correctWire = (sbyte)(j + 1);
                                    souvenir = 6;
                                    Debug.LogFormat("[Role Reversal #{0}] Condition 6 (If 4 or more wires share the same color): True, cut wire {1}.", _moduleId, _correctWire);
                                }
                            }
                        }
                    }

                    if (_correctWire != 0)
                        break;

                    //if first, fourth, or last share any same color
                    if (_convertedSeed[0] == _convertedSeed[3] || _convertedSeed[0] == _convertedSeed[6] || _convertedSeed[3] == _convertedSeed[6])
                    {
                        _correctWire = 4;
                        souvenir = 7;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 7 (If first, fourth or last wire share any same colors): True, cut wire {1}.", _moduleId, _correctWire);
                        break;
                    }

                    //otherwise
                    else
                    {
                        _correctWire = 3;
                        souvenir = 8;
                        Debug.LogFormat("[Role Reversal #{0}] Condition 8 (Otherwise): True, cut wire 3.", _moduleId);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Counts the amount of wires and keeps track of their position.
    /// </summary>
    private void GetWires()
    {
        //reset all lists in case if it was ran previously
        redWires = new List<byte>(0);
        orangeWires = new List<byte>(0);
        yellowWires = new List<byte>(0);
        greenWires = new List<byte>(0);
        blueWires = new List<byte>(0);
        purpleWires = new List<byte>(0);

        //count number of wires that are any color
        for (int i = 0; i < _convertedSeed.Count; i++)
        {
            switch (_convertedSeed[i])
            {
                case '0':
                    redWires.Add((byte)i);
                    break;

                case '1':
                    orangeWires.Add((byte)i);
                    break;

                case '2':
                    yellowWires.Add((byte)i);
                    break;

                case '3':
                    greenWires.Add((byte)i);
                    break;

                case '4':
                    blueWires.Add((byte)i);
                    break;

                case '5':
                    purpleWires.Add((byte)i);
                    break;
            }
        }

        //all wires together
        wires = new List<List<byte>>(6) { redWires, orangeWires, yellowWires, greenWires, blueWires, purpleWires };
    }


    /// <summary>
    /// Updates what needs to be displayed on screen.
    /// </summary>
    private void DisplayScreen()
    {
        screenText.text = "";

        _currentText = "Seed: " + _seed.ToString();
        _currentText += "\n\n" + _instructions[_instructionsIndex];

        _displayText = new char[_currentText.Length];
        _displayText = _currentText.ToCharArray();
    }

    /// <summary>
    /// Logs information about the module's seed, wires, and their colors.
    /// </summary>
    private void DisplayDebug()
    {
        Debug.LogFormat("[Role Reversal #{0}] Seed: {1}", _moduleId, _seed);

        _convertedSeed = ConvertToB6(_seed);

        string _colorList = "";

        foreach (char wire in _convertedSeed)
        {
            switch (wire)
            {
                case '0':
                    _colorList += "Red ";
                    break;

                case '1':
                    _colorList += "Orange ";
                    break;

                case '2':
                    _colorList += "Yellow ";
                    break;

                case '3':
                    _colorList += "Green ";
                    break;

                case '4':
                    _colorList += "Blue ";
                    break;

                case '5':
                    _colorList += "Purple ";
                    break;
            }
        }

        Debug.LogFormat("[Role Reversal #{0}] Final Wires: {1}", _moduleId, _colorList);
    }

    /// <summary>
    /// Converts any number from base-10 to base-6.
    /// </summary>
    /// <param name="num">The base-10 number provided to convert the number.</param>
    /// <returns>The variable provided but in base-6.</returns>
    private List<char> ConvertToB6(int num)
    {
        string B6number = "";
        byte wireCount = (byte)(num % 6 + 2);

        Debug.LogFormat("[Role Reversal #{0}] Amount of Wires: {1}", _moduleId, wireCount);

        while (num >= 6)
        {
            B6number += System.Convert.ToString(num % 6);
            num /= 6;
        }

        B6number += System.Convert.ToString(num);
        List<char> result = B6number.Reverse().ToList();

        //ensures that if the base 6 conversion yields less than 7 wires, it should add leading 0's
        while (result.Count < 7)
            result.Insert(0, '0');

        while (wireCount < result.Count)
            result.RemoveAt(result.Count - 1);

        return result;
    }

    /// <summary>
    /// Updates the color of all objects within the module.
    /// </summary>
    private void UpdateColor()
    {
        //fade out rgb
        mainColor.r += (byte)((0 - mainColor.r) / 100);
        mainColor.g += (byte)((200 - mainColor.g) / 100);
        mainColor.b += (byte)((255 - mainColor.b) / 100);

        //change all colors
        submitText.color = mainColor;
        screenText.color = mainColor;

        //background changes color
        background.GetComponent<MeshRenderer>().material.color = new Color32((byte)(mainColor.r / 2), (byte)(mainColor.g / 2), (byte)(mainColor.b / 2), 255);

        //fifth button is a screen, so it isn't included here
        for (int i = 0; i < 4; i++)
            btn[i].GetComponent<MeshRenderer>().material.color = new Color32((byte)(mainColor.r), (byte)(mainColor.g), mainColor.b, 255);

        //both top and bottom panel
        submit.GetComponent<MeshRenderer>().material.color = new Color32((byte)(mainColor.r / 10), (byte)(mainColor.g / 10), (byte)(mainColor.b / 10), 255);
        btn[4].GetComponent<MeshRenderer>().material.color = new Color32((byte)(mainColor.r / 10), (byte)(mainColor.g / 10), (byte)(mainColor.b / 10), 255);
    }

    private string[] _instructions = new string[63]
    {
        //0
        "Tutorial",

        "On the Subject of\nRole Reversal.\n\nPress the bottom\nright button\nto continue.",
        "If you use the HTML\nmanual, the middle\ntile gives modulo,\nand the right base-6.",
        "Tell the expert the\nseed number above.\nModulo the seed by 6,\nthen add 2 will be the\namount of wires.",
        "Convert the seed\ninto base-6, each\ndigit represents a\ncolored wire.",
        "If you have less\nthan 7 digits, add\n0's at the beginning\nuntil you do.",
        "Convert digits using\nTable A and stop\nwhen you have as many\nwires as the amount\nof wires calculated.",
        "To cut a wire,\nnavigate with the\ntop buttons, then\ncut using the\ntop screen.",
        "Cycle to the correct\nsection by pressing\nthe bottom screen.\n\nGood luck!",

        //9
        "2 Wires",

        "2 Wires (Condition: 1)\n\nIf the first wire\nis a secondary, skip\nto Condition 3.",
        "2 Wires (Condition: 2)\n\nIf both wires are\ntriadic to each other,\ncut the lowest-\nvalued wire.",
        "2 Wires (Condition: 3)\n\nIf the second wire\nis yellow, cut the\nfirst yellow wire.",
        "2 Wires (Condition: 4)\n\nIf both wires are\ncolored the same,\ncut the second wire.",
        "2 Wires (Condition: 5)\n\nIf there are 0\n batteries, cut the\nfirst wire.",
        "2 Wires (Condition: 6)\n\nIf both wires are\ncomplemetary to\neach other, cut the\ncold-colored wire.",
        "2 Wires (Condition: 7)\n\nIf the first wire has\nthe higher value, cut\nthe second wire.",
        "2 Wires (Condition: 8)\n\nOtherwise, cut the\nfirst secondary-\ncolored wire.",

        //18
        "3 Wires",

        "3 Wires (Condition: 1)\n\nIf the first digit in\nthe serial is odd,\nskip to Condition 3.",
        "3 Wires (Condition: 2)\n\nIf there is only one\nwire matching this\nmodule's color, cut\nthat matching wire.",
        "3 Wires (Condition: 3)\n\nIf all wires share\nthe same color, cut\nthe second wire.",
        "3 Wires (Condition: 4)\n\nIf there isn't only one\nRole Reversal module,\ncut the third wire,\nalso thanks!",
        "3 Wires (Condition: 5)\n\nIf any warm color\ncomes before a\ncold one, cut the\nlast cold wire.",
        "3 Wires (Condition: 6)\n\nIf the serial contains\nany letters found in\nthe module name,\ncut the first wire.",
        "3 Wires (Condition: 7)\n\nIf only two of the\nwires share the\nsame color, cut\nthe unique wire.",
        "3 Wires (Condition: 8)\n\nOtherwise, cut the\nsecond wire.",

        //27
        "4 Wires",

        "4 Wires (Condition: 1)\n\nIf you have more\nthan 2 indicators,\nskip to Condition 3.",
        "4 Wires (Condition: 2)\n\nIf the first wire is\nred, cut the\nlast red wire.",
        "4 Wires (Condition: 3)\n\nIf there are 4 unique\ncolored wires, cut\nthe first wire with\nvalues 1, 4, or 5.",
        "4 Wires (Condition: 4)\n\nIf less than 1 minute\nis remaining, cut\nthe first wire.",
        "4 Wires (Condition: 5)\n\nIf all wires are\nsorted, cut the first\ncold-colored or\nnon-unique wire.",
        "4 Wires (Condition: 6)\n\nIf there's 1, 2, 3, or 4\nsolved modules, cut\nwire (solved modules).",
        "4 Wires (Condition: 7)\n\nIf the first wire is\na warm color,\ncut the last warm-\ncolored wire.",
        "4 Wires (Condition: 8)\n\nOtherwise, cut the\nsecond wire.",

        //36
        "5 Wires",

        "5 Wires (Condition: 1)\n\nIf there are any red\nwires, skip to\nCondition 3.",
        "5 Wires (Condition: 2)\n\nIf there are any\norange wires, cut the\nfirst orange wire.",
        "5 Wires (Condition: 3)\n\nIf any yellow wires\nlie adjacently left\nto a green wire,\ncut the first\nyellow wire.",
        "5 Wires (Condition: 4)\n\nIf any yellow wires\nlie adjacently right\nto a green wire,\ncut the first\ngreen wire.",
        "5 Wires (Condition: 5)\n\nIf indicator FRK or\nCAR exist, cut\n(N modulo 7) + 1 for\nN lit indicators.",
        "5 Wires (Condition: 6)\n\nIf there is only one\npurple wire, cut that\npurple wire.",
        "5 Wires (Condition: 7)\n\nIf any indicators\nare off, cut the\nthird wire.",
        "5 Wires (Condition: 8)\n\nOtherwise, cut the\nsecond wire.",

        //45
        "6 Wires",

        "6 Wires (Condition: 1)\n\nIf the serial has\nexactly 2 digits,\nskip to Condition 3.",
        "6 Wires (Condition: 2)\n\nIf the serial has\nany vowel, cut\nthe sixth wire.",
        "6 Wires (Condition: 3)\n\nIf all primary colors\nexist, cut the first\nwire that has its last\nletter an E.",
        "6 Wires (Condition: 4)\n\nIf the seed is\ndivisible by 5, cut\nthe fourth wire.",
        "6 Wires (Condition: 5)\n\nIf exactly 2 pairs\nor exactly 1 triplet\nmatch colors, cut the\nfirst unique wire.",
        "6 Wires (Condition: 6)\n\nIf more than 10\nminutes are\nremaining, cut\nthe second wire.",
        "6 Wires (Condition: 7)\n\nIf the seed is even,\ncut the fifth wire.",
        "6 Wires (Condition: 8)\n\nOtherwise, cut the\nthird wire.",

        //54
        "7 Wires",

        "7 Wires (Condition: 1)\n\nIf there are as many\nor more lit indicators\nas unlit indicators,\nskip to Condition 3.",
        "7 Wires (Condition: 2)\n\nIf there aren't 2 or\nmore purple wires,\ncut the seventh wire.",
        "7 Wires (Condition: 3)\n\nIf there are 3 or more\nblue wires, cut the\nwire after the second\nblue wire.",
        "7 Wires (Condition: 4)\n\nIf the serial has any\nmatching numbers to\namount of red wires,\ncut the sixth wire.",
        "7 Wires (Condition: 5)\n\nIf there are less\nbatteries than orange\nwires, cut the\nlast orange wire.",
        "7 Wires (Condition: 6)\n\nIf there are 4 or\nmore of the same\ncolors, cut the\nlast non-unique wire.",
        "7 Wires (Condition: 7)\n\nIf the first, fourth,\nor last wire share\nany color, cut the\nfourth wire.",
        "7 Wires (Condition: 8)\n\nOtherwise, cut the\nthird wire.",
    };

    /// <summary>
    /// Determines whether the input from the TwitchPlays chat command is valid or not.
    /// </summary>
    /// <param name="par">The string from the user.</param>
    private bool IsValid(string par, bool submit)
    {
        byte b;
        //cut wire 1-7
        if (submit)
            return byte.TryParse(par, out b) && b < 8 && b != 0;

        //wire 1-7 (1 is tutorial), condition 0-8 (0 is section header)
        if (par.Length != 3)
            return false;

        return char.GetNumericValue(par.ToCharArray()[0]) < 8 && char.GetNumericValue(par.ToCharArray()[0]) != 0 && par.ToCharArray()[1] == '.' && char.GetNumericValue(par.ToCharArray()[2]) != 0 && char.GetNumericValue(par.ToCharArray()[2]) < 9;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} cut <#> (Cuts the wire '#' | valid numbers are from 1-7) !{0} manual <#>.<#> (Left digit refers to amount of wires, right digit refers to instruction count. If you don't know how this module works, do manual 1:3, manual 1:4, manual 1:5...)";
#pragma warning restore 414

    /// <summary>
    /// TwitchPlays Compatibility, detects every chat message and clicks buttons accordingly.
    /// </summary>
    /// <param name="command">The twitch command made by the user.</param>
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.Split(' ');

        //if command is formatted correctly
        if (Regex.IsMatch(commands[0], @"^\s*cut\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            //if command has no parameters
            if (commands.Length < 2)
                yield return "sendtochaterror Please specify the wire you want to cut! (Valid: 1-7)";

            //if command has too many parameters
            else if (commands.Length > 2)
                yield return "sendtochaterror Too many wires requested! Only one can be cut at any time.";

            //if command has an invalid parameter
            else if (!IsValid(commands.ElementAt(1), true))
                yield return "sendtochaterror Invalid number! Only wires 1-7 can be pushed.";

            //if command is valid, cut wire accordingly
            else
            {
                while (true)
                {
                    if (char.GetNumericValue(commands[1].ToCharArray()[0]) == _wireSelected + 1)
                        break;

                    btn[1].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }

                submit.OnInteract();
            }
        }

        //if command is formatted correctly
        if (Regex.IsMatch(commands[0], @"^\s*manual\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            //if command has no parameters
            if (commands.Length < 2)
                yield return "sendtochaterror Please specify the manual section you want to read! (Valid: <#>.<#>, 2-7.1-8)";

            //if command has too many parameters
            else if (commands.Length > 2)
                yield return "sendtochaterror Too many instructions sent! Only one page can be viewed at a time.";

            //if command has an invalid parameter
            else if (!IsValid(commands.ElementAt(1), false))
                yield return "sendtochaterror Invalid instruction! Expected: <#>.<#>, 2-7.1-8";

            //if command is valid, go to section accordingly
            else
            {
                btn[4].OnInteract();

                while (char.GetNumericValue(commands[1][0]) != (_instructionsIndex / 9) + 1)
                {
                    btn[4].OnInteract();
                    yield return new WaitForSeconds(0.02f);
                }

                while (char.GetNumericValue(commands[1][2]) != _instructionsIndex % 9)
                {
                    btn[3].OnInteract();
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
    }

    /// <summary>
    /// Force the module to be solved in TwitchPlays
    /// </summary>
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Debug.LogFormat("[Role Reversal #{0}] Admin has initated a forced solve...", _moduleId);

        CalculateAnswer();
        while (true)
        {
            if (_correctWire == _wireSelected + 1)
                break;

            btn[1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        submit.OnInteract();
    }
}