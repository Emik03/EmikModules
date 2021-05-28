using TheOctadecayotton;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Linq;

public class InteractScript : MonoBehaviour
{
    public GameObject Polygon, Sphere;
    public KMBombInfo Info;
    public Renderer ModuleRenderer;
    public Shader Shader, AssistShader;
    public Texture EvilTexture, NeutralTexture;

    internal List<SphereScript> Spheres { get; set; }
    internal Rotation[][][] Rotations { get; private set; }
    internal Dictionary<Axis, bool> AnchorSphere { get; private set; }

    internal bool isRotating, isSubmitting, isActive, isStarting, isUsingBounce, isUsingElastic;
    internal int Dimension { get { return _dimension; } set { if (_dimension == 0) _dimension = Mathf.Clamp(value, ModSettingsJSON.Min, ModSettingsJSON.Max); } }
    internal int GetLastDigitOfTimer { get { return (int)GetPreciseLastDigitOfTimer; } }
    internal float GetPreciseLastDigitOfTimer { get { return Info.GetTime() % (Dimension > 10 ? 20 : 10); } }
    internal float rotationProgress;
    internal static readonly string allCharAxes = Enum.GetValues(typeof(Axis)).Cast<Axis>().Join("");
    internal Dictionary<Axis, bool> startingSphere = new Dictionary<Axis, bool>();
    internal static IEnumerable<Axis> allAxes = Enum.GetValues(typeof(Axis)).Cast<Axis>();

    private int _moduleId, _breakCount, _dimension;
    private float _speed;
    private Axis[] _order;
    private List<Axis> _inputs;
    private Dictionary<Axis, int> _axesUsed = new Dictionary<Axis, int>();
    private Animate _animate;
    private TheOctadecayottonScript _octadecayotton;

    internal KMSelectable.OnInteractHandler Init(TheOctadecayottonScript octadecayotton, bool checkForTP, int dimension)
    {
        return () =>
        {
            if (isStarting || isActive || octadecayotton.IsSolved || (checkForTP && octadecayotton.TwitchPlaysActive))
                return true;

            isStarting = true;
            rotationProgress = 0;

            _octadecayotton = octadecayotton;
            _animate = new Animate(this, _octadecayotton);
            _moduleId = octadecayotton.moduleId;
            isUsingBounce = octadecayotton.isUsingBounce;
            isUsingElastic = octadecayotton.isUsingElastic;

            _speed = 1 / Mathf.Pow(2, -octadecayotton.slowness);

            if (octadecayotton.SlownessOverride != default(byte))
                _speed = 1 / Mathf.Pow(2, -octadecayotton.SlownessOverride);

            if (octadecayotton.DimensionOverride != default(byte))
                octadecayotton.dimensionOverride = octadecayotton.DimensionOverride;

            if (Dimension == 0)
                TheOctadecayottonScript.Activated++;

            Dimension = _octadecayotton.dimensionOverride == 0 ? dimension + TheOctadecayottonScript.Activated : octadecayotton.dimensionOverride;

            StartCoroutine(_animate.CreateHypercube(Dimension));

            octadecayotton.PlaySound(Dimension > 9 ? SFX.Oct.StartupHard : SFX.Oct.Startup);
            octadecayotton.ModuleSelectable.AddInteractionPunch(Dimension > 9 ? 64 : 32);

            Rotations = !Application.isEditor || octadecayotton.ForceRotation.IsNullOrEmpty()
                      ? TheOctadecayottonExtensions.GetRandomRotations(new RotationOptions(dimension: Dimension, rotationCount: octadecayotton.RotationOverride != default(byte) ? octadecayotton.RotationOverride : octadecayotton.rotation))
                      : octadecayotton.ForceRotation.ToRotations();

            Debug.LogFormat("[The Octadecayotton #{0}]: Initializing with {1} dimensions and {2} rotation{3}.",
                _moduleId,
                Dimension,
                octadecayotton.rotation,
                octadecayotton.rotation == 1 ? "" : "s");
            Debug.LogFormat("[The Octadecayotton #{0}]: NOTE: Rotations are cyclic, meaning that +X-Y+Z is the same as -Y+Z+X and +Z+X-Y! Commas (,) separate different subrotations, and ampersands (&) separate different rotations.", _moduleId);
            Debug.LogFormat("[The Octadecayotton #{0}]: The rotation{1} {2}.",
                _moduleId,
                octadecayotton.rotation == 1 ? " is" : "s are",
                Rotations.ToLog());

            AnchorSphere = Rotations.Get(Dimension, _moduleId);
            Debug.LogFormat("[The Octadecayotton #{0}]: The anchor sphere is in {1}. ({2}-ordered)",
                _moduleId,
                AnchorSphere.Select(a => a.Value ? "+" : "-").Join(""),
                allCharAxes.Substring(0, Dimension));

            CreateStartingSphere();
            Debug.LogFormat("[The Octadecayotton #{0}]: To solve this module, press anywhere to enter submission, submit the digits from left-to-right when the {1} matches the digit shown, then submit on every digit from {2} down to 0.",
                _moduleId,
                Dimension > 10 ? "timer modulo 20" : "last digit of the timer",
                Dimension - 1);
            Debug.LogFormat("[The Octadecayotton #{0}]: Example full solution (not knowing axes) => {1}.",
                _moduleId,
                startingSphere.GetAnswer(AnchorSphere, _axesUsed, _order, true).Select(i => i.Join(Dimension > 9 ? " " : "")).Join(", "));
            Debug.LogFormat("[The Octadecayotton #{0}]: Quickest solution (knowing axes) => {1}.",
                _moduleId,
                startingSphere.GetAnswer(AnchorSphere, _axesUsed, _order, false).Select(i => i.Join(Dimension > 9 ? " " : "")).Join(", "));

            return true;
        };
    }

