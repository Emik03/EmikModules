using KeepCoding;
using PointlessMachines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointlessMachinesScript : ModuleScript
{
    public Color32[] Static, Purple, Yellow, White, Red, Blue;
    public HingeScript[] Hinges;
    public Light Light;
    public Renderer[] Display;
    public TextMesh Text;
    public Transform LightParent, Status;
    public Flash[] Flashes;

    internal View[] answer;

    private bool _isSelected, _isSubmit;

    private static readonly View[] _indices = new View[] { View.Left, View.Left | View.Up, View.Up, View.Up | View.Right, View.Right, View.Down | View.Right, View.Down, View.Left | View.Down, };
    private View[] _views = new View[0];

    private readonly Dictionary<Flash, View> _conversion = new Dictionary<Flash, View>();

    private Vector3 _neutral;

    private void Start()
    {
        Get<AudioScript>().Play(SFX.Pm.Loop, loop: true, volume: 0);

        Get<KMSelectable>().Assign(overrideReturn: !IsVR, onInteract: () =>
        {
            if (IsSolved)
                return;

            _isSelected = !_isSelected;
            _neutral = Bomb.transform.localEulerAngles;

            if (_isSelected)
            {
                PlaySound(SFX.Pm.Reset);
                Get<AudioScript>().Fade(volume: 0.25f, time: 3);
                return;
            }

            Get<AudioScript>().Fade(volume: 0, time: 3);
        });

        var flashes = GetFlashes();

        Log("The flashes are {0}.".Form(flashes));

        GenerateRuleseed(Get<KMRuleSeedable>().GetRNG());
        Calculate(flashes, new List<int>());

        answer = flashes.Select(f => _conversion[f]).ToArray();

        Log("The answer is {0}.".Form(answer.Combine(delimiter: ", then ")));

        StartCoroutine(KMBombListen());
        StartCoroutine(MachineHandler(flashes));
    }

    private static readonly Func<Flash[], int, bool>[] _majorRules = new Func<Flash[], int, bool>[]
    {
        (a, i) => a[i] == a.Indistinct().First(),
        (a, i) => a[5 - i] == a.Indistinct().First(),
        (_, i) => i > 2,
        (_, i) => i < 3,
        (_, i) => i % 2 == 0,
        (_, i) => i % 2 == 1,
        (a, i) => a[i] != a.Indistinct().First()
    };
    private static readonly Func<Flash[], int, int>[] _variableRules = new Func<Flash[], int, int>[]
    {
        (_, i) => (i + 5) % 6,
        (_, i) => (i + 3) % 6,
        (_, i) => 1,
        (a, _) => a.LastIndexOf(a.Indistinct().First()),
        (_, i) => (i + 1) % 6,
        (_, i) => (i + 2) % 6,
        (_, i) => (i + 4) % 6,
        (_, i) => 0,
        (_, i) => 2,
        (_, i) => 3,
        (_, i) => 4,
        (_, i) => 5,
        (a, _) => a.IndexOf(a.Indistinct().First()),
        (_, i) => 5 - i,
        (_, i) => 5 - ((i + 3) % 6)
    };
    private static readonly Func<Flash[], int, Flash>[] _constantRules = new Func<Flash[], int, Flash>[]
    {
        (a, i) => a[(a.IndexOf((f, x) => f == a[i] && x != i) + 1) % 6],
        (a, i) => a[5 - ((i + 3) % 6)],
        (a, _) => a[4],
        (a, _) => a.Indistinct().First(),
        (a, i) => a[(i + 5) % 6],
        (a, i) => a[(i + 1) % 6],
        (a, i) => a[(i + 3) % 6],
        (a, i) => a[(i + 2) % 6],
        (a, i) => a[(i + 4) % 6],
        (a, _) => a[0],
        (a, _) => a[1],
        (a, _) => a[2],
        (a, _) => a[3],
        (a, _) => a[5],
        (a, i) => a[5 - i]
    };
    private static readonly View[] _directions = new View[]
    {
        View.Up,
        View.Left,
        View.Up | View.Right,
        View.Down | View.Right,
        View.Up | View.Left,
        View.Right,
        View.Down,
        View.Down| View.Left
    };
    private static readonly Flash[] _manualOrder = new Flash[] { Flash.White, Flash.Purple, Flash.Red, Flash.Blue, Flash.Yellow };

    private Func<Flash[], int, bool> _majorRuleA, _majorRuleB, _majorRuleC;
    private Func<Flash[], int, int> _variableRuleA, _variableRuleB, _variableRuleC, _variableRuleD;
    private Func<Flash[], int, Flash> _constantRuleA, _constantRuleB, _constantRuleC, _constantRuleD;
    private void GenerateRuleseed(MonoRandom rng)
    {
        Log("Using ruleseed {0}.", rng.Seed);
        Func<Flash[], int, bool>[] maj;
        Func<Flash[], int, int>[] var;
        Func<Flash[], int, Flash>[] con;
        View[] dirs;
        if (rng.Seed == 1)
        {
            maj = _majorRules;
            var = _variableRules;
            con = _constantRules;
            dirs = _directions;
        }
        else
        {
            maj = _majorRules.OrderBy(_ => rng.NextDouble()).Take(3).ToArray();
            var = _variableRules.OrderBy(_ => rng.NextDouble()).Take(4).ToArray();
            con = _constantRules.Skip(1).OrderBy(_ => rng.NextDouble()).Take(4).ToArray();
            dirs = _directions.OrderBy(_ => rng.NextDouble()).Take(5).ToArray();
        }

        _majorRuleA = maj[0];
        _majorRuleB = maj[1];
        _majorRuleC = maj[2];
        _variableRuleA = var[0];
        _variableRuleB = var[1];
        _variableRuleC = var[2];
        _variableRuleD = var[3];
        _constantRuleA = con[0];
        _constantRuleB = con[1];
        _constantRuleC = con[2];
        _constantRuleD = con[3];
        _flashTypes = Enumerable.Range(0, 5).ToDictionary(i => _manualOrder[i], i => dirs[i]);

        Log(_variableRuleD(new Flash[] { Flash.White, Flash.Purple, Flash.Red, Flash.Blue, Flash.Yellow, Flash.Red }, 4));
        Log(_constantRuleD(new Flash[] { Flash.White, Flash.Purple, Flash.Red, Flash.Blue, Flash.Yellow, Flash.Red }, 4));
    }

    public override void OnActivate()
    {
        if (TP.IsTP)
            Status.transform.localPosition = new Vector3(0, 0.01f, 0);
    }

    private Flash[] GetFlashes()
    {
        int[] comp;

        if (IsEditor)
        {
            if (Flashes.Length != 6 || Flashes.Distinct().Count() != 5)
                throw new ArgumentException("Flashes must be length 6 and have all colors: " + Flashes.Combine());

            comp = Flashes.Select((f, i) => f.ToTuple(i)).Where(t => t.Item1 == Flashes.Indistinct().First()).Select(t => t.Item2).ToArray();

            if (Math.Abs(comp[0] - comp[1]) == 1)
                throw new ArgumentException("Flashes cannot have any of the same members be neighbours: " + Flashes.Combine());

            return Flashes;
        }

        Flash[] flashes;

        do
        {
            flashes = default(Flash).GetValues().Append(default(Flash).GetValues().PickRandom()).Shuffle();
            comp = flashes.Select((f, i) => f.ToTuple(i)).Where(t => t.Item1 == flashes.Indistinct().First()).Select(t => t.Item2).ToArray();
        }
        while (Math.Abs(comp[0] - comp[1]) == 1);

        return flashes;
    }

    private IEnumerator MachineHandler(Flash[] flashes)
    {
        const float Wait = 0.06f;

        while (!IsSolved)
        {
            for (int i = 0; i < flashes.Length; i++)
            {
                yield return MachineFlash(flashes[i], Wait * 8);
                yield return MachineStatic(4, Wait, false);
            }

            yield return MachineStatic(32, Wait, false);
        }
    }

    private IEnumerator MachineFlash(Flash flash, float length)
    {
        Color32[] colors;
        string sound;

        switch (flash)
        {
            case Flash.Purple: colors = Purple; sound = SFX.Pm.Purple; break;
            case Flash.Yellow: colors = Yellow; sound = SFX.Pm.Yellow; break;
            case Flash.White: colors = White; sound = SFX.Pm.White; break;
            case Flash.Red: colors = Red; sound = SFX.Pm.Red; break;
            case Flash.Blue: colors = Blue; sound = SFX.Pm.Blue; break;
            default: throw new NotImplementedException(flash.ToString());
        }

        if (_isSelected)
            PlaySound(sound);

        for (int i = 0; i < Display.Length; i++)
            Display[i].material.color = colors[i];

        yield return new WaitForSecondsRealtime(length);
    }

    private IEnumerator MachineStatic(int times, float length, bool infinite)
    {
        for (int i = 0; i < times || infinite; i++)
        {
            Display.ForEach(r => r.material.color = Static.PickRandom());
            yield return new WaitForSecondsRealtime(length);
        }
    }

    private View Calculate(Flash[] flashes, List<int> indicesUsed, int index = 0, int iter = 4)
    {
        index = index.Modulo(flashes.Length);

        while (indicesUsed.Contains(index) || _conversion.ContainsKey(flashes[index]))
            index = (index + 1).Modulo(flashes.Length);

        var current = flashes[index];

        Log("The current color is {0}, located at the {1} position.".Form(current, (index + 1).ToOrdinal()));

        indicesUsed.Add(index);
        _conversion.Add(current, View.None);

        if (iter == 0)
            return AddEntry(current.Call(c => Log("The 1st condition is used because {0} is the last color.".Form(c))), GetConstantView(current).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))), GetConstantView(current).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c)))).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        if (_majorRuleA(flashes, index))
            return AddEntry(
                current.Call(_ => Log("The 2nd condition is used.")),
                Calculate(flashes, indicesUsed, _variableRuleA(flashes, index), iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))),
                GetConstantView(_constantRuleA(flashes, index).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))
                ).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        if (_majorRuleB(flashes, index))
            return AddEntry(
                current.Call(_ => Log("The 3rd condition is used.")),
                Calculate(flashes, indicesUsed, _variableRuleB(flashes, index), iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))),
                GetConstantView(_constantRuleB(flashes, index).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))
                ).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        if (_majorRuleC(flashes, index))
            return AddEntry(
                current.Call(_ => Log("The 4th condition is used.")),
                Calculate(flashes, indicesUsed, _variableRuleC(flashes, index), iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))),
                GetConstantView(_constantRuleC(flashes, index).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))
                ).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        return AddEntry(
            current.Call(_ => Log("The 5th condition is used.")),
            Calculate(flashes, indicesUsed, _variableRuleD(flashes, index), iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))),
            GetConstantView(_constantRuleD(flashes, index).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))
            ).Call(c => Log("{0} is equal to {1}.".Form(current, c)));
    }

    private View AddEntry(Flash flash, View view, View otherView)
    {
        _conversion[flash] = CombineViews(view, otherView);
        return _conversion[flash];
    }

    private View CombineViews(View viewA, View viewB)
    {
        int i = _indices.IndexOf(viewB) + _indices.IndexOf(viewB) - _indices.IndexOf(viewA);

        while (_conversion.Any(p => p.Value == _indices.ElementAtWrap(i)))
            i++;

        return _indices.ElementAtWrap(i);
    }

    private IEnumerator KMBombListen()
    {
        while (Bomb == null)
            yield return null;

        while (!IsSolved)
        {
            var vector = WrappedVector(Bomb.transform.localEulerAngles - _neutral);

            if (vector.y >= 180)
                vector = WrappedVector(new Vector3(vector.x * -1 - 30, vector.y - 180, vector.z + 180));

            var view = ViewFromVector(vector);

            if (IsEditor)
                Text.text = "{0}\n{1}\n{2}\n\n{3}".Form(Math.Truncate(vector.x), Math.Truncate(vector.y), Math.Truncate(vector.z), view);

            HandleView(view);

            yield return new WaitUntil(() => _isSelected);
            yield return null;
        }
    }

    internal void HandleView(View view)
    {
        if (view == View.None)
        {
            _isSubmit = true;
            return;
        }

        if (!_isSubmit || _views.LastOrDefault() == view)
            return;

        var submit = view.ToString().Split(", ").Select(s => s.First());

        Hinges.ForEach(h =>
        {
            if (submit.Count() == 1 && submit.ElementAt(0) == h.name[0]
            || submit.Count() == 2 && submit.All(c => h.name.Contains(c)))
                h.Flash();
        });

        _views = _views.Append(view);

        if (view != answer[_views.GetUpperBound(0)])
            _views = new View[0];

        if (_isSubmit)
        {
            _isSubmit = false;
            PlaySound(SFX.Pm.Direction(_indices.IndexOf(view)));
        }

        if (!_views.SequenceEqual(answer))
            return;

        StopAllCoroutines();
        StartCoroutine(SolveAnimation());
    }

    internal IEnumerator SolveAnimation()
    {
        PlaySound(SFX.Pm.Solve);

        8.For(_ =>
        {
            Instantiate(Light, LightParent, false).GetComponent<LightScript>().enabled = false;
        });

        Get<AudioScript>().Fade(volume: 0, time: 0.3f);

        yield return new WaitForSeconds(0.3f);

        Get<AudioScript>().Stop();

        yield return new WaitForSeconds(3);

        Status.localPosition = Vector3.zero.Set(y: 0.0133f);

        StartCoroutine(MachineStatic(0, 0.06f, true));

        Solve();
    }

    private View ViewFromVector(Vector3 vector)
    {
        const float Min = 12, Max = 32;

        var direction = View.None;

        float x = Mathf.Abs(vector.x), z = Mathf.Abs(vector.z);

        if (Mathf.Max(x, z) > Max * 2)
            return direction;

        if (x < Min && z < Max)
            return direction;

        if (z < Min && x < Max)
            return direction;

        if (vector.z < -Min)
            direction |= View.Left;
        else if (vector.z > Min)
            direction |= View.Right;

        if (vector.x < -Min)
            direction |= View.Up;
        else if (vector.x > Min)
            direction |= View.Down;

        return direction;
    }

    private Dictionary<Flash, View> _flashTypes;
    private View GetConstantView(Flash flash)
    {
        return _flashTypes[flash];
    }

    private Vector3 WrappedVector(Vector3 vector)
    {
        return new Vector3(WrappedFloat(vector.x), WrappedFloat(vector.y), WrappedFloat(vector.z));
    }

    private float WrappedFloat(float f)
    {
        return f > 180 ? f - 360 : f;
    }
}
