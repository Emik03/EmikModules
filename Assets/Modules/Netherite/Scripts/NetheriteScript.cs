using EmikBaseModules;
using KModkit;
using Netherite;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// On the Subject of Netherite - A modded "Keep Talking and Nobody Explodes" module created by Emik.
/// </summary>
public class NetheriteScript : ModuleScript
{
    public KMSelectable[] Buttons;
    public ParticleSystem Particle, ParticleOnSolve;
    public Renderer ModuleRenderer;
    public Texture[] ModuleTextures;

    internal bool IsStrike { get; set; }

    private IEnumerable<int> Serial { get { return Get<KMBombInfo>().GetSerialNumberNumbers(); } }
    private IEnumerable<int> SerialWithSolves { get { return Serial.Prepend(NetheriteID); } }

    internal int Stage { get; set; }
    private int NetheriteCount { get { return IsEditor ? 3 : Get<KMBombInfo>().GetSolvableModuleNames().Where(m => m == "Netherite").Count(); } }

    private static int NetheriteID { get; set; }
    private static int CurrentlySolvingID
    {
        get { return _currentlySolvingId; }
        set { if (_currentlySolvingId == default(int)) _currentlySolvingId = value; }
    }
    private static int _currentlySolvingId;

    internal int[] Sequence
    {
        get
        {
            int[] vs = new int[11];
            vs[0] = DigitalRoot(SerialWithSolves.ElementAtWrap(0));
            for (int i = 1; i < vs.Length; i++)
                vs[i] = DigitalRoot(SerialWithSolves.ElementAtWrap(i) + vs[i - 1]);
            return vs;
        }
    }

    private float? Voltage
    {
        get
        {
            var query = Get<KMBombInfo>().QueryWidgets("volt", "");
            return query.Count != 0
                ? (float?)float.Parse(JsonConvert.DeserializeObject<VoltData>(query.First()).Voltage)
                : null;
        }
    }

    private void Start()
    {
        Buttons.Assign(onInteract: OnInteract);

        // Static values have to be reset.
        _currentlySolvingId = default(int);
        NetheriteID = 1;

        Log("The 3x3 board will submit the following values:");

        // This logs the entire board.
        for (int i = 0; i < 3; i++)
            Log(Enumerable.Range(i * 3, 3).Select(j => ApplyRules(j)).Join(" "));

        // The only way to adjust the answer is to transmutate NetheriteID, this allows us to log each answer.
        for (NetheriteID = 1; NetheriteID <= NetheriteCount; NetheriteID++)
            Log("The expected sequence ({0} netherite) is {1}.", ToOrdinal(NetheriteID), Sequence.Join());

        NetheriteID = 1;
    }

    internal int ApplyRules(int i)
    {
        // These are the rules for when a Voltage Meter widget is on the bomb.
        if (Voltage != null)
        {
            if (Voltage % 1 != 0)
                i = FlipIndexHorizontally(i);

            if (Voltage > 5)
                i = FlipIndexVertically(i);
        }

        // These are the rules for when a Voltage Meter widget is not on the bomb.
        else
        {
            if (Get<KMBombInfo>().GetOnIndicators().Count().ToString().Any(a => Serial.Sum().ToString().Any(b => a == b))
                || Serial.Any(a => a == Get<KMBombInfo>().GetOnIndicators().Count()))
                i = FlipIndexHorizontally(i);

            if (Get<KMBombInfo>().GetOffIndicators().Count().ToString().Any(a => Serial.Sum().ToString().Any(b => a == b))
                || Serial.Any(a => a == Get<KMBombInfo>().GetOffIndicators().Count()))
                i = FlipIndexVertically(i);
        }

        // We have to increment i by 1 due to an off-by-one error.
        return ++i;
    }

    private void OnInteract(int i)
    {
        // On solve the module disappears, it would be weird for it to still be possible to interact with it.
        if (IsSolved)
            return;

        // Plays a random dig sound.
        Buttons[i].Push(Get<KMAudio>(), 1, Sounds.Dig, KMSoundOverride.SoundEffect.ButtonPress);

        // Ping if this is the first time the module is being cracked.
        if (Stage == 0)
            Get<KMAudio>().Play(transform, Sounds.Ping);

        // Plays the break block effect.
        Particle.Play();

        // This ensures that only one of them can be solved at a time, which is part of the rules.
        CurrentlySolvingID = ModuleId;

        // This strikes the module if either the condition is wrong or multiple of them are being solved.
        if (Sequence[Stage] != ApplyRules(i) || CurrentlySolvingID != ModuleId)
        {
            Get<KMAudio>().Play(transform, Sounds.Hit);
            Strike("While trying to mine for the {0} time, the value {1} was submitted, when {2} was expected! Strike!".Form(
                ToOrdinal(Stage + 1),
                ApplyRules(i),
                Sequence[Stage]));
        }

        // This makes the module component appear to crack more.
        ModuleRenderer.material.mainTexture = ModuleTextures[++Stage];

        // This solves the module.
        if (Stage >= Sequence.Length)
        {
            NetheriteID++;
            _currentlySolvingId = default(int);

            // Makes the module disappear.
            ModuleRenderer.transform.localScale = new Vector3(0, 0, 0);

            ParticleOnSolve.Play();

            Get<KMAudio>().Play(transform, Sounds.Solve);

            Solve("The Netherite block has been mined. Module solved!");
        }
    }

    private static int FlipIndexHorizontally(int i)
    {
        return (i / 3 * 3) + (2 - (i % 3));
    }

    private static int FlipIndexVertically(int i)
    {
        return (i % 3) + (3 * (2 - (i / 3)));
    }

    private static int DigitalRoot(int i)
    {
        return (i - 1) % 9 + 1;
    }

    private static string ToOrdinal(int i)
    {
        switch ((i / 10 % 10) == 1 ? 0 : i % 10)
        {
            case 1: return i + "st";
            case 2: return i + "nd";
            case 3: return i + "rd";
            default: return i + "th";
        }
    }
}
