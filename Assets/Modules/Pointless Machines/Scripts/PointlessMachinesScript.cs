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

    private Vector3 _neutral = Vector3.zero;

    private KMBomb _bomb;

    private void Start()
    {
        Get<AudioScript>().Play(SFX.Pm.Loop, loop: true, volume: 0);

        Get<KMSelectable>().Assign(overrideReturn: !IsVR, onInteract: () =>
        {
            if (IsSolved)
                return;

            _isSelected = !_isSelected;
            _neutral = _bomb.transform.localEulerAngles;

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

        Calculate(flashes, new List<int>());

        answer = flashes.Select(f => _conversion[f]).ToArray();

        Log("The answer is {0}.".Form(answer.UnwrapToString(delimiter: ", then ")));

        StartCoroutine(KMBombListen());
        StartCoroutine(MachineHandler(flashes));
    }

    private Flash[] GetFlashes()
    {
        int[] comp;

        if (IsEditor)
        {
            if (Flashes.Length != 6 || Flashes.Distinct().Count() != 5)
                throw new ArgumentException("Flashes must be length 6 and have all colors: " + Flashes.UnwrapToString());

            comp = Flashes.Select((f, i) => f.ToTuple(i)).Where(t => t.Item1 == Flashes.Indistinct().First()).Select(t => t.Item2).ToArray();

            if (Math.Abs(comp[0] - comp[1]) == 1)
                throw new ArgumentException("Flashes cannot have any of the same members be neighbours: " + Flashes.UnwrapToString());

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

        if (current == flashes.Indistinct().First())
            return AddEntry(current.Call(c => Log("The 2nd condition is used because {0} shows up twice.".Form(c))), Calculate(flashes, indicesUsed, index - 1, iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))), GetConstantView(flashes.ElementAtWrap(flashes.Select((f, n) => f.ToTuple(n)).Where(t => t.Item1 == current && t.Item2 != index).Select(t => t.Item2).First() + 1).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        if (flashes.Reverse().Select((f, n) => f.ToTuple(n)).Where(t => t.Item1 == flashes.Indistinct().First()).Any(t => t.Item2 == index))
            return AddEntry(current.Call(c => Log("The 3rd condition is used because {0} is opposite of a non-unique color.".Form(c))), Calculate(flashes, indicesUsed, index + 3, iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))), GetConstantView(flashes.ElementAtWrap(2 - index).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        if (index >= flashes.Length / 2)
            return AddEntry(current.Call(c => Log("The 4th condition is used because {0} is on the second half of the sequence.".Form(c))), Calculate(flashes, indicesUsed, 1, iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))), GetConstantView(flashes[4]).Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c)))).Call(c => Log("{0} is equal to {1}.".Form(current, c)));

        return AddEntry(current.Call(c => Log("The 5th condition is used because {0} matches none of the other conditions.".Form(c))), Calculate(flashes, indicesUsed, flashes.LastIndexOf(flashes.Indistinct().First()), iter - 1).Call(c => Log("The {0} variable in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))), GetConstantView(flashes.Indistinct().First().Call(c => Log("The {0} constant in the sequence is {1}.".Form((index + 1).ToOrdinal(), c))))).Call(c => Log("{0} is equal to {1}.".Form(current, c)));
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
        do
        {
            _bomb = GetComponentInParent<KMBomb>();
            yield return new WaitForSecondsRealtime(0.1f);
        } while (_bomb == null);

        while (!IsSolved)
        {
            var vector = WrappedVector(_bomb.transform.localEulerAngles - _neutral);

            if (vector.y >= 180)
                vector = WrappedVector(new Vector3((vector.x * -1) - 30, vector.y - 180, vector.z + 180));

            var view = ViewFromVector(vector);

            if (IsEditor)
                Text.text = "{0}\n{1}\n{2}\n\n{3}".Form(Math.Truncate(vector.x), Math.Truncate(vector.y), Math.Truncate(vector.z), view);

            HandleView(view);

            yield return new WaitUntil(() => _isSelected);
            yield return new WaitForSecondsRealtime(0.1f);
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

        Enumerable.Range(1, 8).ToArray().ForEach(i => Instantiate(Light, LightParent, false).GetComponent<LightScript>().enabled = true);

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

    private View GetConstantView(Flash flash)
    {
        switch (flash)
        {
            case Flash.Purple: return View.Left;
            case Flash.Yellow: return View.Left | View.Up;
            case Flash.White: return View.Up;
            case Flash.Red: return View.Up | View.Right;
            case Flash.Blue: return View.Down | View.Right;
            default: throw new NotImplementedException("Unrecognised flash type: " + flash);
        }
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
