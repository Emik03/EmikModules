using HexOSModule;
using PathManager = KeepCoding.PathManager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Video;
using Rnd = UnityEngine.Random;
using KeepCoding;

public class HexOS : MonoBehaviour
{
    #region Fields
    public class ModSettingsJSON
    {
        [JsonProperty("hexOS -> DisableOctOS")]
        public bool DisableOctOS { get; set; }
        [JsonProperty("hexOS -> ForceOctOS")]
        public bool ForceOctOS { get; set; }
        [JsonProperty("hexOS -> FastStrike")]
        public bool FastStrike { get; set; }
        [JsonProperty("hexOS -> ExperimentalShake")]
        public bool ExperimentalShake { get; set; }
        [JsonProperty("hexOS -> ForceAltSolve")]
        public bool ForceAltSolve { get; set; }
        [JsonProperty("hexOS -> FlashOtherColors")]
        public byte FlashOtherColors { get; set; }
        [JsonProperty("hexOS -> DelayPerBeat")]
        public float DelayPerBeat { get; set; }
        [JsonProperty("hexOS -> CustomSolveQuote")]
        public string CustomSolveQuote { get; set; }
    }

    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMModSettings ModSettings;
    public KMSelectable Button;
    public Renderer Bezel;
    public MeshRenderer[] Ciphers, Cylinders;
    public Renderer Background, Foreground, VideoRenderer;
    public TextMesh Number, UserNumber, Status, Quote, ModelName, GroupCounter;
    public Texture[] FrequencyTextures;
    public Transform[] Spinnables;
    public VideoPlayer VideoOct, VideoGrid;
    public VideoClip[] Clips;

#pragma warning disable 414
    bool isSolved = false, solvedInOctOS = false;
#pragma warning restore 414
    char[] decipher = new char[2];
    string sum = "", screen = "";

    private Color32 _hexBezel = new Color32(127, 100, 92, 255), _octBezel = new Color32(42, 33, 30, 255);
    private System.Random _rnd;
    private static bool _forceAltSolve, _experimentalShake, _canBeOctOS;
    private bool _lightsOn, _octOS, _isHolding, _playSequence, _hasPlayedSequence, _octAnimating, _fastStrike;
    private static byte _flashOtherColors = 5;
    private sbyte _press = -1, _held = 0;
    private readonly char[] _tempDecipher = new char[2];
    private readonly byte[] _rhythms = new byte[2], _ciphers = new byte[6], _octRhythms = new byte[2], _octSymbols = new byte[18];
    private readonly List<byte> _octColors = new List<byte>(0);
    private static int _moduleIdCounter = 1, _y = 0, _rotationSpeed;
    private int _moduleId = 0;
    private static float _delayPerBeat, _hexOSStrikes;
    private static string _customSolveQuote;
    private string _user = "", _answer = "", _octAnswer = "", _submit = "", _tempScreen, _tempSum;
    #endregion

    #region Initalization
    /// <summary>
    /// ModuleID and JSON Initialisation.
    /// </summary>
    private void Start()
    {
        // Give each module of hexOS a different number.
        _moduleId = _moduleIdCounter++;

        SFX.LogVersionNumber(Module, _moduleId);

        // Set the variables in case if they don't get set by ModSettings.
        _canBeOctOS = true;
        _fastStrike = false;
        _experimentalShake = false;
        _forceAltSolve = false;
        _flashOtherColors = 5;
        _delayPerBeat = 0.07f;
        _customSolveQuote = "";
        _hexOSStrikes = 0;

        if (LoadMission())
            GetModSetting();
        
        // Hides ciphers
        for (byte i = 0; i < Ciphers.Length; i++)
            Ciphers[i].transform.localPosition = new Vector3(Ciphers[i].transform.localPosition.x, -2.1f, Ciphers[i].transform.localPosition.z);

        // Hide toilet if force alt solve is on, otherwise hide stars.
        _rotationSpeed = (byte)((Convert.ToByte(_forceAltSolve) * 9) + 1);
        byte hideIndex = (byte)(1 - Convert.ToByte(_forceAltSolve));
        for (byte i = (byte)(2 * hideIndex); i < 2 + hideIndex; i++)
            Spinnables[i].transform.localPosition = new Vector3(Spinnables[i].transform.localPosition.x, Spinnables[i].transform.localPosition.y / 3, Spinnables[i].transform.localPosition.z / 2);

        int seed = Rnd.Range(0, int.MaxValue);
        _rnd = new System.Random(seed);
        // SET YOUR SEED HERE IN CASE OF BUGS!!
        // _rnd = new System.Random(1);
        Debug.LogFormat("[hexOS #{0}] Entering dimension no. {1}x{2}!", _moduleId, HexOSStrings.Version, seed);

        // Start module.
        if (!Application.isEditor)
        {
            var enumerator = PathManager.LoadVideoClips(GetType(), "hex");

            while (enumerator.MoveNext())
                if (enumerator.Current.GetType() == typeof(VideoClip[]))
                    Clips = (VideoClip[])enumerator.Current;
        }
        Activate();

        if (_octOS)
            OctGenerate();
    }

    /// <summary>
    /// Button initialisation.
    /// </summary>
    private void Awake()
    {
        // Press.
        Button.OnInteract += delegate ()
        {
            HandlePress();
            return false;
        };

        // Release.
        Button.OnInteractEnded += delegate ()
        {
            HandleRelease();
        };
    }

    /// <summary>
    /// Button hold handler.
    /// </summary>
    private void FixedUpdate()
    {
        float offset = Time.time * 0.01f;
        Foreground.material.mainTextureOffset = new Vector2(offset, -offset);

        // Rotates it as long as the module isn't solved.
        for (byte i = 0; i < Spinnables.Length; i++)
            Spinnables[i].localRotation = Quaternion.Euler(86 * Convert.ToByte(i != 2), _y += _rotationSpeed * Convert.ToSByte(!isSolved) * ((2 * Convert.ToSByte(_canBeOctOS)) - 1), 0);

        // Changes color back.
        for (byte i = 0; i < Cylinders.Length; i++)
        {
            Cylinders[i].material.color = new Color32(
                (byte)((Cylinders[i].material.color.r * 255) - (Convert.ToByte(Cylinders[i].material.color.r * 255 > 85) * 2)),
                (byte)((Cylinders[i].material.color.g * 255) - (Convert.ToByte(Cylinders[i].material.color.g * 255 > 85) * 2)),
                (byte)((Cylinders[i].material.color.b * 255) - (Convert.ToByte(Cylinders[i].material.color.b * 255 > 85) * 2)), 255);
        }

        // Increment the amount of frames of the user holding the button.
        if (_lightsOn && !isSolved && _isHolding)
            _held++;

        // Indicates that it is ready.
        Number.color = HexOSStrings.PerfectColors[1 + Convert.ToByte(_held >= 25)];

        if (_held == 25)
        {
            Audio.PlaySoundAtTransform(SFX.Hex.Ready, Module.transform);
            Status.text = "Boot Manager\nStoring " + _submit + "...";
        }

        // Autoreset
        else if (_held == 125)
        {
            Audio.PlaySoundAtTransform(SFX.Hex.Cancel, Module.transform);
            Status.text = "Boot Manager\nCancelling...";
            _isHolding = false;
            _held = -1;
        }
    }

    private void GetModSetting()
    {
        // Get JSON settings.
        try
        {
            // Get settings.
            ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(ModSettings.Settings);

            // If it contains information.
            if (settings != null)
            {
                // Get variables from mod settings.
                _canBeOctOS = !settings.DisableOctOS;
                _octOS = settings.ForceOctOS;
                _fastStrike = settings.FastStrike;
                _experimentalShake = settings.ExperimentalShake;
                _forceAltSolve = settings.ForceAltSolve;
                _flashOtherColors = Math.Min(settings.FlashOtherColors, (byte)6);
                _delayPerBeat = Math.Min(Math.Abs(settings.DelayPerBeat), 1);
                _customSolveQuote = settings.CustomSolveQuote;
            }
        }
        catch (JsonReaderException e)
        {
            // In the case of catastrophic failure and devastation.
            Debug.LogFormat("[hexOS #{0}] JSON reading failed with error: \"{1}\", resorting to default values.", _moduleId, e.Message);
        }
    }

