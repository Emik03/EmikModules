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

    private int NetheriteCount
    {
        get
        {
            return IsEditor ? 3 : Get<KMBombInfo>().GetSolvableModuleNames().Count(m => m == Module.Name);
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

    private IEnumerable<int> Serial
    {
        get
        {
            return Get<KMBombInfo>().GetSerialNumberNumbers();
        }
    }

    private IEnumerable<int> SerialWithSolves
    {
        get
        {
            return Helper.Prepend(_serialOrder(Serial), NetheriteId + _rulseedOffset);
        }
    }

    private NetheriteScript[] AllNetherites
    {
        get
        {
            return GetComponentInParent<KMBomb>().GetComponentsInChildren<NetheriteScript>();
        }
    }

    private void Start()
    {
        Buttons.Assign(onInteract: OnInteract);

        GenerateRuleseed(Get<KMRuleSeedable>().GetRNG());
        NetheriteId = 1;

        if (!_is2FA || !_edgeworkCheck())
        {
            bool A, B;
            if (_edgeworkCheck())
            {
                A = _edgeworkCheckA();
                B = _edgeworkCheckB();
            }
            else
            {
                A = Get<KMBombInfo>()
                       .GetOnIndicators()
                       .Count()
                       .ToString()
                       .Any(a => Serial.Sum().ToString().Any(b => a == b)) ||
                    Serial.Any(a => a == _edgeworkCheckC());
                B = Get<KMBombInfo>()
                       .GetOffIndicators()
                       .Count()
                       .ToString()
                       .Any(a => Serial.Sum().ToString().Any(b => a == b)) ||
                    Serial.Any(a => a == _edgeworkCheckD());
            }

            Log("Rules are {0} and {1}.", A ? "true" : "false", B ? "true" : "false");

            Log("The 3x3 board will submit the following values:");

            // This logs the entire board.
            for (int i = 0; i < 3; i++)
                Log(Enumerable.Range(i * 3, 3).Select(j => ApplyRules(j)).Join(" "));
        }
        else
        {
            Log("The 3x3 board will submit the following values for each possible rule combination:");
            Log("(False False)");
            for (int i = 0; i < 3; i++)
                Log(Enumerable.Range(i * 3, 3).Select(j => _tables[0][j]).Join(" "));
            Log("(False True)");
            for (int i = 0; i < 3; i++)
                Log(Enumerable.Range(i * 3, 3).Select(j => _tables[1][j]).Join(" "));
            Log("(True False)");
            for (int i = 0; i < 3; i++)
                Log(Enumerable.Range(i * 3, 3).Select(j => _tables[2][j]).Join(" "));
            Log("(True True)");
            for (int i = 0; i < 3; i++)
                Log(Enumerable.Range(i * 3, 3).Select(j => _tables[3][j]).Join(" "));
        }

        // The only way to adjust the answer is to transmutate NetheriteID, this allows us to log each answer.
        for (NetheriteId = 1; NetheriteId <= NetheriteCount; NetheriteId++)
            Log("The expected sequence ({0} netherite) is {1}.", NetheriteId.ToOrdinal(), Sequence.Join());

        NetheriteId = 1;
    }

    private int _rulseedOffset;
    private System.Func<IEnumerable<int>, IEnumerable<int>> _serialOrder = i => i;
    internal System.Func<bool> _edgeworkCheck, _edgeworkCheckA, _edgeworkCheckB;
    private System.Func<int> _edgeworkCheckC, _edgeworkCheckD;
    internal bool _is2FA;
    private int[][] _tables = new int[][]
    {
        new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
        new int[] { 7, 8, 9, 4, 5, 6, 1, 2, 3 },
        new int[] { 3, 2, 1, 6, 5, 4, 9, 8, 7 },
        new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 },
        new int[] { 1, 4, 7, 2, 5, 8, 3, 6, 9 },
        new int[] { 7, 4, 1, 8, 5, 2, 9, 6, 3 },
        new int[] { 3, 6, 9, 2, 5, 8, 1, 4, 7 },
        new int[] { 9, 6, 3, 8, 5, 2, 7, 4, 1 }
    };
    private void GenerateRuleseed(MonoRandom rng)
    {
        Log("Using ruleseed {0}.", rng.Seed);

        _edgeworkCheck = () => Voltage != null;
        _edgeworkCheckA = () => Voltage % 1 != 0;
        _edgeworkCheckB = () => Voltage > 5;
        _edgeworkCheckC = Get<KMBombInfo>().GetOnIndicators().Count;
        _edgeworkCheckD = Get<KMBombInfo>().GetOffIndicators().Count;
        if (rng.Seed == 1)
            return;
        _rulseedOffset = rng.Next(10) - 1;
        if (rng.Next(2) == 1)
            _serialOrder = i => i.Reverse();
        int modded = rng.Next(3);
        System.Func<bool>[] conds = new System.Func<bool>[0];
        switch (modded)
        {
            case 0: // Voltage Meter
                _edgeworkCheck = () => Voltage != null;
                conds = new System.Func<bool>[]
                {
                    () => Voltage % 1 == 0,
                    () => Voltage % 1 != 0,
                    () => Voltage > 5,
                    () => Voltage < 6,
                    () => (int)Voltage % 2 == 0,
                    () => (int)Voltage % 2 == 1
                };
                break;
            case 1: // Modded Ports
                string[] vanillaPorts = "DVI/PS2/RJ45/StereoRCA/Parallel/Serial".Split("/");
                _edgeworkCheck = () => Get<KMBombInfo>()
                    .GetPorts()
                    .Any(port => !vanillaPorts.Contains(port));
                conds = new System.Func<bool>[]
                {
                    () => Get<KMBombInfo>().GetPorts().Contains("AC"),
                    () => Get<KMBombInfo>().GetPorts().Contains("ComponentVideo"),
                    () => Get<KMBombInfo>().GetPorts().Contains("HDMI"),
                    () => Get<KMBombInfo>().GetPorts().Contains("CompositeVideo"),
                    () => Get<KMBombInfo>().GetPorts().Contains("VGA"),
                    () => Get<KMBombInfo>().GetPorts().Contains("USB"),
                    () => Get<KMBombInfo>().GetPorts().Contains("PCMCIA")
                };
                break;
            case 2: // 2FA
                _is2FA = true;
                _edgeworkCheck = Get<KMBombInfo>().IsTwoFactorPresent;
                conds = new System.Func<bool>[]
                {
                    () => Get<KMBombInfo>().GetTwoFactorCodes().Any(i => i % 2 == 1),
                    () => Get<KMBombInfo>().GetTwoFactorCodes().Any(i => i % 2 == 0),
                    () => Get<KMBombInfo>().GetTwoFactorCodes().Any(i => (i.ToString()[0] - '0') % 2 == 1),
                    () => Get<KMBombInfo>().GetTwoFactorCodes().Any(i => (i.ToString()[0] - '0') % 2 == 0)
                };
                break;
        }
        conds = conds.OrderBy(_ => rng.NextDouble()).Take(2).ToArray();
        _edgeworkCheckA = conds[0];
        _edgeworkCheckB = conds[1];
        var conds2 = new System.Func<int>[]
        {
            Get<KMBombInfo>().GetOnIndicators().Count,
            Get<KMBombInfo>().GetOffIndicators().Count,
            Get<KMBombInfo>().GetBatteryCount,
            Get<KMBombInfo>().GetBatteryHolderCount,
            Get<KMBombInfo>().GetPortCount,
            Get<KMBombInfo>().GetPortPlateCount,
            () => (int)(Get<KMBombInfo>().GetModuleNames().Count / 10f),
            () => (int)(Get<KMBombInfo>().GetModuleNames().Count / 10f) == (Get<KMBombInfo>().GetModuleNames().Count / 10f)
                ?(int)(Get<KMBombInfo>().GetModuleNames().Count / 10f)
                :(int)(Get<KMBombInfo>().GetModuleNames().Count / 10f) + 1,
        };
        conds2 = conds2.OrderBy(_ => rng.NextDouble()).Take(2).ToArray();
        _edgeworkCheckC = conds2[0];
        _edgeworkCheckD = conds2[1];
        _tables = _tables.OrderBy(_ => rng.NextDouble()).ToArray();
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

            Strike(
                "While trying to mine for the {0} time, the value {1} was submitted, when {2} was expected! Strike!"
                   .Form(
                        (Stage + 1).ToOrdinal(),
                        ApplyRules(i),
                        Sequence[Stage]
                    )
            );
        }

        // If there is another Netherite on the bomb which is partially cracked,
        // you CANNOT crack any other Netherites until that block is fully cracked.
        if (CurrentlySolvingId != Id)
            return;

        // This makes the module component appear to crack more.
        ModuleRenderer.material.mainTexture = ModuleTextures.ElementAtOrDefault(++Stage);

        // This solves the module.
        if (Stage >= Sequence.Length)
        {
            AllNetherites.ForEach(
                n =>
                {
                    n.NetheriteId++;
                    n.CurrentlySolvingId = 0;
                }
            );

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
        byte flips = 0;
        if (_edgeworkCheck())
        {
            if (_edgeworkCheckA())
                flips |= 2;

            if (_edgeworkCheckB())
                flips |= 1;
        }
        else
        {
            if (Get<KMBombInfo>()
                   .GetOnIndicators()
                   .Count()
                   .ToString()
                   .Any(a => Serial.Sum().ToString().Any(b => a == b)) ||
                Serial.Any(a => a == _edgeworkCheckC()))
                flips |= 2;

            if (Get<KMBombInfo>()
                   .GetOffIndicators()
                   .Count()
                   .ToString()
                   .Any(a => Serial.Sum().ToString().Any(b => a == b)) ||
                Serial.Any(a => a == _edgeworkCheckD()))
                flips |= 1;
        }

        return _tables[flips][i];
    }
}
