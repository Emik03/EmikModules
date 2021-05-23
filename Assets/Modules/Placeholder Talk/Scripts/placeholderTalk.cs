using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;

public class placeholderTalk : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;
    public Transform screen;
    public TextMesh screenText;
    public KMSelectable[] btn;
    public TextMesh[] txt;
    public Transform[] anchor;

    bool isSolved = false;
    byte answerId;
    sbyte previousModules = 0;

#pragma warning disable 414
    string currentOrdinal, firstString;
#pragma warning restore 414

    private bool _lightsOn = false, _isRandomising = false, formatText = true, _animate = true, _debug = false;
    private byte _questionId, _questionOffsetId, _randomised = 0, frames = 0;
    private short _answerOffsetId, _strikes;
    private int _moduleId = 0;
    private float _yAnchor, _zScale = 0.12f;
    private string _screenText1 = "", _screenText2 = "";

    private static bool _playSound;
    private static int _moduleIdCounter = 1;

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
    /// Initalising buttons.
    /// </summary>
    private void Awake()
    {
        previousModules = 0;
        _playSound = true;

        for (int i = 0; i < 4; i++)
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
    /// Lights get turned on.
    /// </summary>
    void Activate()
    {
        Init();
        _lightsOn = true;
    }

    /// <summary>
    /// Runs the text flicker effect.
    /// </summary>
    private void FixedUpdate()
    {
        //makes the z coordinate based on sine waves for each button
        for (int i = 0; i < btn.Length; i++)
        {
            //adjust sine wave here
            float amplified = 0.002f;
            byte frequency = 4;

            //sine wave
            if (_animate)
                anchor[i].transform.localPosition = new Vector3(anchor[i].transform.localPosition.x, anchor[i].transform.localPosition.y, Mathf.Sin(frequency * Time.time + i * Mathf.PI / 2) * amplified - 0.062f);

            //stores all positions
            float x = anchor[i].transform.position.x;
            float y = anchor[i].transform.position.y;
            float z = anchor[i].transform.position.z;

            //if it's solved the buttons should move inwards
            if (isSolved && _yAnchor <= 250)
            {
                _yAnchor += 1f;

                if (_yAnchor < 145)
                {
                    //sets fade out
                    txt[i].color = new Color32(0, 0, 0, (byte)(145 - _yAnchor));
                    screenText.color = new Color32(255, 216, 0, (byte)(145 - _yAnchor));
                }
                else
                {
                    //sets all fonts to be invisible as the screen scales accordingly
                    _zScale -= _zScale / 17;
                    anchor[i].transform.localPosition = new Vector3(anchor[i].transform.localPosition.x, anchor[i].transform.localPosition.y - ((_yAnchor - 145) / 50000), anchor[i].transform.localPosition.z);

                    txt[i].color = new Color32(0, 0, 0, 0);
                    screenText.color = new Color32(255, 216, 0, 0);
                }

                if (_yAnchor == 250)
                {
                    //disable sinewave animation, finishes animation
                    _animate = false;
                    _zScale = 0;

                    //hides it behind the module
                    screen.transform.position = new Vector3(x, y, z);
                }
            }

            //update button positions and screen scaling
            btn[i].transform.position = new Vector3(x, y, z);
            screen.localScale = new Vector3(0.12f, 0.02f, _zScale);
        }

        if (_isRandomising && !_debug)
        {
            //frame counter, a cycle is 3 frames
            frames++;
            frames %= 3;

            //play sound effect once
            if (_randomised == 0 && _playSound)
            {
                Audio.PlaySoundAtTransform(SFX.Pht.Shuffle, Module.transform);
                _playSound = false;
            }

            //if cycle is prepped
            if (frames == 0)
            {
                //shuffle the text 20 times
                if (_randomised < 20)
                    UpdateText(true);

                //after shuffling it 20 times, display the phrases
                else
                {
                    UpdateText(false);

                    frames = 0;
                    _randomised = 0;
                    _isRandomising = false;
                }
            }
        }

        //debug code
        else if (_debug)
        {
            //frame counter, a cycle is however many frames it modulates
            frames++;
            frames %= 255;

            if (frames == 0)
                UpdateText(false);
        }
    }

    /// <summary>
    /// Generate new phrases and calculate the answer of the module.
    /// </summary>
    void Init()
    {
        previousModules = 0;
        currentOrdinal = ordinals[Random.Range(0, ordinals.Length)];

        if (!_debug)
        {
            _questionOffsetId = (byte)Random.Range(0, firstPhrase.Length);
            _questionId = (byte)Random.Range(0, _secondPhrase.Length);
        }
        else
        {
            _questionOffsetId = 15;
            _questionId = 147;
        }

        firstString = firstPhrase[_questionOffsetId];

        Debug.LogFormat("[Placeholder Talk #{0}] First Phrase ({1}): \"THE ANSWER {2} {3}\"", _moduleId, _questionOffsetId, firstPhrase[_questionOffsetId], currentOrdinal);
        Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase ({1}): \"{2}\"", _moduleId, _questionId, _secondPhrase[_questionId].Replace("\n\n", " "));

        //start the shuffling generation effect
        _isRandomising = true;

        //generate an answer
        Debug.LogFormat("[Placeholder Talk #{0}] (First Phrase + Second Phrase) modulated by 4 = {1}. Push the button labeled {1}.", _moduleId, GetAnswer() + 1);
    }

    /// <summary>
    /// Renders the text on screen.
    /// </summary>
    /// <param name="random">Determine whether or not the text rendered should be random.</param>
    void UpdateText(bool random)
    {
        //if not in a debugging state
        if (!_debug)
        {
            //if the text should be random
            if (!random)
            {
                //render the real text
                _screenText1 = "THE ANSWER ";
                _screenText1 += firstPhrase[_questionOffsetId] + "\n\n";
                _screenText1 += currentOrdinal + "\n\n";
                _screenText1 += "\n\n";
                _screenText2 = _secondPhrase[_questionId];

                //render the text
                RenderText();
            }
            else
            {
                //empty all text
                _screenText1 = "";
                _screenText2 = "";

                //render text randomly
                _randomised++;

                //render the text
                RandomText();
            }
        }

        //debug
        else
        {
            _questionId++;
            _questionId %= 164;

            //debug
            _screenText1 = "THE ANSWER ";
            _screenText1 += firstPhrase[15] + "\n\n";
            _screenText1 += ordinals[7] + "\n\n";
            _screenText1 += "\n\n";
            _screenText2 = _secondPhrase[_questionId];

            //render the text
            RenderText();
        }
    }

    /// <summary>
    /// Renders the Screen using Screen.text
    /// </summary>
    void RenderText()
    {
        screenText.text = "";
        byte searchRange;

        //proper formatting
        switch (_questionId)
        {
            //error messages should display one line
            case 68:
            case 69:
            case 70:
            case 148:
                formatText = false;
                searchRange = 18;
                screenText.fontSize = 85;
                break;

            //ultra large messages display smaller font size
            case 66:
            case 67:
            case 162:
            case 163:
                formatText = true;
                searchRange = 35;
                screenText.fontSize = 60;
                break;

            //normal display
            default:
                formatText = true;
                searchRange = 18;
                screenText.fontSize = 110;
                break;
        }

        //render first phrase
        char[] renderedText = new char[_screenText1.Length];
        renderedText = _screenText1.ToCharArray();

        for (int i = 0; i < renderedText.Length; i++)
        {
            //render the character as normal
            screenText.text += renderedText[i];
        }

        //format it into screen.Text
        ushort startPos = searchRange;

        //render second phrase
        renderedText = new char[_screenText2.Length];
        renderedText = _screenText2.ToCharArray();

        //while it isn't outside of the array
        while (startPos < renderedText.Length && formatText)
        {
            //change it to placeholder line break
            if (renderedText[startPos] == ' ')
            {
                renderedText[startPos] = '§';
                startPos += searchRange;
            }
            else
                startPos--;
        }

        for (int i = 0; i < renderedText.Length; i++)
        {
            //converting placeholder line breaks to actual line breaks
            if (renderedText[i] == '§' && formatText)
                screenText.text += "\n\n";

            //render the character as normal
            else
                screenText.text += renderedText[i];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RandomText()
    {
        screenText.text = "";

        char[] renderedText = new char[13];

        screenText.fontSize = 170;

        for (int i = 0; i < renderedText.Length; i++)
        {
            if (Random.Range(0, 2) == 0)
                renderedText[i] = _generation1[i];

            else
                renderedText[i] = _generation2[i];

            screenText.text += renderedText[i];
        }
    }

    /// <summary>
    /// Handle button presses and determine whether the answer is correct or not.
    /// </summary>
    /// <param name="num">The button that has been pushed, with the index being used as a comparsion against the answer of the module.</param>
    void HandlePress(int num)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn[num].transform);
        btn[num].AddInteractionPunch(2);

        //if the lights are off or it's solved or it's randomising, do nothing
        if (!_lightsOn || isSolved || _isRandomising)
            return;

        //update strikes
        _strikes = (short)(Info.GetStrikes() - _strikes);
        _answerOffsetId += _strikes;
        _answerOffsetId %= 4;

        if (_strikes != 0)
            Debug.LogFormat("[Placeholder Talk #{0}]: Noticed an additional strike, this means that the answer changes. The correct answer is now {1}.", _moduleId, _answerOffsetId + 1);

        //if the button pushed is correct, initiate solved module status
        if (num == _answerOffsetId)
        {
            Module.HandlePass();
            Audio.PlaySoundAtTransform(SFX.Pht.Solve, Module.transform);
            isSolved = true;

            Debug.LogFormat("[Placeholder Talk #{0}] Module Passed! The amount of times you solved this module is now {1}.", _moduleId, Info.GetSolvedModuleNames().Count(s => s == "Placeholder Talk"));

            //1 in 50 chance of getting a funny message
            if (Random.Range(0, 50) == 0)
            {
                screenText.text = "talk time :)";
                Debug.LogFormat("[Placeholder Talk #{0}] talk time :)", _moduleId);
            }
        }

        //strike condition
        else
        {
            Debug.LogFormat("[Placeholder Talk #{0}] Answer incorrect! Strike and reset! Your answer: {1}, The correct answer: {2}", _moduleId, num + 1, _answerOffsetId + 1);
            Audio.PlaySoundAtTransform(SFX.Pht.Strike, Module.transform);
            Audio.PlaySoundAtTransform(SFX.Pht.Shuffle, Module.transform);
            Module.HandleStrike();

            //generate new phrases & answers
            Init();
        }
    }

    /// <summary>
    /// Calculates the answer of the module and stores it in AnswerOffsetId.
    /// </summary>
    private short GetAnswer()
    {
        //step 1 for calculating the first variable is starting with 1
        _answerOffsetId = 1;
        Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: Start with N = {1}.", _moduleId, _answerOffsetId);

        //step 2 for calculating the first variable is adding 1 for every strike
        _strikes = (short)Info.GetStrikes();
        _answerOffsetId += _strikes;
        Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} + {2} (Strikes) = {3}.", _moduleId, _answerOffsetId - Info.GetStrikes(), Info.GetStrikes(), _answerOffsetId);

        //step 3 for calculating the first variable is adding or subtracting based on the first phrase given
        switch (_questionOffsetId)
        {
            case 0:
                _answerOffsetId++;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - (-1) = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId - 1, _answerOffsetId);
                break;

            case 1:
            case 2:
            case 3:
            case 16:
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - 0 = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId, _answerOffsetId);
                break;

            case 4:
            case 5:
            case 6:
            case 17:
                _answerOffsetId--;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - 1 = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId + 1, _answerOffsetId);
                break;

            case 7:
            case 8:
            case 9:
            case 18:
                _answerOffsetId -= 2;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - 2 = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId + 2, _answerOffsetId);
                break;

            case 10:
            case 11:
            case 12:
            case 19:
                _answerOffsetId -= 3;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - 3 = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId + 3, _answerOffsetId);
                break;

            case 13:
                _answerOffsetId -= 27;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - 27 = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId + 27, _answerOffsetId);
                break;

            case 14:
                _answerOffsetId -= 30;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - 30 = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId + 30, _answerOffsetId);
                break;

            case 15:
                _answerOffsetId += 2;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} - (-2) = {3}", _moduleId, firstPhrase[_questionOffsetId], _answerOffsetId - 2, _answerOffsetId);
                break;
        }

        switch (currentOrdinal)
        {
            case "":
                _answerOffsetId--;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + (-1) = {3}", _moduleId, currentOrdinal, _answerOffsetId + 1, _answerOffsetId);
                break;

            case "FIRST POS.":
                _answerOffsetId++;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + 1 = {3}", _moduleId, currentOrdinal, _answerOffsetId - 1, _answerOffsetId);
                break;

            case "SECOND POS.":
                _answerOffsetId += 2;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + 2 = {3}", _moduleId, currentOrdinal, _answerOffsetId - 2, _answerOffsetId);
                break;

            case "THIRD POS.":
                _answerOffsetId += 3;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + 3 = {3}", _moduleId, currentOrdinal, _answerOffsetId - 3, _answerOffsetId);
                break;

            case "FOURTH POS.":
                _answerOffsetId += 4;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + 4 = {3}", _moduleId, currentOrdinal, _answerOffsetId - 4, _answerOffsetId);
                break;

            case "FIFTH POS.":
                _answerOffsetId += 5;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + 5 = {3}", _moduleId, currentOrdinal, _answerOffsetId - 5, _answerOffsetId);
                break;

            case "MILLIONTH POS.":
            case "BILLIONTH POS.":
                _answerOffsetId += 10;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + 10 = {3}", _moduleId, currentOrdinal, _answerOffsetId - 10, _answerOffsetId);
                break;

            case "LAST POS.":
                _answerOffsetId -= 4;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + (-4) = {3}", _moduleId, currentOrdinal, _answerOffsetId + 4, _answerOffsetId);
                break;

            case "AN ANSWER":
                _answerOffsetId -= 7;
                Debug.LogFormat("[Placeholder Talk #{0}] First Phrase: {1} = {2} + (-7) = {3}", _moduleId, currentOrdinal, _answerOffsetId + 7, _answerOffsetId);
                break;
        }

        //calculate answerId (second section of manual, second variable)
        answerId = (byte)(_questionId % 4);
        Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: The phrase \"{1}\" is found under value {2}. Second Phrase: {2}.", _moduleId, _secondPhrase[_questionId].Replace("\n\n", " "), answerId + 1);

        //there's an exception where you add n for every n backslashes with phrases containing odd slashes
        //this also includes whether or not previous placeholder talks should be counted
        switch (_questionId)
        {
            //one backslash
            case 6:
            case 10:
            case 13:
            case 15:
            case 68:
            case 69:
            case 80:
            case 99:
            case 110:
            case 133:
            case 134:
                answerId++;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Odd number of slashes on second phrase, message contains 1 backslash. Add 1. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does not contain the variable N, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //two backslashes
            case 0:
            case 4:
            case 11:
            case 20:
            case 23:
            case 28:
            case 33:
            case 35:
            case 98:
            case 138:
                answerId += 2;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Odd number of slashes on second phrase, message contains 2 backslashes. Add 2. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does not contain the variable N, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //three backslashes
            case 148:
                answerId += 3;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Odd number of slashes on second phrase, message contains 3 backslashes. Add 3. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does not contain the variable N, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //four backslashes
            case 71:
                answerId += 4;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Odd number of slashes on second phrase, message contains 4 backslashes. Add 4. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does not contain the variable N, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //thirteen backslashes
            case 70:
                answerId += 13;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Odd number of slashes on second phrase, message contains 13 backslashes. Add 13. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does not contain the variable N, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //n statements (negative placeholder)
            case 66:
                previousModules = -1;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Even number of slashes on second phrase, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does contain the variable N, add -1 for every solved Placeholder Talk to second phrase. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //n statements (positive placeholder)
            case 67:
                previousModules = 1;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Even number of slashes on second phrase, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does contain the variable N, add 1 for every solved Placeholder Talk to second phrase. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //n statements (n + 0)
            case 122:
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Even number of slashes on second phrase, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does contain the variable N, add 0 to second phrase. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //n statements (n + 2)
            case 156:
                answerId += 2;
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Even number of slashes on second phrase, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does contain the variable N, add 2 to second phrase. Second Phrase: {1}", _moduleId, answerId + 1);
                break;

            //everything else
            default:
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Even number of slashes on second phrase, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                Debug.LogFormat("[Placeholder Talk #{0}] Second Phrase: Does not contain the variable N, continue without changes. Second Phrase: {1}", _moduleId, answerId + 1);
                break;
        }

        //combine answers
        _answerOffsetId += answerId;

        //include the amount of times solved if you got the special phrases about N = previous modules
        _answerOffsetId += (short)(Info.GetSolvedModuleNames().Count(s => s == "Placeholder Talk") * previousModules);

        //remodulate them twice since it can give negatives for some reason
        _answerOffsetId %= 4;
        _answerOffsetId += 4;
        _answerOffsetId %= 4;

        return _answerOffsetId;
    }

    //first phrase
    readonly string[] firstPhrase = new string[20]
    {
            "", "IS IN THE", "IS THE", "IS IN UH", "IS", "IS AT", "IS INN", "IS THE IN", "IN IS", "IS IN.", "IS IN", "THE", "FIRST-", "IN", "UH IS IN", "AT", "LAST-", "UH", "LIES", "A"
    };

    //random ordinals
    readonly string[] ordinals = new string[10]
    {
            "", "FIRST POS.", "SECOND POS.", "THIRD POS.", "FOURTH POS.", "FIFTH POS.", "MILLIONTH POS.", "BILLIONTH POS.", "LAST POS.", "AN ANSWER"
    };

    //second phrase generation
    private readonly char[] _generation1 = new char[13]
    {
            'G', 'E', 'N', 'E', 'R', 'A', 'T', 'I', 'N', 'G', '.', '.', '.'
    };

    private readonly char[] _generation2 = new char[13]
    {
            'g', 'e', 'n', 'e', 'r', 'a', 't', 'i', 'n', 'g', '.', '.', '.'
    };

    //second phrase
    private readonly string[] _secondPhrase = new string[164]
    {
            //0
            "\\ / \\",
            "BACKSLASH\n\nSLASH BACKSLASH",
            "\\ SLASH \\",
            "BACKSLASH / BACKSLASH",

            //4
            "BACKSLASH BACK / \\ \\",
            "\\ \\ \\ \\",
            "BACKSLASH BACKSLASH BACK / \\",
            "\\ \\ \\ BACKSLASH",

            //8
            "BACK \\ SLASH \\",
            "BACK / \\ BACK /",
            "BACK BACKSLASH / \\",
            "BACK \\ / \\",

            //12
            "BACK SLASH / BACK SLASH",
            "BACK SLASH BACK SLASH BACK / \\",
            "BACK SLASH SLASH BACK SLASH",
            "BACK BACK SLASH / \\",

            //16
            "I JUST LOST THE GAME",
            "\\ \\ \\ BACK SLASH",
            "SINCE WHEN DID WE HAVE A NEEDY?",
            "LITERALLY JUST A SLASH AND THEN A \\",

            //20
            "ALL OF THESE ARE WORDS: \\ / \\",
            "ALL OF THESE ARE SYMBOLS: SLASH SLASH BACKSLASH",
            "BACKSLASH SLASH SLASH, THE FIRST AND THIRD ARE SYMBOLS",
            "THE FIRST, SECOND AND THIRD ARE SYMBOLS, READY? \\ \\ / BACKSLASH",

            //24
            "WAIT, IS THIS A BACKSLASH?",
            "WAIT IS THIS A BACKSLASH?",
            "BACKSLASH BACK AND SLASH",
            "BACK SLASH BACK AND SLASH",

            //28
            "\\ SLASH / SLASH / SLASH / SLASH \\",
            "ZEEROW",
            "QUOTE BACKSLASH SLASH BACKSLASH END QUOTE SYMBOLS",
            "BACKSASH",

            //32
            "/ * / = /",
            "/ * \\ = \\",
            "\\ * \\ = \\",
            "\\ * / = \\",

            //36
            "NOTHING",
            "",
            "LITERALLY NOTHING",
            "NULL",

            //40
            "EMPTY",
            "IT'S EMPTY",
            "I CAN'T SEE ANYTHING",
            "BACKHASH",

            //44
            "READY?",
            "THE LIGHTS",
            "A VERY LONG LIST OF SLASHES",
            "A VERY LONG LIST OF SLASH",

            //48
            "/ / / / / / / / / / / / / / / / / / / / / / / /",
            "LAST DIGIT OF THE SERIAL NUMBER",
            "QUOTE SLASH END QUOTE",
            "BACK- I MEAN SLASH NOT BACKSLASH THEN A BACKSLASH",

            //52
            "BLACKHASH",
            "THE LIGHTS WENT OUT, HOLD ON",
            "THERE ARE TWENTY OR SOMETHING SLASHES",
            "THERE ARE 20 OR SOMETHING SLASHES",

            //56
            "TWO BACKSLASHES",
            "2 BACKSLASHES",
            "TO BACKSLASHES",
            "TOO BACKSLASHES",

            //60
            "TWO \\",
            "TWO \\ES",
            "THERE ARE TWO BACKSLASHES",
            "THERE'RE TWO BACKSLASHES",

            //64
            "THEIR ARE TWO BACKSLASHES",
            "THEY'RE ARE TWO BACKSLASHES",
            "ADD -N IN SECOND PHRASE WHERE N = AMOUNT OF TIMES THIS MODULE HAS BEEN SOLVED IN YOUR CURRENT BOMB",
            "ADD N IN SECOND PHRASE WHERE N = AMOUNT OF TIMES THIS MODULE HAS BEEN SOLVED IN YOUR CURRENT BOMB",

            //68
            "Parse error: syntax\n\nerror, unexpected\n\n''\\'' in /placeholderTalk/\n\nAssets/placeholderTalk.cs\n\non line 786",
            "Parse error: syntax\n\nerror, unexpected\n\n''\\'' in /placeholderTalk/\n\nManual/placeholderTalk.html\n\n on line 388",
            "/give @a command_block\n\n{Name:\"\\ \\ \\ \\ \\ \\ \\ \\ \\ \\ \\ \\ \\\"} 1",
            "/ u r a u r a \" \\ Parse Error \" u r a \" \\ Parse u r a / \" \\ Parse Error \" Error \" \\ Parse Error / \"",

            //72
            "WAIT THE ALARM WENT OFF",
            "MY GAME CRASHED",
            "BE RIGHT BACK",
            "I THOUGHT I DISABLED VANILLA MODULES",

            //76
            "WE HAVE WIRE SEQUENCES BLACK TO CHARLIE",
            "WE HAVE WIRE SEQUENCES BLACK TO C",
            "WE HAVE WIRE SEQUENCES BLACK TO SEA",
            "WE HAVE WIRE SEQUENCES BLACK TO SEE",

            //80
            "LITERALLY JUST A / AND THEN A \\",
            "ZEE ROW",
            "Z ROW",
            "THE ENTIRE ALPHABET",

            //84
            "WE HAVE TEN SECONDS",
            "ALPHA BRAVO CHARLIE AND SO ON",
            "ABCDEFGHIJKLL NOPQRSTUVWXYZ",
            "ABCDEFGHIJKLM NOPQRSTUVWXYZ",

            //88
            "THE ENTIRE ALPHABET BUT LETTERS",
            "LITERALLY THE ENTIRE ALPHABET",
            "ARE BEADY CUE DJANGO EYE FIJI",
            "THE ENTIRE ALFABET BUT LETTERS",

            //92
            "ALFA BRAVO CHARLIE DELTA ECHO FOXTROT",
            "AISLE BDELLIUM CUE DJEMBE EYE PHONEIC",
            "A B C D E F",
            "AYY BEE CEE DEE EEE EFF",

            //96
            "ABORT, WE'RE STARTING OVER",
            "I AM GONNA RESTART",
            "\\ / \\ WE HAVE ONE STRIKE",
            "BACKSLASH BACKSLASH BACK / \\ WITH ONE STRIKE",

            //100
            "YOU ARE CUTTING OUT",
            "I CANNOT HEAR YOU",
            "WAIT COMMA IS THIS A BACKSLASH?",
            "NEVERMIND ANOTHER MODULE",

            //104
            "SLAAAAAASH",
            "SLAAAAAAASH",
            "SLAAAAAAAASH",
            "SLAAAAAAAAASH",

            //108
            "OKAY I GUESSED AND IT WAS CORRECT",
            "I THINK THE MOD IS BROKEN",
            "THERE ARE 3 BATTERIES. LITERALLY JUST A / AND THEN A \\",
            "DOES THE MANUAL SAY ANYTHING ABOUT A SECOND STAGE ?",
            
            //112
            "WHAT IS YOUR LEAST FAVORITE MODULE?",
            "THIS MASSAGE IS REALLY HARD TO COMMUNICATE",
            "THIS MESSAGE IS REALLY HARD TO COMMUNICATE",
            "THE ANSWER IS IN THE UH SECOND POS.",

            //116
            "ALL WORDS THE NUMBER ZERO",
            "THE NUMBER ZERO",
            "THE NUMBER 0",
            "THE NUMBER 0 AS IN DIGIT",

            //120
            "0",
            "ZERO",
            "N + 0",
            "WAIT HOW MANY BATTERIES DO WE HAVE",

            //124
            "0 BATTERIES",
            "TIME RAN OUT",
            "AND KABOOM",
            "HUH?",

            //128
            "SOME CHINESE CHARACTERS",
            "頁 - 設 - 是 - 煵",
            "THE TEXT DOESN'T FIT",
            "AAAAAAAAAA",

            //132
            "FORWARD SLASH",
            "/(o w o)\\",
            "/(u w u)\\",
            "BACKWARD SLASH",

            //136
            "I HAVE TEN SECONDS",
            "THE SECOND PHRASE IS QUOTE BACKSLASH SLASH BACKSLASH END QUOTE",
            "IT SAYS ALL SYMBOLS BACK / \\ \\ BACKSLASH",
            "THE SECOND PHRASE IS QUOTE BACKSLASH SLASH BACKSLASH UNQUOTE",

            //140
            "WAIT IT CHANGED",
            "ALPHA BRAVO CHARLIE DELTA ECHO FOXTROT",
            "WAIT COMMA IS THIS A BACKSLASH",
            "YOU JUST LOST THE GAME",

            //144
            "WAIT COMMA IS THIS A BACKSLASH QUESTION MARK",
            "ALL WORDS WAIT COMMA IS THIS A BACKSLASH QUESTION MARK",
            "배 - 탓 - 배 - 몸",
            "え - み - さ - ん",

            //148
            "Error:\n\nMissingComponentException\n\n(Could not find \"/screenFont\"\n\nin F:\\placeholderTalk\\\n\nAssets\\Materials)",
            "IT'S THE SAME AS BEFORE",
            "THIS MODULE HAS BEEN SPONSORED BY RAID SHADOW LEGENDS",
            "OH WE BLUE UP AS IN THE COLOR",

            //152
            "OH WE BLUE UP",
            "THIS MODULE HAS BEEN SPONSORED",
            "IT'S THE SAME ONE",
            "OH WE BLEW UP",

            //156
            "N + 2",
            "OH WE BLEW UP AS IN THE COLOR",
            "PRESS 1 IF >2 BATTERIES, ELSE 2",
            "PARSE ERROR",

            //160
            "WAIT, IS THIS A BACKSLASH",
            "WAIT COMMA IS THIS A BACK SLASH",
            "hello guys welcome back to another minecraft video and in todays video we will be talking about my brand new enderman holding a bacon statue, its furious, its hot and its powerful guys. its the definition of engineering at its finest, now lets enter from the rear of the building.",
            "o m g guys we found a creeper in the downstairs bathroom lemme get my diamond hoe from the inventory and shit i just died. thank you so much for watching and have a great rest of your day, make sure to like, comment and subscribe and eat that bell icon like its enderman bacon\n\nFOOL"

    };

    /// <summary>
    /// Determines whether the input from the TwitchPlays chat command is valid or not.
    /// </summary>
    /// <param name="par">The string from the user.</param>
    private bool IsValid(string par)
    {
        string[] validNumbers = { "1", "2", "3", "4" };

        if (validNumbers.Contains(par))
            return true;

        return false;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <#> (Presses the button labeled '#' | valid numbers are from 1-4)";
#pragma warning restore 414

    /// <summary>
    /// TwitchPlays Compatibility, detects every chat message and clicks buttons accordingly.
    /// </summary>
    /// <param name="command">The twitch command made by the user.</param>
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] buttonPressed = command.Split(' ');

        //if command is formatted correctly
        if (Regex.IsMatch(buttonPressed[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            //if command has no parameters
            if (buttonPressed.Length < 2)
                yield return "sendtochaterror Please specify the button you want to press! (Valid: 1-4)";

            //if command has too many parameters
            else if (buttonPressed.Length > 2)
                yield return "sendtochaterror Too many buttons pressed! Only one can be pressed at any time.";

            //if command has an invalid parameter
            else if (!IsValid(buttonPressed.ElementAt(1)))
                yield return "sendtochaterror Invalid number! Only buttons 1-4 can be pushed.";

            //if command is valid, push button accordingly
            else
            {
                int s;
                int.TryParse(buttonPressed[1], out s);
                btn[s - 1].OnInteract();
            }
        }
    }

    /// <summary>
    /// Force the module to be solved in TwitchPlays
    /// </summary>
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        btn[(_answerOffsetId + Info.GetStrikes() - _strikes) % 4].OnInteract();
    }
}