    private bool LoadMission()
    {
        string description = Application.isEditor ? "" : Game.Mission.Description;

        if (description == null)
            return true;

        Regex regex = new Regex(@"\[hexOS\] (\d+,){1}\d+");

        var match = regex.Match(description);

        if (!match.Success)
            return true;

        string[] vs = match.Value.Replace("[hexOS] ", "").Split(',');

        if (vs.Length != 8)
            return true;

        int[] values = vs.Skip(2).Take(6).ToArray().ToNumbers(minLength: 6, maxLength: 6);

        if (values == null)
            return true;

        if (values.Take(5).Any(i => !i.InRange(0, 1)))
            return true;

        if (!float.TryParse(vs[1], out _delayPerBeat))
            return true;

        _customSolveQuote = vs.First();
        _canBeOctOS = values[0] == 0;
        _octOS = values[1] == 1;
        _fastStrike = values[2] == 1;
        _experimentalShake = values[3] == 1;
        _forceAltSolve = values[4] == 1;
        _flashOtherColors = Math.Min((byte)values[5], (byte)6);
        return false;
    }

    /// <summary>
    /// Lights get turned on.
    /// </summary>
    private void Activate()
    {
        // Plays the foreground video as decoration.
        VideoGrid.clip = Clips[0];
        VideoGrid.Prepare();
        VideoGrid.Play();

        // Reset the textures just in case
        Background.material.SetColor("_Color", HexOSStrings.TransparentColors[2]);
        Foreground.material.SetColor("_Color", Color.blue);

        // Get the correct answer.
        _answer = HexGenerate();

        // Add leading 0's.
        while (_answer.Length < 3)
            _answer = "0" + _answer;

        Debug.LogFormat("[hexOS #{0}]: The expected answer is {1}.", _moduleId, _answer);
        Status.text = "Boot Manager\nWaiting...";
        _lightsOn = true;
    }

    /// <summary>
    /// Button interaction, and handling of the cycled chords/sequences.
    /// </summary>
    private void HandlePress()
    {
        // Sounds and punch effect.
        Audio.PlaySoundAtTransform(SFX.Hex.Click, Module.transform);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);

        Button.AddInteractionPunch(2.5f);

        // Lights off, solved then it should end it here.
        if (!_lightsOn || isSolved || _octAnimating)
            return;

        // Is now holding button.
        _isHolding = true;