    internal KMSelectable.OnInteractHandler OnInteract(TheOctadecayottonScript octadecayotton, bool checkForTP, int dimension)
    {
        return Init(octadecayotton, checkForTP, dimension) + (() =>
        {
            _octadecayotton.ModuleSelectable.AddInteractionPunch();
            _octadecayotton.PlaySound(SFX.Oct.InteractInterrupt);
            if (!isActive || _octadecayotton.IsSolved)
                return false;
            if (isRotating)
                isSubmitting = true;
            return HandleSubmission();
        });
    }

    internal int[][] GetAnswer(bool flip)
    {
        List<int[]> temp = startingSphere.GetAnswer(AnchorSphere, _axesUsed, _order, false).ToList();
        temp.Add(Enumerable.Range(0, Dimension).Reverse().ToArray());
        return flip ? temp.Select(c => c.Reverse().ToArray()).ToArray() : temp.ToArray();
    }

    private void Update()
    {
        // This is 
        Shader.SetGlobalMatrix("_W2L", transform.worldToLocalMatrix);
    }

    private void FixedUpdate()
    {
        if (_octadecayotton == null || _octadecayotton.IsSolved)
            return;

        if (isSubmitting && !isRotating && _inputs.Count != 0 && 
           ((_octadecayotton.ZenModeActive && GetPreciseLastDigitOfTimer > (Dimension > 10 ? 19.5f : 9.5f) && GetPreciseLastDigitOfTimer < (Dimension > 10 ? 19.75f : 9.75f)) ||
           (!_octadecayotton.ZenModeActive && GetPreciseLastDigitOfTimer < (Dimension > 10 ? 19.5f : 9.5f) && GetPreciseLastDigitOfTimer > (Dimension > 10 ? 19.25f : 9.25f))))
        {
            if (!_inputs.Validate(startingSphere, AnchorSphere, _axesUsed, _order, ref _breakCount, Dimension, ref _moduleId))
                StartCoroutine(_animate.Strike());

            else if (_inputs.Count == Dimension)
                StartCoroutine(_animate.Solve());

            else if (_inputs.Count == (Dimension == 3 ? 1 : 3))
                _octadecayotton.PlaySound(SFX.Oct.StartingSphere);

            for (int i = 0; i < Spheres.Count; i++)
                Spheres[i].StartCoroutine(Spheres[i].UpdateValue());

            _inputs = new List<Axis>();
        }

        if (!isActive || !isRotating)
            return;

        if (rotationProgress >= Rotations.Length + 0.25f || (Rotations.Length == 1 && rotationProgress > 1))
        {
            rotationProgress = 0;
        }

        if (rotationProgress % 1 == 0)
        {
            if (isSubmitting)
            {
                _inputs = new List<Axis>();
                _octadecayotton.PlaySound(SFX.Oct.StartingSphere);
                for (int i = 0; i < Spheres.Count; i++)
                    Spheres[i].StartCoroutine(Spheres[i].UpdateValue());
                isRotating = false;
            }

            if (Rotations.Length != 0)
                for (int i = 0; i < Spheres.Count; i++)
                {
                    if (rotationProgress == 0)
                        Spheres[i].pos.newVectorReady = false;
                    Spheres[i].pos.SetRotation(rotationProgress < Rotations.Length ? Rotations[(int)rotationProgress] : Rotations[0]);
                }
        }

        if (rotationProgress < Rotations.Length)
            foreach (var sphere in Spheres)
                sphere.gameObject.transform.localPosition = sphere.pos.MergeDimensions(isUsingBounce ? (rotationProgress % 1).InOutBounce() : isUsingElastic ? (rotationProgress % 1).ElasticInOut() : Easing.InOutCubic(rotationProgress % 1, 0, 1, 1));

        rotationProgress += _speed;
    }

