using KeepCoding;
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
    public Renderer ModuleRenderer;
    public Texture[] ModuleTextures;

    internal int Stage { get; set; }

    private int CurrentlySolvingId { get; set; }
    private int NetheriteId { get; set; }
    private int NetheriteCount { get { return IsEditor ? 3 : Get<KMBombInfo>().GetSolvableModuleNames().Count(m => m == Module.Name); } }
    
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

    internal int[] Sequence
    {
        get
        {
            int[] vs = new int[11];
            vs[0] = SerialWithSolves.ElementAtWrap(0).DigitalRoot();
            for (int i = 1; i < vs.Length; i++)
                vs[i] = (SerialWithSolves.ElementAtWrap(i) + vs[i - 1]).DigitalRoot();
            return vs;
        }
    }

    private IEnumerable<int> Serial { get { return Get<KMBombInfo>().GetSerialNumberNumbers(); } }
    private IEnumerable<int> SerialWithSolves { get { return Serial.Prepend(NetheriteId); } }

    private NetheriteScript[] AllNetherites { get { return GetComponentInParent<KMBomb>().GetComponentsInChildren<NetheriteScript>(); } }

    private void Start()
    {
        Buttons.Assign(onInteract: OnInteract);

        NetheriteId = 1;

        Log("The 3x3 board will submit the following values:");

        // This logs the entire board.
        for (int i = 0; i < 3; i++)
            Log(Enumerable.Range(i * 3, 3).Select(j => ApplyRules(j)).Join(" "));

        // The only way to adjust the answer is to transmutate NetheriteID, this allows us to log each answer.
        for (NetheriteId = 1; NetheriteId <= NetheriteCount; NetheriteId++)
            Log("The expected sequence ({0} netherite) is {1}.", NetheriteId.ToOrdinal(), Sequence.Join());

        NetheriteId = 1;
    }

    private void OnInteract(int i)
    {
        // On solve the module disappears, it would be weird for it to still be possible to interact with it.
        if (IsSolved)
            return;

        // Plays a random dig sound.
        ButtonEffect(Buttons[i], 1, SFX.N.Dig, KMSoundOverride.SoundEffect.ButtonPress);

        // Ping if this is the first time the module is being cracked.
        if (Stage == 0)
            PlaySound(SFX.N.Ping);

        // Plays the break block effect.
        Get<ParticleSystem>().Play();

        // This ensures that only one of them can be solved at a time, which is part of the rules.
        if (CurrentlySolvingId == 0)
            CurrentlySolvingId = Id;

        // This strikes the module if either the condition is wrong or multiple of them are being solved.
        if (Sequence[Stage] != ApplyRules(i) || CurrentlySolvingId != Id)
        {
            PlaySound(SFX.N.Hit);
            Strike("While trying to mine for the {0} time, the value {1} was submitted, when {2} was expected! Strike!".Form(
                (Stage + 1).ToOrdinal(),
                ApplyRules(i),
                Sequence[Stage]));
        }

        // This makes the module component appear to crack more.
        ModuleRenderer.material.mainTexture = ModuleTextures.ElementAtOrDefault(++Stage);

        // This solves the module.
        if (Stage >= Sequence.Length)
        {
            AllNetherites.ForEach(n =>
            {
                n.NetheriteId++;
                n.CurrentlySolvingId = 0;
            });

            // Makes the module disappear.
            ModuleRenderer.transform.localScale = Vector3.zero;

            // Cannot set values whilst particle system is playing.
            Get<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Increases particle duration and amount.
            var main = Get<ParticleSystem>().main;
            main.duration = 2;
            main.maxParticles = 100;

            Get<ParticleSystem>().Play();

            PlaySound(SFX.N.Solve);

            Solve("The Netherite block has been mined. Module solved!");
        }
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

    private static int FlipIndexHorizontally(int i)
    {
        return (i / 3 * 3) + (2 - (i % 3));
    }

    private static int FlipIndexVertically(int i)
    {
        return (i % 3) + (3 * (2 - (i / 3)));
    }
}