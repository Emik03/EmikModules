using PalindromesModule;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Palindromes : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public MeshRenderer Component;
    public KMSelectable[] Buttons;
    public TextMesh[] Text;

    bool isSolved = false;
    string x = "", y = "", z = "", n = "";

    private bool _isAnimating;
    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private List<string> _exampleSolution;

    private void Awake()
    {
        _moduleId = _moduleIdCounter++;

        //puts in correct index when you push one of the three buttons
        for (byte i = 0; i < Buttons.Length; i++)
        {
            byte j = i;

            Buttons[i].OnInteract += delegate ()
            {
                HandlePress(j);
                return false;
            };
        }

        //makes sure that it doesn't generate a palindrome to prevent very rare unicorns
        while (true)
        {
            //generates a 9-digit number for the screen display
            Text[0].text = Random.Range(10000000, 1000000000).ToString();

            //anti-palindrome check
            for (byte i = 0; i < Text[0].text.Length; i++)
                if (Text[0].text[i] != Text[0].text[Text[0].text.Length - i - 1])
                    goto generated;
        }

    //go here when the module has generated a valid number
    generated:
        while (Text[0].text.Length < 9)
            Text[0].text = Text[0].text.Insert(0, "0");

        //logging
        Debug.LogFormat("[Palindromes #{0}]: Screen > {1}.", _moduleId, Text[0].text);
        _exampleSolution = PalindromesSolver.Get(Text[0].text, _moduleId);
    }

    private void HandlePress(byte btn)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[btn].transform);
        Buttons[btn].AddInteractionPunch();

        //if solved, do nothing
        if (isSolved || _isAnimating)
            return;

        string[] vs = new string[3] { x, y, z };

        switch (btn)
        {
            //cycle number
            case 0:
                Audio.PlaySoundAtTransform(Sounds.Pld.Cycle, Buttons[btn].transform);
                Text[2].text = ((byte.Parse(Text[2].text) + 1) % 10).ToString();
                break;

            //submit number
            case 1:
                Audio.PlaySoundAtTransform(Sounds.Pld.Submit, Buttons[btn].transform);
                for (byte i = 0; i < vs.Length; i++)
                {
                    //if current variable hasn't been filled yet
                    if (vs[i].Length < 9 - i)
                    {
                        vs[i] += Text[2].text;
                        Text[2].text = "0";
                        //if left half has been inputted, generate palindrome
                        if (vs[i].Length == 5 - System.Convert.ToByte(i != 0))
                        {
                            for (sbyte j = (sbyte)(3 - System.Convert.ToByte(i == 2)); j >= 0; j--)
                                vs[i] += vs[i][j];

                            goto render;
                        }
                        break;
                    }
                }
                break;

            //delete number
            case 2:
                Audio.PlaySoundAtTransform(Sounds.Pld.Delete, Buttons[btn].transform);
                for (sbyte i = (sbyte)(vs.Length - 1); i >= 0; i--)
                {
                    if (vs[i].Length != 0)
                    {
                        vs[i] = "";
                        break;
                    }
                }
                Text[2].text = "0";
                break;

            //panic (twitch plays exclusive)
            default: x = "000000000"; y = "00000000"; z = "0000000"; break;
        }

    render:
        bool isSubmitting = true;
        for (byte i = 0; i < vs.Length; i++)
        {
            switch (i)
            {
                case 0: Text[1].text = "X"; break;
                case 1: Text[1].text += "\nY"; break;
                case 2: Text[1].text += "\nZ"; break;
            }

            Text[1].text += "  =  " + vs[i];

            if (isSubmitting && vs[i].Length < 9 - i)
            {
                Text[1].text += Text[2].text;
                isSubmitting = false;
            }
        }

        x = vs[0];
        y = vs[1];
        z = vs[2];

        //if everything has been filled, check if the answer is correct
        if (isSubmitting)
            StartCoroutine(CheckAnswer(int.Parse(x) + int.Parse(y) + int.Parse(z) != int.Parse(Text[0].text) && btn < 3));
    }

    private IEnumerator CheckAnswer(bool strike)
    {
        _isAnimating = true;

        Audio.PlaySoundAtTransform(Sounds.Pld.Calculate, Buttons[1].transform);
        Debug.LogFormat("[Palindromes #{0}]: Submitting > {1}.", _moduleId, new string[3] { x, y, z }.Join(" & "));

        n = Text[0].text;
        int temp = int.Parse(Text[0].text), total = int.Parse(x) + int.Parse(y) + int.Parse(z), inc = 0;

        while (inc < 10000)
        {
            inc += 125;
            float f = inc;
            f /= 10000;
            Text[0].text = Mathf.Clamp(temp - total * OutBack(f), -999999999, 999999999).ToString("#########") + "";
            yield return new WaitForSeconds(0.02f);
        }

        //strike
        if (strike)
        {
            Audio.PlaySoundAtTransform(Sounds.Pld.Answer, Buttons[1].transform);
            Debug.LogFormat("[Palindromes #{0}]: Strike! > {1}", _moduleId, temp - total);
            Module.HandleStrike();

            Text[1].text = "";
            string error = "ERROR:\nX+Y+Z does\nnot equal N!";

            for (byte i = 0; i < error.Length; i++)
            {
                Text[1].text += error[i];
                yield return new WaitForSeconds(0.02f);
            }

            float f = 0;
            while (f < 1)
            {
                for (int i = 0; i < Text.Length; i++)
                    Text[i].color = new Color32((byte)(Text[i].color.r * 255), (byte)(Text[i].color.g * 255), (byte)(Text[i].color.b * 255), (byte)((1 - Easing.OutCubic(f, 0, 1, 1)) * 255));
                yield return new WaitForSeconds(0.02f);
                f += 0.0125f;
            }

            x = "";
            y = "";
            z = "";

            Text[0].text = temp.ToString("#########");
            Text[1].text = "X  =  " + Text[2].text + "\nY  =  \nZ  =  ";

            f = 0;
            while (f < 1)
            {
                for (int i = 0; i < Text.Length; i++)
                    Text[i].color = new Color32((byte)(Text[i].color.r * 255), (byte)(Text[i].color.g * 255), (byte)(Text[i].color.b * 255), (byte)(Easing.OutCubic(f, 0, 1, 1) * 255));
                yield return new WaitForSeconds(0.01f);
                f += 0.0125f;
            }
        }

        //solve
        else
        {
            Audio.PlaySoundAtTransform(Sounds.Pld.Answer, Buttons[1].transform);
            Text[0].text = "0";
            Text[1].text = "YOU  FOUND  IT!";
            Debug.LogFormat("[Palindromes #{0}]: Solved! > 0", _moduleId);
            isSolved = true;
            Module.HandlePass();
        }

        _isAnimating = false;
    }

    private static float OutBack(float k)
    {
        float s = 1.70158f;
        return (k -= 1f) * k * ((s + 1f) * k + s) + 1f;
    }

    private bool IsValid(string par)
    {
        //palindrome check
        for (byte i = 0; i < par.Length; i++)
            if (par[i] != par[par.Length - i - 1])
                return false;

        //positive number check
        uint num;
        return uint.TryParse(par, out num);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit <#########> <########> <#######> (Submits the numbers on the module. All numbers must be palindromic. Example: !{0} submit 420696024 13377331 0000000)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] par = command.Split(' ');

        //if command is formatted correctly
        if (Regex.IsMatch(par[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            //if three numbers haven't been submitted
            if (par.Length < 4)
                yield return "sendtochaterror Please specify the three numbers you wish to submit.";

            //if more than three numbers have been submitted
            else if (par.Length > 4)
                yield return "sendtochaterror Too many numbers submitted! Please submit exactly 3 numbers.";

            //if any numbers aren't the correct digit length
            else if (par[1].Length != 9 || par[2].Length != 8 || par[3].Length != 7)
                yield return "sendtochaterror The numbers have to be 9 digits, 8 digits, then 7 digits, in that order.";

            //if any input aren't numbers, or aren't palindromes
            else if (!IsValid(par[1]) || !IsValid(par[2]) || !IsValid(par[3]))
                yield return "sendtochaterror The numbers have to be palindromes! (Written the same forwards as backwards.)";

            else
            {
                //deletes everything in case if anything was inputted
                while (x.Length != 0)
                {
                    Buttons[2].OnInteract();
                    yield return new WaitForSeconds(0.15f);
                }

                //execute the instructions provided for each character in each string
                for (byte i = 1; i <= 3; i++)
                    for (byte j = 0; j < 5; j++)
                    {
                        //the only 5-digit number that needs to be inputted is X
                        if (i != 1 && j == 4)
                            continue;

                        //if the current button isn't equal to what the user submitted, press the left button to cycle through the numbers until it's false
                        while (Text[2].text[0] != par[i][j])
                        {
                            Buttons[0].OnInteract();
                            yield return new WaitForSeconds(0.05f);
                        }

                        //press the middle button which submits the digit
                        Buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.15f);
                    }
                //since the module has a solve animation, i'm required to submit early whether the module solves or not
                if (int.Parse(par[1]) + int.Parse(par[2]) + int.Parse(par[3]) != int.Parse(n))
                    yield return "strike";

                else
                    yield return "solve";
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        //autosolve
        yield return null;
        Debug.LogFormat("[Palindromes #{0}]: AutoSolver > Initiating.", _moduleId);

        //if the algorithm failed, solve instantly
        if (_exampleSolution.Count == 0)
            HandlePress(3);

        //if the algorithm succeeds, proceed to submit the numbers generated
        else
        {
            //execute the instructions provided for each character in each string
            for (byte i = 1; i <= 3; i++)
                for (byte j = 0; j < 5; j++)
                {
                    //the only 5-digit number that needs to be inputted is X
                    if (i != 1 && j == 4)
                        continue;

                    //if the current button is equal to what the user submitted, press the left button to cycle through the numbers until it's false
                    while (Text[2].text[0] != _exampleSolution[i][j])
                    {
                        Buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.05f);
                    }

                    //press the middle button which submits the digit
                    Buttons[1].OnInteract();
                    yield return new WaitForSeconds(0.15f);
                }

            while (!isSolved)
                yield return true;
        }
    }
}