    private void CreateStartingSphere()
    {
        do
        {
            _breakCount = 0;
            _inputs = new List<Axis>();
            startingSphere = new Dictionary<Axis, bool>();
            _axesUsed = new Dictionary<Axis, int>();

            for (int i = 0; i < Dimension; i++)
            {
                startingSphere.Add(allAxes.ElementAt(i),
                    !Application.isEditor || _octadecayotton.ForceStartingSphere.IsNullOrEmpty()
                    ? Rnd.Range(0, 1f) > 0.5f
                    : _octadecayotton.ForceStartingSphere.Where(c => c == '-' || c == '+').ElementAtOrDefault(i) == '+');
                _axesUsed.Add(allAxes.ElementAt(i), 0);
            }
        } while (startingSphere.Select((a, n) => a.Value != AnchorSphere.ElementAt(n).Value).All(b => !b));

        _order = allAxes.Take(Dimension).ToArray().Shuffle().ToArray();
        Debug.LogFormat("[The Octadecayotton #{0}]: The axes (from 0 to {1}) for the last digits of the timer is {2}.",
            _moduleId,
            Dimension - 1,
            _order.Join(""));
        Debug.LogFormat("[The Octadecayotton #{0}]: The starting sphere is in {1}. ({2}-ordered)",
            _moduleId,
            startingSphere.Select(a => a.Value ? "+" : "-").Join(""),
            allCharAxes.Substring(0, Dimension));
        _octadecayotton.souvenirRotations = Rotations.ToLog();
        _octadecayotton.souvenirSphere = startingSphere.ToLog();
    }

    private bool HandleSubmission()
    {
        if (GetLastDigitOfTimer >= Dimension || !isSubmitting || isStarting)
        {
            if (!isSubmitting)
                _inputs = new List<Axis>();
            return false;
        }

        _octadecayotton.PlaySound(SFX.Oct.Interact);

        if (!_inputs.Contains((Axis)GetLastDigitOfTimer))
            _inputs.Add((Axis)GetLastDigitOfTimer);

        for (int i = 0; i < Spheres.Count && !isRotating; i++)
        {
            if (Spheres[i] == null)
                continue;
            Spheres[i].StartCoroutine(Spheres[i].UpdateValue());
        }

        return false;
    }
}