        // Store the button press so that it wouldn't matter how long you hold on to the button.
        if (!_playSequence)
            _submit = Number.text;
    }

    /// <summary>
    /// Button interaction, and handling of the action depending on how long the button was held.
    /// </summary>
    private void HandleRelease()
    {
        // Is no longer holding button.
        _isHolding = false;

        // Lights off, solved, or playing sequence should end it here.
        if (!_lightsOn || isSolved || _octAnimating)
            return;

        if (!_playSequence)
            Status.text = "Boot Manager\nWaiting...";

        // If the button was held for less than 25 frames (0.5 seconds), then play the sequence.
        if (_held < 20)
        {
            // If the input was cancelled, don't play the sequence.
            if (_held < 0)
                return;

            // Reset holding.
            _held = 0;

            // If the sequence isn't already playing, play it.
            if (!_playSequence)
            {
                // Increment presses so that the correct chords and sequences are played.
                _press = (sbyte)((_press + 1) % 8);

                if (!_octOS)
                    StartCoroutine(HexPlaySequence());
                else
                    StartCoroutine(OctPlaySequence());
            }
        }

        // Otherwise, submit the answer that displayed when the button was pushed.
        else
        {
            Audio.PlaySoundAtTransform(SFX.Hex.Submit, Module.transform);

            // Reset holding.
            _held = 0;

            // Add digit to user input if the number exists, otherwise clear it.
            _user = _submit != "   " ? _user + _submit[_user.Length] : "";

            // If the user input has 3 inputs, check for answer.
            if (_user.Length == 3)
            {
                // Color each cylinder depending on which corresponding digit is correct.
                for (byte i = 0; i < Cylinders.Length; i++)
                {
                    if (_user == _octAnswer && _octOS)
                        Cylinders[i].material.color = new Color32(222, 222, 222, 255);

                    else if (_user[i] == _answer[i] && !(_user == "888" && _answer != "888"))
                        Cylinders[i].material.color = new Color32(51, 222, 51, 255);

                    else
                        Cylinders[i].material.color = new Color32(255, 51, 51, 255);
                }

                // User matched the expected answer, solve.
                if (_user == _answer && !_octOS)
                {
                    // This solves the module.
                    StartCoroutine(HexSolve());
                }
                else if (_user == _octAnswer && _octOS)
                {
                    // This solves the module in hard mode.
                    StartCoroutine(OctSolve());
                }

                // If the user activates hard mode.
                else if (_user == "888" && !_hasPlayedSequence && !_octOS && _canBeOctOS)
                {
                    // Generate new answer.
                    _octAnswer = OctGenerate();
                    Debug.LogFormat("[hexOS #{0}]: The expected answer for the current octOS is {1}.", _moduleId, _octAnswer);
                }

                // Otherwise, strike and reset the user input.
                else
                {
                    Debug.LogFormat("[hexOS #{0}]: The number submitted ({1}) did not match the expected answer ({2}), that's a strike!", _moduleId, _user, _octOS ? _octAnswer : _answer);
                    _user = "";

                    if (!_octOS)
                    {
                        Audio.PlaySoundAtTransform(SFX.Hex.Strike, Module.transform);
                        Status.text = "Boot Manager\nError!";

                        // Caps at 1, 20+ are treated the same as exactly 20 strikes
                        _hasPlayedSequence = false;
                        _hexOSStrikes = Math.Min(++_hexOSStrikes, 20);

                        Module.HandleStrike();
                    }
                    else
                        StartCoroutine(OctStrike());
                }
            }

            UserNumber.text = _user;

            while (UserNumber.text.Length != 3)
                UserNumber.text += '-';
        }
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Solves the module when run. This stops ALL coroutines.
    /// </summary>
    private IEnumerator HexSolve()
    {
        // Typical module handle pass.
        Background.material.SetColor("_Color", HexOSStrings.TransparentColors[1]);
        Foreground.material.SetColor("_Color", Color.green);
        Button.AddInteractionPunch(50);
        isSolved = true;
        Status.text = "Boot Manager\nUnlocked!";
        Debug.LogFormat("[hexOS #{0}]: The correct number was submitted, module solved!", _moduleId);
        Module.HandlePass();

        // If forceAltSolve is enabled, pick a joke message.
        if (_forceAltSolve)
        {
            Audio.PlaySoundAtTransform(SFX.Hex.SolveAlt, Module.transform);
            Quote.text = HexOSStrings.AltSolvePhrases[Rnd.Range(0, HexOSStrings.AltSolvePhrases.Length)];
        }

        // Otherwise pick a regular message.
        else
        {
            Audio.PlaySoundAtTransform(SFX.Hex.Solve, Module.transform);
            Quote.text = HexOSStrings.SolvePhrases[Rnd.Range(0, HexOSStrings.SolvePhrases.Length)];
        }

        // If custom quote has been filled, render it.
        if (_customSolveQuote != "")
            Quote.text = _customSolveQuote;

        // Shuffles through a bunch of random numbers.
        for (byte i = 0; i < 20; i++)
        {
            Number.text = Rnd.Range(100, 1000).ToString();
            yield return new WaitForSecondsRealtime(0.05f);
        }

        // Stops everything.
        Number.text = "---";

        // Goes through 3-255 and stops after overflow.
        for (byte i = 3; i > 2; i += 2)
        {
            Quote.color = new Color32(i, i, i, 255);
            yield return new WaitForSecondsRealtime(0.02f);
        }

        StopAllCoroutines();
    }

    /// <summary>
    /// Solves the module when run in hard mode. This stops ALL coroutines.
    /// </summary>
    private IEnumerator OctSolve()
    {
        _octAnimating = true;
        solvedInOctOS = true;
        yield return new WaitForEndOfFrame();

        // Sets the background and foreground to be white in case if the video animation is slightly delayed.
        Background.material.SetColor("_Color", HexOSStrings.TransparentColors[3]);
        Foreground.material.SetColor("_Color", Color.white);

        // Resets all strings.
        UserNumber.text = "";
        Number.text = "";
        Status.text = "";

        // Gives powerful emphasis.
        Button.AddInteractionPunch(100);

        // Plays the solve animation.
        VideoOct.transform.localPosition = new Vector3(0, 0.84f, 0);
        VideoRenderer.material.color = new Color32(255, 255, 255, 255);
        VideoOct.clip = Clips[1];
        VideoOct.Prepare();
        VideoOct.Play();

        Audio.PlaySoundAtTransform(SFX.Hex.OctSolve, Module.transform);

        // The exact amount of seconds for the audio clip to go quiet is 10.122 seconds.
        yield return new WaitForSecondsRealtime(10.122f);

        Debug.LogFormat("[hexOS #{0}]: The correct number for octOS was submitted, module solved! +36 additional points!", _moduleId);
        isSolved = true;
        Module.HandlePass();

        StopAllCoroutines();
    }

    /// <summary>
    /// Strikes the module in an animation for hard mode.
    /// </summary>
    private IEnumerator OctStrike()
    {
        _octAnimating = true;
        _playSequence = false;
        yield return new WaitForEndOfFrame();

        // Turn back to black.
        for (byte i = 0; i < Ciphers.Length; i++)
        {
            Ciphers[i].material.mainTexture = null;
            Ciphers[i].material.color = Color.white;
            Ciphers[i].material.color = new Color32(0, 0, 0, 255);
            Ciphers[i].transform.localPosition = new Vector3(Ciphers[i].transform.localPosition.x, -2.1f, Ciphers[i].transform.localPosition.z);
        }

        // Resets all strings.
        UserNumber.text = "";
        Number.text = "";
        Status.text = "";

        // Sets the background and foreground to be white in case if the video animation is slightly delayed.
        Background.material.SetColor("_Color", HexOSStrings.TransparentColors[3]);
        Foreground.material.SetColor("_Color", Color.white);

        VideoOct.transform.localPosition = new Vector3(0, 0.84f, 0);
        VideoRenderer.material.color = new Color32(255, 255, 255, 0);

        // Long animation.
        if (!_fastStrike)
        {
            VideoOct.clip = Clips[2];
            VideoOct.Prepare();
            VideoOct.Play();
            Audio.PlaySoundAtTransform(SFX.Hex.OctStrike, Module.transform);

            byte c = 248;
            VideoRenderer.material.color = new Color32(255, 255, 255, c);

            while (c > 128)
            {
                c -= 20;
                VideoRenderer.material.color = new Color32(255, 255, 255, c);
                yield return new WaitForSecondsRealtime(0.1f);
            }

            while (c != 252)
            {
                c += 4;
                VideoRenderer.material.color = new Color32(255, 255, 255, c);
                yield return new WaitForSecondsRealtime(1.484375f);
            }

            yield return new WaitWhile(() => VideoOct.isPlaying);
        }

        // Short animation.
        else
        {
            VideoOct.clip = Clips[3];
            VideoOct.Prepare();
            VideoOct.Play();
            Audio.PlaySoundAtTransform(SFX.Hex.OctStrikeFast, Module.transform);
            // For reference, the audio clip is 11.85 seconds.
            byte c = 0;
            while (c != 255)
            {
                c++;
                VideoRenderer.material.color = new Color32(255, 255, 255, c);
                yield return new WaitForSecondsRealtime(0.04f);
            }
            yield return new WaitWhile(() => VideoOct.isPlaying);
        }

        VideoOct.transform.localPosition = new Vector3(0, -0.42f, 0);

        // Reset back to hexOS, restoring all the values.
        _octOS = false;
        Bezel.material.color = _hexBezel;
        ModelName.text = "hexOS";
        ModelName.color = new Color32(0, 0, 0, 255);
        screen = _tempScreen;
        sum = _tempSum;
        UserNumber.text = "---";
        Status.text = "Boot Manager\nWaiting...";

        decipher = new char[2];
        for (int i = 0; i < decipher.Length; i++)
            _tempDecipher[i] = decipher[i];

        Background.material.SetColor("_Color", HexOSStrings.TransparentColors[2]);
        Foreground.material.SetColor("_Color", Color.blue);

        // Start it up again.
        Debug.LogFormat("[hexOS #{0}]: octOS Struck! Reverting back to hexOS...", _moduleId);
        _octAnimating = false;
        StartCoroutine(UpdateScreen());
        Module.HandleStrike();
    }

    /// <summary>
    /// Updates the screen every second to cycle all digits.
    /// </summary>
    private IEnumerator UpdateScreen()
    {
        byte index = 0;

        // While not solved, cycle through 30 digit number.
        while (!isSolved)
        {
            // Stop routine if octOS is currently playing a video.
            if (_octAnimating)
                yield break;

            // If in last index, put a pause and restart loop.
            if (index >= screen.Length)
            {
                index = 0;
                Number.text = "   ";
            }

            // Otherwise, display next 3 digits.
            else
                Number.text = screen[index++].ToString() + screen[index++].ToString() + screen[index++].ToString();

            // Display lag.
            yield return new WaitForSecondsRealtime(1f + (_hexOSStrikes / 20) - (Convert.ToSingle(_octOS) / 1.5f));
        }
    }

    /// <summary>
    /// Play the sequence of notes and flashes on the module.
    /// </summary>
    private IEnumerator HexPlaySequence()
    {
        // The harder version can be activated only when the sequence hasn't been played yet.
        _hasPlayedSequence = true;

        // Prevent button presses from playing the sequence when it's already being played.
        _playSequence = true;

        byte[,] seqs = new byte[3, 19];

        // Establish colors to be displayed for each tile, 0 = black, 1 = white, 2 = magenta.
        for (byte i = 0; i < _flashOtherColors - _hexOSStrikes; i++)
            // For each color.
            for (byte j = 0; j < 3; j++)
                // For each sequence variable.
                for (byte k = 0; k < seqs.GetLength(0); k++)
                    seqs[k, (i * 3) + j] = j;

        // Fill in remaining slots.
        for (byte i = (byte)Math.Max(3 * (_flashOtherColors - _hexOSStrikes), 0); i < seqs.GetLength(1); i++)
            // For each sequence variable.
            for (byte j = 0; j < seqs.GetLength(0); j++)
                seqs[j, i] = _ciphers[(_press % 2 * 3) + j];

        if (Status.text != "Boot Manager\nSaving " + _submit + "...")
            Status.text = "Boot Manager\nPlaying...";

        // Show cipher squares.
        for (byte i = 0; i < Ciphers.Length; i++)
            Ciphers[i].transform.localPosition = new Vector3(Ciphers[i].transform.localPosition.x, 0.21f, Ciphers[i].transform.localPosition.z);

        // Cache results for operations in use for later calculations.
        float delay = Math.Min(_delayPerBeat + (_hexOSStrikes / 20), 1);
        bool hihat = delay > 0.2f;

        // Display the colors at the same time as the rhythms, which have different timings.
        StartCoroutine(HexDisplayColors(seqs));

        // If true, this returns a coroutine in a fixed speed which may cause a bit of lag.
        // This is necessary because the hihat plays every tick.
        if (hihat)
            for (byte i = 0; i < HexOSStrings.Notes[_press].Length; i++)
            {
                // At least 2 strikes, start playing hi-hat.
                Audio.PlaySoundAtTransform(SFX.Hex.HiHat, Module.transform);

                // Look through the sequence of rhythms, if a note should be playing, play note.
                if (HexOSStrings.Notes[_rhythms[_press % 2]][i] == 'X')
                {
                    Audio.PlaySoundAtTransform(SFX.Hex.Chord(_press + 1), Module.transform);
                    if (_experimentalShake)
                        Button.AddInteractionPunch(0.5f);
                }

                yield return new WaitForSecondsRealtime(delay);
            }

        // If false, this returns a coroutine based on a linear search which makes delays more accurate.
        // This is necessary because Unity isn't accurate with small amounts of WaitForSeconds().
        else
            for (byte i = 0; i < HexOSStrings.Notes[_rhythms[_press % 2]].Length - 1; i += 0)
            {
                // Play note.
                Audio.PlaySoundAtTransform(SFX.Hex.Chord(_press + 1), Module.transform);
                if (_experimentalShake)
                    Button.AddInteractionPunch(0.5f);

                // Temporarily store the variable 'i'.
                byte temp = i;

                // Calculate ahead the delay between the next note.
                for (i++; i < HexOSStrings.Notes[_rhythms[_press % 2]].Length - 1; i++)
                    if (HexOSStrings.Notes[_rhythms[_press % 2]][i] == 'X')
                        break;

                yield return new WaitForSecondsRealtime((i - temp) * delay);
            }

        // Play one last note, with emphasis on percussion.
        Audio.PlaySoundAtTransform(SFX.Hex.Chord(_press + 1), Module.transform);
        Audio.PlaySoundAtTransform(SFX.Hex.Clap, Module.transform);
        if (_experimentalShake)
            Button.AddInteractionPunch(5);

        if (Status.text != "Boot Manager\nStoring " + _submit + "...")
            Status.text = "Boot Manager\nLoading...";

        // Hide ciphers.
        for (byte j = 0; j < Ciphers.Length; j++)
        {
            Ciphers[j].material.color = new Color32(0, 0, 0, 255);
            Ciphers[j].transform.localPosition = new Vector3(Ciphers[j].transform.localPosition.x, -2.1f, Ciphers[j].transform.localPosition.z);
        }

        yield return new WaitForSecondsRealtime(Math.Min((_delayPerBeat * 12) + (_hexOSStrikes / 20), 1));

        if (Status.text != "Boot Manager\nStoring " + _submit + "...")
            Status.text = "Boot Manager\nWaiting...";

        // Allow button presses.
        _playSequence = false;
    }

    /// <summary>
    /// Play the hard mode's sequence of notes and flashes on the module.
    /// </summary>
    private IEnumerator OctPlaySequence()
    {
        // Prevent button presses from playing the sequence when it's already being played.
        _playSequence = true;

        if (Status.text != "Boot Manager\nSaving " + _submit + "...")
            Status.text = "Boot Manager\nPlaying...";

        // Show ciphers.
        for (byte i = 0; i < Ciphers.Length; i++)
            Ciphers[i].transform.localPosition = new Vector3(Ciphers[i].transform.localPosition.x, 0.21f, Ciphers[i].transform.localPosition.z);

        byte[,] seqs = new byte[18, 9];

        // Array initializer.
        for (byte i = 0; i < seqs.GetLength(0); i++)
        {
            // Fills in distracting lights.
            for (byte j = 0; j < 6; j++)
                seqs[i, j] = (byte)(j / 2 * 12);

            // ULTRA-CRUEL VARIANT (literally unplayable garbage, don't use this)
            // Fills in 1 with incorrect color.
            //seq[i, 7] = (byte)(12 * (_octColors[i] + 1) % 3);

            // Fills remainder with "true" colors.
            for (byte j = 6; j < seqs.GetLength(1); j++)
                seqs[i, j] = (byte)(12 * _octColors[i]);
        }

        // Display the colors at the same time as the rhythms, which have different timings.
        StartCoroutine(OctDisplayColors(seqs));

        for (byte i = 0; i < HexOSStrings.OctNotes[_octRhythms[_press % 2]].Length - 1; i += 0)
        {
            // Stop routine if octOS is currently playing a video.
            if (_octAnimating)
                yield break;

            Audio.PlaySoundAtTransform(SFX.Hex.Chord(_press + 9), Module.transform);
            if (_experimentalShake)
                Button.AddInteractionPunch(.5f);

            // Temporarily store the variable 'i'.
            byte temp = i;

            // Calculate ahead the delay between the next note.
            for (i++; i < HexOSStrings.OctNotes[_octRhythms[_press % 2]].Length - 1; i++)
                if (HexOSStrings.OctNotes[_octRhythms[_press % 2]][i] == 'X')
                    break;

            // 60 / 1140 (190bpm * 6beat)
            yield return new WaitForSecondsRealtime((i - temp) * 0.0526315789474f);
        }

        // Play one last note, with emphasis on percussion.
        Audio.PlaySoundAtTransform(SFX.Hex.Chord(_press + 9), Module.transform);
        Audio.PlaySoundAtTransform(SFX.Hex.Clap, Module.transform);
        if (_experimentalShake)
            Button.AddInteractionPunch(5);

        if (Status.text != "Boot Manager\nStoring " + _submit + "...")
            Status.text = "Boot Manager\nLoading...";

        // Turn back to black.
        for (byte j = 0; j < Ciphers.Length; j++)
        {
            Ciphers[j].material.color = new Color32(0, 0, 0, 255);
            Ciphers[j].transform.localPosition = new Vector3(Ciphers[j].transform.localPosition.x, -2.1f, Ciphers[j].transform.localPosition.z);
        }

        // (60 / 1140) * 12 (190bpm * 6beat * 12beat)
        //yield return new WaitForSecondsRealtime(0.63157894736f);

        if (Status.text != "Boot Manager\nStoring " + _submit + "...")
            Status.text = "Boot Manager\nWaiting...";

        // Allow button presses.
        _playSequence = false;
    }

    private IEnumerator HexDisplayColors(byte[,] seqs)
    {
        // Shuffle it for ambiguity.
        Shuffle(seqs);

        // Reset textures from octOS.
        for (byte j = 0; j < Ciphers.Length; j++)
            Ciphers[j].material.mainTexture = null;

        for (byte i = 0; i < HexOSStrings.Notes[_press].Length; i += 2)
        {
            // Render color, but only half as often as the rhythms.
            for (byte j = 0; j < Ciphers.Length; j++)
                Ciphers[j].material.color = HexOSStrings.PerfectColors[seqs[j, i / 2]];

            yield return new WaitForSecondsRealtime(Math.Min(_delayPerBeat + (_hexOSStrikes / 20), 1) * 2);
        }
    }

    private IEnumerator OctDisplayColors(byte[,] seqs)
    {
        // Shuffle it for ambiguity.
        Shuffle(seqs);

        // Reset colors from hexOS.
        for (byte j = 0; j < Ciphers.Length; j++)
            Ciphers[j].material.color = Color.white;

        const float delay = 0.0526315789474f * 2;

        for (byte i = 0; i < HexOSStrings.OctNotes[_press].Length; i += 2)
        {
            // Stop routine if octOS is currently playing a video.
            if (_octAnimating)
                yield break;

            // Create the amount of dots corresponding to which group it is cycling through.
            GroupCounter.text = "";
            for (byte k = 0; k <= i / 17; k++)
                GroupCounter.text += _press % 2 == 0 ? '.' : ':';

            // Render texture, but only half as often as the rhythms.
            for (byte j = 0; j < Ciphers.Length; j++)
                Ciphers[j].material.mainTexture = FrequencyTextures[seqs[j + (i / 17 * 3) + (_press % 2 * 9), i % 17 / 2] + _octSymbols[j + (i / 17 * 3) + (_press % 2 * 9)]];

            // 60 / 1140 (190bpm * 6beat)
            yield return new WaitForSecondsRealtime(delay);
        }

        GroupCounter.text = "";
    }
    #endregion

    #region Module Generation
    /// <summary>
    /// Generates an answer. This should only be run once at the beginning of the module.
    /// </summary>
    private string HexGenerate()
    {
        // Generate random rhythm indexes, making sure that neither are the same.
        _rhythms[0] = (byte)_rnd.Next(0, HexOSStrings.Notes.Length);
        do _rhythms[1] = (byte)_rnd.Next(0, HexOSStrings.Notes.Length);
        while (_rhythms[1] == _rhythms[0]);

        Debug.LogFormat("[hexOS #{0}]: The first rhythm sequence is {1}.", _moduleId, HexOSStrings.Notes[_rhythms[0]]);
        Debug.LogFormat("[hexOS #{0}]: The second rhythm sequence is {1}.", _moduleId, HexOSStrings.Notes[_rhythms[1]]);

        // Generate random ciphers.
        for (byte i = 0; i < _ciphers.Length; i++)
            _ciphers[i] = (byte)_rnd.Next(0, 3);

        string[] logColor = { "Black", "White", "Magenta" };
        Debug.LogFormat("[hexOS #{0}]: Perfect Cipher is {1}, {2}, {3}, and {4}, {5}, {6}.", _moduleId, logColor[_ciphers[0]], logColor[_ciphers[1]], logColor[_ciphers[2]], logColor[_ciphers[3]], logColor[_ciphers[4]], logColor[_ciphers[5]]);

        // Generate numbers 0-9 for each significant digit.
        byte[,] temp = new byte[3, 10]
        {
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
        };

        // Shuffles each array.
        Shuffle(temp);

        // Add it to the screen variable so that it's ready to be displayed.
        for (byte i = 0; i < 10; i++)
            for (byte j = 0; j < 3; j++)
                screen += temp[j, i];

        // Stores the current screen display in case if it gets replaced by octOS and needs to be reverted.
        _tempScreen = screen;
        _tempSum = sum;

        // Thumbnail.
        //screen = "420420420420420420420420420420";

        Debug.LogFormat("[hexOS #{0}]: The screen displays the number {1}.", _moduleId, screen);
        StartCoroutine(UpdateScreen());

        // Converts indexes to binary which is how it is shown in the manual.
        string[] rhythmLog = { Convert.ToString(_rhythms[0], 2), Convert.ToString(_rhythms[1], 2) };
        for (byte i = 0; i < rhythmLog.Length; i++)
            while (rhythmLog[i].Length < 4)
                rhythmLog[i] = "0" + rhythmLog[i];

        Debug.LogFormat("[hexOS #{0}]: The rhythm sequences translate to {1} and {2}.", _moduleId, Convert.ToString(_rhythms[0], 2), Convert.ToString(_rhythms[1], 2));

        // Creates the sum, ensuring that it stays 4 bits long.
        sum = (short.Parse(Convert.ToString(_rhythms[0], 2)) + short.Parse(Convert.ToString(_rhythms[1], 2))).ToString();
        while (sum.Length < 4)
            sum = "0" + sum;

        Debug.LogFormat("[hexOS #{0}]: The sum of the rhythm sequence is {1}.", _moduleId, sum);

        byte[] encipher = new byte[2];

        // Gets index for Perfect Cipher
        for (byte i = 0; i < encipher.Length; i++)
        {
            byte j = (byte)(i * 3);
            encipher[i] = (byte)(_ciphers[j] + (_ciphers[j + 1] * 3) + (_ciphers[j + 2] * 9));
        }

        // Gets value from Perfect Cipher Dictionary
        for (byte i = 0; i < encipher.Length; i++)
        {
            HexOSStrings.PerfectCipher.TryGetValue(encipher[i], out decipher[i]);
            _tempDecipher[i] = decipher[i];
        }

        Debug.LogFormat("[hexOS #{0}]: Perfect Cipher decrypts to {1} and {2}.", _moduleId, decipher[0], decipher[1]);

        byte n = 0;
        byte[] logicA = new byte[6], logicB = new byte[6];

        // Creates pairs.
        for (byte l = 0; l < 4; l++)
            for (byte r = (byte)(l + 1); r < 4; r++)
            {
                logicA[n] = byte.Parse(sum[l].ToString());
                logicB[n] = byte.Parse(sum[r].ToString());
                n++;
            }

        Dictionary<char, string> logicGateNames = new Dictionary<char, string>(27) { { ' ', "SUM" }, { 'A', "AND" }, { 'B', "NAND" }, { 'C', "XAND" }, { 'D', "COMPARISON" }, { 'E', "A=1 THEN B" }, { 'F', "SUM" }, { 'G', "EQUALITY" }, { 'H', "OR" }, { 'I', "NOR" }, { 'J', "XOR" }, { 'K', "GULLIBILITY" }, { 'L', "NA THEN NB" }, { 'M', "IMPLIES" }, { 'N', "IMPLIES" }, { 'O', "NA THEN NB" }, { 'P', "GULLIBILITY" }, { 'Q', "XOR" }, { 'R', "NOR" }, { 'S', "OR" }, { 'T', "EQUALITY" }, { 'U', "SUM" }, { 'V', "A=1 THEN B" }, { 'W', "COMPARISON" }, { 'X', "XAND" }, { 'Y', "NAND" }, { 'Z', "AND" } };
        Debug.LogFormat("[hexOS #{0}]: The pairs to use in logic gates {1} and {2} are {3}{4}, {5}{6}, {7}{8}, {9}{10}, {11}{12}, {13}{14}.", _moduleId, logicGateNames[decipher[0]], logicGateNames[decipher[1]], logicA[0], logicB[0], logicA[1], logicB[1], logicA[2], logicB[2], logicA[3], logicB[3], logicA[4], logicB[4], logicA[5], logicB[5]);

        sbyte[] logicOutput = new sbyte[12];

        // Logic gates.
        for (byte i = 0; i < logicA.Length; i++)
            for (byte j = 0; j < decipher.Length; j++)
            {
                switch (decipher[j])
                {
                    case 'A': // AND
                    case 'Z':
                        logicOutput[(i * 2) + j] = (sbyte)(Math.Min(logicA[i], logicB[i]) - 1);
                        break;

                    case 'B': // NAND
                    case 'Y':
                        logicOutput[(i * 2) + j] = (sbyte)(2 - Math.Min(logicA[i], logicB[i]) - 1);
                        break;

                    case 'C': // XAND
                    case 'X':
                        logicOutput[(i * 2) + j] = (sbyte)(Mathf.Clamp(logicA[i] + logicB[i], 0, 1) + Convert.ToByte(logicA[i] + logicB[i] == 4) - 1);
                        break;

                    case 'D': // COMPARISON
                    case 'W':
                        logicOutput[(i * 2) + j] = (sbyte)(Convert.ToByte(logicA[i] > logicB[i]) + Convert.ToByte(logicA[i] >= logicB[i]) - 1);
                        break;

                    case 'E': // A=1 THEN B
                    case 'V':
                        if (logicA[i] == logicB[i])
                            logicOutput[(i * 2) + j] = (sbyte)(logicA[i] - 1);
                        else if (logicB[i] != 1)
                            logicOutput[(i * 2) + j] = (sbyte)(logicB[i] - 1);
                        else
                            logicOutput[(i * 2) + j] = (sbyte)(logicA[i] - 1);
                        break;

                    case 'F': // SUM
                    case 'U':
                    case ' ':
                        logicOutput[(i * 2) + j] = (sbyte)(((logicA[i] + logicB[i] + 2) % 3) - 1);
                        break;

                    case 'G': // EQUALITY
                    case 'T':
                        logicOutput[(i * 2) + j] = (sbyte)((2 * Convert.ToByte(logicA[i] == logicB[i])) - 1);
                        break;

                    case 'H': // OR
                    case 'S':
                        logicOutput[(i * 2) + j] = (sbyte)(Math.Max(logicA[i], logicB[i]) - 1);
                        break;

                    case 'I': // NOR
                    case 'R':
                        logicOutput[(i * 2) + j] = (sbyte)((2 - Math.Max(logicA[i], logicB[i])) - 1);
                        break;

                    case 'J': // XOR
                    case 'Q':
                        if (logicA[i] == 1 || logicB[i] == 1)
                            logicOutput[(i * 2) + j] = 0;
                        else if (logicA[i] == logicB[i])
                            logicOutput[(i * 2) + j] = 1;
                        else
                            logicOutput[(i * 2) + j] = -1;
                        break;

                    case 'K': // GULLIBILITY
                    case 'P':
                        if (logicA[i] + logicB[i] == 2)
                            logicOutput[(i * 2) + j] = 0;
                        else if (logicA[i] + logicB[i] > 2)
                            logicOutput[(i * 2) + j] = 1;
                        else
                            logicOutput[(i * 2) + j] = -1;
                        break;

                    case 'L': // NA THEN NB
                    case 'O':
                        if (logicA[i] == 1)
                            logicOutput[(i * 2) + j] = 0;
                        else if (logicA[i] == logicB[i] || logicA[i] + logicB[i] == 3)
                            logicOutput[(i * 2) + j] = 1;
                        else
                            logicOutput[(i * 2) + j] = -1;
                        break;

                    case 'M': // IMPLIES
                    case 'N':
                        logicOutput[(i * 2) + j] = (sbyte)(Mathf.Clamp(4 - (logicA[i] + logicB[i]), 0, 2) - 1);
                        break;
                }
            }

        // Creates offset.
        sbyte offset = 0;
        for (byte i = 0; i < logicOutput.Length; i++)
            offset += logicOutput[i];

        // Calculates the digital root with the offset.
        string newScreen = "";
        for (byte i = 0; i < screen.Length; i++)
            newScreen += ((byte.Parse(screen[i].ToString()) + Math.Abs(offset) - 1) % 9) + 1;

        Debug.LogFormat("[hexOS #{0}]: The output from each logic computation is {1}", _moduleId, logicOutput.Join(", "));
        Debug.LogFormat("[hexOS #{0}]: Combining all of them gives the offset {1}.", _moduleId, offset);
        Debug.LogFormat("[hexOS #{0}]: The modified screen display is {1}.", _moduleId, newScreen);

        // Run the algorithm to compress the 30-digit number into 3, then returning it.
        return (short.Parse(HexThreeDigit(newScreen)) % 1000).ToString();
    }

    /// <summary>
    /// Generates an answer for hard mode. This should only be run once when activated.
    /// </summary>
    private string OctGenerate()
    {
        Debug.LogFormat("[hexOS #{0}]: octOS has been activated! Regenerating module...", _moduleId);
        Bezel.material.color = _octBezel;
        ModelName.text = "octOS";
        ModelName.color = new Color32(222, 222, 222, 255);

        Status.text = "Boot Manager\n...?";
        Audio.PlaySoundAtTransform(SFX.Hex.OctActivate, Module.transform);

        _octOS = true;
        _user = "";

        Background.material.SetColor("_Color", HexOSStrings.TransparentColors[0]);
        Foreground.material.SetColor("_Color", Color.red);

        // Generate random rhythm indexes, making sure that neither are the same.
        _octRhythms[0] = (byte)_rnd.Next(0, HexOSStrings.OctNotes.Length);
        do _octRhythms[1] = (byte)_rnd.Next(0, HexOSStrings.OctNotes.Length);
        while (_octRhythms[1] == _octRhythms[0]);

        // Converts sum from decimal to base 4.
        sum = ConvertBase((Convert.ToInt16(_octRhythms[0]) * 16) + _octRhythms[1], new char[] { '0', '1', '2', '3' });
        while (sum.Length < 4)
            sum = '0' + sum;

        sbyte[] bitSum = new sbyte[4];
        for (byte i = 0; i < bitSum.Length; i++)
            bitSum[i] = (sbyte)char.GetNumericValue(sum[i]);

        Debug.LogFormat("[hexOS #{0}]: The first rhythm sequence is {1}.", _moduleId, HexOSStrings.OctNotes[_octRhythms[0]]);
        Debug.LogFormat("[hexOS #{0}]: The second rhythm sequence is {1}.", _moduleId, HexOSStrings.OctNotes[_octRhythms[1]]);
        Debug.LogFormat("[hexOS #{0}]: The 4-bit sum is {1}.", _moduleId, sum);

        // Generate random key from a piece of a phrase.
        string key = HexOSStrings.OctPhrases[_rnd.Next(0, HexOSStrings.OctPhrases.Length)];
        key = key.Remove(0, _rnd.Next(0, key.Length - 6));

        while (key.Length > 6)
            key = key.Substring(0, key.Length - 1);

        // Generate random symbols.
        for (byte i = 0; i < _octSymbols.Length; i++)
            _octSymbols[i] = (byte)_rnd.Next(0, 12);

        char[] encipheredKey = key.ToCharArray();
        decipher = key.ToCharArray();

        // Enciphers each letter with the symbols.
        for (byte i = 0; i < encipheredKey.Length; i++)
        {
            byte index = 0;
            for (byte j = 0; j < HexOSStrings.Alphabet.Length; j++)
                if (HexOSStrings.Alphabet[j] == encipheredKey[i])
                {
                    index = j;
                    break;
                }

            encipheredKey[i] = HexOSStrings.Alphabet[(index - HexOSStrings.Symbols[_octSymbols[i * 3]] - HexOSStrings.Symbols[_octSymbols[(i * 3) + 1]] - HexOSStrings.Symbols[_octSymbols[(i * 3) + 2]] + 27) % 27];
        }

        _octColors.Clear();

        // Enciphers each letter into colors.
        for (byte i = 0; i < encipheredKey.Length; i++)
        {
            byte index = HexOSStrings.PerfectCipher.FirstOrDefault(x => x.Value == encipheredKey[i].ToString().ToUpper().ToCharArray()[0]).Key;
            string colors = ConvertBase(index, new char[] { '0', '1', '2' });

            while (colors.Length < 3)
                colors = '0' + colors;

            foreach (char color in colors.Reverse())
                _octColors.Add((byte)char.GetNumericValue(color));
        }

        List<string> log = new List<string>(0);
        for (int i = 0; i < _octSymbols.Length; i++)
            log.Add(HexOSStrings.Symbols[_octSymbols[i]].ToString());

        byte keyIndex = 0;

        // Finds the first instance of the phrase.
        for (byte i = 0; i < HexOSStrings.OctPhrases.Length && keyIndex == 0; i++)
            for (byte j = 0; j < HexOSStrings.OctPhrases[i].Length - 5 && keyIndex == 0; j++)
            {
                string comparison = HexOSStrings.OctPhrases[i].Substring(j, 6);

                if (key == comparison)
                    keyIndex = (byte)(i + 1);
            }

        Debug.LogFormat("[hexOS #{0}]: The colors decipher the phrase \"{1}\".", _moduleId, encipheredKey.Join(""));
        Debug.LogFormat("[hexOS #{0}]: The symbols' values are {1}.", _moduleId, log.Join(", "));
        Debug.LogFormat("[hexOS #{0}]: The deciphered letters are \"{1}\".", _moduleId, key);
        Debug.LogFormat("[hexOS #{0}]: The value obtained from the key is \"{1}\".", _moduleId, keyIndex);

        // Generate numbers 0-9 for each significant digit.
        byte[,] temp = new byte[3, 10]
        {
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
        };

        // Shuffles each array.
        Shuffle(temp);

        // Add it to the screen variable so that it's ready to be displayed.
        screen = "";
        for (byte i = 0; i < 10; i++)
            for (byte j = 0; j < 3; j++)
                screen += temp[j, i];

        // Testing various numbers on octOS. !PANIC
        //screen = "874243369655992010486528737101";
        //bitSum = new sbyte[4] { 2, 0, 1, 3 };

        Debug.LogFormat("[hexOS #{0}]: The screen displays the number {1}.", _moduleId, screen);

        string alpha = "", beta = "", gamma = "", delta = "";

        // Half of the screen string is alpha, and the other half is beta.
        for (int i = 0; i < screen.Length - 5; i += 6)
        {
            alpha += screen[i].ToString() + screen[i + 1].ToString() + screen[i + 2].ToString();
            beta += screen[i + 3].ToString() + screen[i + 4].ToString() + screen[i + 5].ToString();
        }

        Debug.LogFormat("[hexOS #{0}]: α = {1}.", _moduleId, alpha);
        Debug.LogFormat("[hexOS #{0}]: β = {1}.", _moduleId, beta);

        // Half of gamma is alpha, the other half is beta, with exceptions.
        for (int i = 0; i < alpha.Length; i++)
        {
            // Special PRODUCT case
            if (i > 0 && beta[i] == beta[i - 1])
                gamma += '*';

            // Special SUM case
            else if (i > 0 && alpha[i] == alpha[i - 1])
                gamma += '+';

            // Special EQUALITY case
            else if (Math.Abs(char.GetNumericValue(alpha[i]) - char.GetNumericValue(beta[i])) == 5)
                gamma += '=';

            // Add ALPHA
            else if (i % 2 == 0)
                gamma += alpha[i];

            // Add BETA
            else
                gamma += beta[i];

            // Special IMPLIES case, has to be checked last.
            if (i > 0 && gamma[i] == gamma[i - 1])
                gamma = gamma.Remove(gamma.Length - 1, 1) + '>';
        }

        Debug.LogFormat("[hexOS #{0}]: γ = {1}.", _moduleId, gamma);

        // Checks if NOT gate should be used.
        bool notGate = false;
        if (bitSum[0] == bitSum[2] || bitSum[1] == bitSum[3])
        {
            notGate = true;
            for (byte i = 0; i < bitSum.Length; i++)
                bitSum[i] = (sbyte)(4 - bitSum[i]);
        }

        string sumLog = "";

        // Logic gates.
        for (byte i = 0; i < gamma.Length; i++)
        {
            byte operand = 0;
            switch (gamma[i])
            {
                case '0': operand = (byte)Math.Min(bitSum[0], bitSum[1]); break; // AND

                case '1': operand = (byte)Math.Max(bitSum[0], bitSum[1]); break; // OR

                case '2': operand = (byte)(4 - Math.Min(bitSum[0], bitSum[1])); break; // NAND

                case '3': operand = (byte)(4 - Math.Max(bitSum[0], bitSum[1])); break; // NOR

                case '4':
                    operand = bitSum[0] <= 1 && bitSum[1] <= 1 ? (byte)Math.Max(bitSum[0], bitSum[1]) : // XAND
                              bitSum[0] >= 3 && bitSum[1] >= 3 ? (byte)Math.Min(bitSum[0], bitSum[1]) : (byte)2; break;

                case '5':
                    operand = bitSum[0] <= 1 && bitSum[1] <= 1 ? (byte)Math.Max(bitSum[0], bitSum[1]) : // XOR
                              bitSum[0] >= 3 && bitSum[1] >= 3 ? (byte)(4 - Math.Min(bitSum[0], bitSum[1])) :
                              bitSum[0] <= 1 && bitSum[1] >= 3 ? (byte)Math.Min(4 - bitSum[0], bitSum[1]) :
                              bitSum[0] >= 3 && bitSum[1] <= 1 ? (byte)Math.Min(bitSum[0], 4 - bitSum[1]) : (byte)2; break;

                case '6': operand = (byte)Mathf.Clamp(bitSum[0] - bitSum[1] + 2, 0, 4); break; // COMPARISON

                case '7': operand = (byte)Mathf.Clamp(bitSum[0] + bitSum[1] - 2, 0, 4); break; // GULLIBILITY

                case '8': operand = (bitSum[0] % 2 == 1 && bitSum[1] == 2) || (bitSum[0] % 4 == 0 && bitSum[1] % 4 != 0) ? (byte)bitSum[0] : (byte)bitSum[1]; break; // A=2 THEN B

                case '9':
                    operand = bitSum[0] % 4 == 0 && bitSum[1] % 4 == 0 && bitSum[0] == bitSum[1] ? (byte)4 : // NA THEN NB
                              (bitSum[0] == 1 && bitSum[1] <= 1) || (bitSum[0] == 3 && bitSum[1] >= 3) ? (byte)3 :
                              bitSum[0] == 2 ? (byte)2 :
                              (bitSum[0] == 1 && bitSum[1] >= 2) || (bitSum[0] == 3 && bitSum[1] <= 2) ? (byte)1 : (byte)0; break;

                case '+': operand = (byte)((bitSum[0] + bitSum[1]) % 5); break; // SUM

                case '*': operand = (byte)(bitSum[0] * bitSum[1] % 5); break; // PRODUCT

                case '>': operand = bitSum[0] == 3 ? (byte)((bitSum[1] % 4) + 1) : bitSum[0] == 4 && bitSum[1] == 4 ? (byte)4 : (byte)(4 - (bitSum[0] * Convert.ToByte(bitSum[1] % Mathf.Pow(2, bitSum[0]) < Mathf.Pow(2, bitSum[0]) / 2))); break; // IMPLIES

                case '=': // EQUALITY
                    operand = (bitSum[0] == 4 || bitSum[1] == 4) && bitSum[0] != bitSum[1] ? (byte)0 : (byte)4;
                    if (operand == 4)
                    {
                        if (bitSum[0] % 2 != bitSum[1] % 2)
                            operand -= 1;
                        if (bitSum[0] / 2 != bitSum[1] / 2)
                            operand -= 2;
                    }
                    break;
            }

            for (byte j = 0; j < bitSum.Length - 1; j++) // Shift left
                bitSum[j] = bitSum[j + 1];

            if (bitSum[1] == operand) // If second and fourth operand are the same, find earliest unique number.
            {
                if (notGate) // Searches forward for smallest unique number.
                    for (sbyte j = 0; j <= 4; j++)
                        for (byte k = 0; k < bitSum.Length; k++)
                        {
                            if (bitSum[k] == j)
                                break;
                            if (k == bitSum.Length - 1)
                            {
                                operand = (byte)j;
                                goto foundNumber;
                            }
                        }

                else // Searches backwards for biggest unique number.
                    for (sbyte l = 4; l >= 0; l--)
                        for (byte m = 0; m < bitSum.Length; m++)
                        {
                            if (bitSum[m] == l)
                                break;
                            if (m == bitSum.Length - 1)
                            {
                                operand = (byte)l;
                                goto foundNumber;
                            }
                        }
            }

        // If it has found a number to override the operand with, this is where it will end up going.
        foundNumber:

            bitSum[3] = (sbyte)operand;

            // Logs the 4-bit sum.
            sumLog += bitSum.Join("");
            if (i != gamma.Length - 1)
                sumLog += ", ";

            // Delta is equal to the operand from gamma's logic gate applied to alpha and beta.
            switch (operand)
            {
                case 0: delta += char.GetNumericValue(alpha[i]) * char.GetNumericValue(beta[i]) % 10; break;
                case 1: delta += Math.Abs(char.GetNumericValue(alpha[i]) - char.GetNumericValue(beta[i])); break;
                case 2: delta += (char.GetNumericValue(alpha[i]) + char.GetNumericValue(beta[i])) % 10; break;
                case 3: delta += Math.Min(char.GetNumericValue(alpha[i]), char.GetNumericValue(beta[i])); break;
                case 4: delta += Math.Max(char.GetNumericValue(alpha[i]), char.GetNumericValue(beta[i])); break;
            }
        }

        Debug.LogFormat("[hexOS #{0}]: 4-bits after applying operands = {1}.", _moduleId, sumLog);
        Debug.LogFormat("[hexOS #{0}]: δ = {1}.", _moduleId, delta);

        // Calculates the digital root.
        string screenWithKeyIndex = "", newScreen = "";
        for (byte i = 0; i < delta.Length; i++)
            screenWithKeyIndex += ((byte.Parse(delta[i].ToString()) + keyIndex - 1) % 9) + 1;

        Debug.LogFormat("[hexOS #{0}]: Applying offset {1} to δ = {2}.", _moduleId, keyIndex, screenWithKeyIndex);

        // Combine one-third with two-thirds.
        for (byte i = 0; i < screenWithKeyIndex.Length; i += 3)
        {
            newScreen += (char.GetNumericValue(screenWithKeyIndex[i]) + char.GetNumericValue(screenWithKeyIndex[i + 2])) % 10;
            newScreen += screenWithKeyIndex[i + 1];
        }

        Debug.LogFormat("[hexOS #{0}]: Adding left 2 digits with right digit: {1}.", _moduleId, newScreen);

        // Return the result of compressing a 15-digit number to a 3.
        return OctThreeDigit(newScreen);
    }

    /// <summary>
    /// An algorithm that takes a 30-digit number and compresses it to a 3- or 4-digit number to return as the answer of the module.
    /// </summary>
    /// <param name="seq">The sequence of digits that will be used.</param>
    private string HexThreeDigit(string seq)
    {
        Debug.LogFormat("[hexOS #{0}]: Current sequence > {1}", _moduleId, seq);

        // Create groups of 6.
        List<int> digits = new List<int>(0);
        for (byte i = 5; i < seq.Length; i += 6)
            digits.Add(int.Parse(string.Concat(seq[i - 5], seq[i - 4], seq[i - 3], seq[i - 2], seq[i - 1], seq[i])));

        Debug.LogFormat("[hexOS #{0}]: Forming groups > {1}", _moduleId, digits.Join(", "));
        seq = "";

        // Add groups of 6 with each other.
        for (byte i = 0; i < digits.Count; i++)
            seq += (digits[i] / 1000 + digits[i] % 1000).ToString();

        Debug.LogFormat("[hexOS #{0}]: Combining the groups > {1}", _moduleId, seq);

        // Get leftovers.
        string leftover = "";
        for (byte i = (byte)(Math.Floor(seq.Length / 6f) * 6); i < seq.Length && i != 0; i++)
            leftover += seq[i];

        string newSeq = "";

        if (leftover.Length != 0)
        {
            // Add leftovers to sequence with digital root.
            for (byte i = 0; i < (Math.Floor(seq.Length / 6f) * 6); i++)
                newSeq += ((((byte)(byte.Parse(seq[i].ToString()) + byte.Parse(leftover[i % leftover.Length].ToString()))) - 1) % 9) + 1;

            Debug.LogFormat("[hexOS #{0}]: Leftovers > {1}", _moduleId, leftover);
            Debug.LogFormat("[hexOS #{0}]: Modified sequence > {1}", _moduleId, newSeq);
        }
        else
        {
            Debug.LogFormat("[hexOS #{0}]: No leftovers. Continue as normal.", _moduleId);
            newSeq = seq;
        }

        // Repeat if equal or more than 6 digits long.
        if (newSeq.Length >= 6)
        {
            Debug.LogFormat("[hexOS #{0}]: Sequence is not less than 6 digits long. Repeat this process.", _moduleId);
            newSeq = HexThreeDigit(newSeq);
        }

        // Remove any additional digits.
        while (newSeq.Length > 3)
            newSeq = newSeq.Substring(1, newSeq.Length - 1);

        // Once you reach here, you have a 3-digit number!
        return newSeq;
    }

    /// <summary>
    /// An algorithm that takes a 30-digit number and compresses it to a 3-digit number to return as the answer of the module.
    /// </summary>
    /// <param name="seq">The sequence of digits that will be used.</param>
    private string OctThreeDigit(string seq)
    {
        string newSeq = "";
        Debug.LogFormat("[hexOS #{0}]: Sequence > {1}", _moduleId, seq);

        // Determine the last digit of the sequence
        switch (seq[seq.Length - 1])
        {
            case '0': // Divide by 10.
                newSeq = seq.Length > 4 ? seq.Substring(0, seq.Length - 2) + '8' : seq.Substring(0, seq.Length - 1);
                break;

            case '1': // Subtract digits with the last.
            case '4':
                for (byte i = 0; i < seq.Length; i++)
                    newSeq += ((char.GetNumericValue(seq[i]) - char.GetNumericValue(seq[seq.Length - 1]) + 10) % 10).ToString();
                break;

            case '2': // Add or subtract prime digits.
            case '5':
            case '7':
                for (byte i = 0; i < seq.Length; i++)
                    switch (i)
                    {
                        case 1:
                        case 2:
                        case 4:
                        case 6:
                            newSeq += ((char.GetNumericValue(seq[i]) + char.GetNumericValue(seq[seq.Length - 1])) % 10).ToString();
                            break;

                        default:
                            newSeq += ((char.GetNumericValue(seq[i]) - char.GetNumericValue(seq[seq.Length - 1]) + 10) % 10).ToString();
                            break;
                    }
                break;

            case '3': // Replace with 0 or 7.
            case '6':
            case '9':
                for (int i = 0; i < seq.Length; i++)
                {
                    if (seq[i] == '3' || seq[i] == '6' || seq[i] == '9')
                        newSeq += (i + 1) % 3 == 0 ? '0' : '7';

                    else
                        newSeq += seq[i];
                }
                break;

            case '8': // Add neighbouring numbers.
                for (byte i = 0; i < seq.Length; i++)
                {
                    byte temp = 0;
                    temp += (byte)char.GetNumericValue(seq[i]);
                    if (i != 0)
                        temp += (byte)char.GetNumericValue(seq[i - 1]);
                    if (i != seq.Length - 1)
                        temp += (byte)char.GetNumericValue(seq[i + 1]);
                    newSeq += (temp % 10).ToString();
                }
                break;
        }

        // Rerun the algorithm if the sequence isn't 3 digits long, otherwise finish!
        return newSeq.Length == 3 ? newSeq : OctThreeDigit(newSeq);
    }
    #endregion

    #region Functions
    /// <summary>
    /// Converts from base 10 to any base.
    /// </summary>
    public static string ConvertBase(int value, char[] baseChars)
    {
        // 32 is the worst cast buffer size for base 2 and int.MaxValue
        byte i = 32;
        char[] buffer = new char[i];
        byte targetBase = (byte)baseChars.Length;

        do
        {
            buffer[--i] = baseChars[value % targetBase];
            value = value / targetBase;
        }
        while (value > 0);

        char[] result = new char[32 - i];
        Array.Copy(buffer, i, result, 0, 32 - i);

        return new string(result);
    }

    /// <summary>
    /// Shuffles the nested array randomly by swapping random indexes with each other.
    /// </summary>
    /// <typeparam name="T">The element type of the array.</typeparam>
    /// <param name="array">The nested array to shuffle.</param>
    private void Shuffle<T>(T[,] array)
    {
        for (byte i = 0; i < array.GetLength(0); i++)
            for (byte j = 0; j < array.GetLength(1); j++)
            {
                byte value = (byte)_rnd.Next(0, array.GetLength(1));

                T temp = array[i, j];
                array[i, j] = array[i, value];
                array[i, value] = temp;
            }
    }
    #endregion

    #region Twitch Plays
    /// <summary>
    /// Determines whether the input from the TwitchPlays chat command is valid. Valid inputs are numbers from 0 to 999.
    /// </summary>
    /// <param name="par">The string from the user.</param>
    private bool IsValid(string par)
    {
        ushort s;
        return ushort.TryParse(par, out s) && s < 1000 && par.Length <= 3;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} play (Plays the sequence provided by the module.) - !{0} submit <###> (Submits the number by holding the button at those specific times. | Valid numbers range from 0-999 | Example: !{0} submit 420) - !{0} mash (Mashes the screen, in octOS it is used for regenerating the module in case of a bug)";
#pragma warning restore 414

    /// <summary>
    /// TwitchPlays Compatibility, detects every chat message and clicks buttons accordingly.
    /// </summary>
    /// <param name="command">The twitch command made by the user.</param>
    IEnumerator ProcessTwitchCommand(string command)
    {
        // Splits each command by spaces.
        string[] user = command.Split(' ');

        // If command is formatted correctly.
        if (Regex.IsMatch(user[0], @"^\s*play\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            // Is animating
            if (_octAnimating)
                yield return "sendtochaterror The module is currently unable to be interacted with!";

            // Sequence is already playing.
            else if (_playSequence)
                yield return "sendtochaterror The sequence is already being played! Wait until the sequence is over!";

            // This command is valid, play sequence.
            else
            {
                Button.OnInteract();
                Button.OnInteractEnded();
            }
        }

        // If command is formatted correctly.
        if (Regex.IsMatch(user[0], @"^\s*mash\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            // Is animating
            if (_octAnimating)
                yield return "sendtochaterror The module is currently unable to be interacted with!";

            // Sequence is already playing.
            else if (_playSequence)
                yield return "sendtochaterror The sequence is already being played! Wait until the sequence is over!";

            // This command is valid, play sequence.
            else
            {
                Button.OnInteract();
                Button.OnInteractEnded();

                while (_octOS && _playSequence)
                {
                    Button.OnInteract();
                    yield return new WaitForEndOfFrame();
                    Button.OnInteractEnded();
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        // If command is formatted correctly.
        else if (Regex.IsMatch(user[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;

            // Is animating
            if (_octAnimating)
                yield return "sendtochaterror The module is currently unable to be interacted with!";

            // No number.
            else if (user.Length < 2)
                yield return "sendtochaterror A number must be specified! (Valid: 0-999)";

            // More than one number.
            else if (user.Length > 2)
                yield return "sendtochaterror Only one number must be specified! (Valid: 0-999)";

            // Number outside range.
            else if (!IsValid(user.ElementAt(1)))
                yield return "sendtochaterror Number wasn't in range! (Valid: 0-999)";

            // If command is valid, push button accordingly.
            else
            {
                // Add leading 0's.
                while (user[1].Length < 3)
                    user[1] = "0" + user[1];

                // Will quickly determine if the module is about to solve or strike.
                if (_octOS && user[1] == _octAnswer)
                {
                    yield return "awardpointsonsolve 36";
                    yield return "solve";
                }
                else if (!_octOS && user[1] == _answer)
                    yield return "solve";

                else if (user[1] == "888" && !_hasPlayedSequence && !_octOS && _canBeOctOS)
                    yield return null;

                else
                    yield return "strike";

                // Cycle through each digit.
                for (byte i = 0; i < user[1].Length; i++)
                {
                    // Wait until the correct number is shown.
                    yield return new WaitWhile(() => user[1][i] != Number.text[i]);

                    // Hold button.
                    Button.OnInteract();

                    // Wait until module can submit.
                    yield return new WaitWhile(() => _held < 20);

                    // Release button.
                    Button.OnInteractEnded();
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

        // Get the correct answer.
        string solve = _octOS ? _octAnswer : _answer;

        Debug.LogFormat("[hexOS #{0}]: Autosolver initiated. The module will now submit {1}.", _moduleId, solve);

        // Cycle through each digit.
        for (byte i = 0; i < solve.Length; i++)
        {
            // Wait until the correct number is shown.
            yield return new WaitWhile(() => solve[i] != Number.text[i]);

            // Hold button.
            Button.OnInteract();

            // Wait until module can submit.
            yield return new WaitWhile(() => _held < 20);

            // Release button.
            Button.OnInteractEnded();
        }
    }
    #endregion
}