using KeepCoding;
using Scale;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScaleScript : ModuleScript
{
    [SerializeField]
    internal KMSelectable selectable;

    [SerializeField]
    private float _alpha, _speed;

    [SerializeField]
    private Renderer _point;

    [SerializeField]
    private Transform _parent;

    internal bool IsCorrect
    {
        get
        {
            return CurrentAxes.SequenceEqual(ScaleMath.Axes[Mathf.FloorToInt(_answer)]) ||
                CurrentAxes.SequenceEqual(ScaleMath.Axes[Mathf.CeilToInt(_answer)]);
        }
    }

    private int RandomNinety { get { return Random.Range(0, 4) * 90; } }
    private Axis[] CurrentAxes { get { return _axes[(int)_progress]; } }
    private Vector3 RandomVector { get { return new Vector3(RandomNinety, RandomNinety, RandomNinety); } }

    private const int Dimension = 4, CubeLength = 16;
    private const float Big = 3 / 4f, Small = Big / 3;

    private float _answer, _progress;
    private float[][] _state;
    private Axis[][] _axes;
    private Renderer[] _cubes;

    public override void OnActivate()
    {
        _cubes = CubeLength.For(i =>
        {
            var cube = Instantiate(_point, _parent, false);
            cube.transform.localEulerAngles = RandomVector;
            return cube;
        }).ToArray();

        _cubes.Take(Random.Range(0, 10)).ForEach(r => r.enabled = false);

        _axes = ScaleMath.Axes.Shuffle();

        int cubeCount = _cubes.Count(r => r.enabled);

        _answer = cubeCount % _axes.Length;

        Log("There are {0} cubes, making the answer {1}.", cubeCount, ScaleMath.Axes[(int)_answer]);

        _state = Enumerable.Range(0, (int)Mathf.Pow(2, Dimension))
            .Select(i => ((long)i).LongToBase(2)
                .PadLeft(Dimension, '0')
                .Select(c => c == '0' ? Small : Big)
                .ToArray())
            .ToArray();

        StartCoroutine(RenderCubes());
    }

    public override void OnDestruct()
    {
        Get<KMGameInfo>().OnLightsChange -= HandleLightsChange;
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void OnValidate()
#pragma warning restore IDE0051 // Remove unused private members
    {
        _alpha = Mathf.Abs(_alpha);
        _speed = Mathf.Abs(_speed);
    }

    private IEnumerator RenderCubes()
    {
        yield return null;

        Get<KMGameInfo>().OnLightsChange += HandleLightsChange;

        selectable.Assign(onInteract: () =>
        {
            if (IsCorrect)
            {
                Solve("The cube was pressed at the correct time. Solved!");
                return;
            }

            Strike("The submission {0} was made, which is incorrect. Strike!".Form(CurrentAxes));
        });

        int current = 0;

        while (true)
        {
            _progress += Time.deltaTime * _speed;

            if (current < Mathf.FloorToInt(_progress))
            {
                current = Mathf.FloorToInt(_progress %= _axes.Length);

                _state = ScaleMath.Apply(
                    _state,
                    _axes.ElementAtWrap((int)_progress - 1),
                    Big,
                    Small);

                if (current == 0)
                    _axes = ScaleMath.Axes.Shuffle();
            }

            float[][] scales = ScaleMath.Combine(
                _state,
                ScaleMath.Apply(_state, CurrentAxes, Big, Small),
                Easing.InOutQuad(_progress % 1, 0, 1, 1),
                CubeLength,
                Dimension);

            _cubes.Call((r, i) =>
            {
                float[] scale = scales[_cubes.IndexOf(c => c == r)];

                r.transform.localScale = new Vector3(
                    scale[0] + scale[3] * 2,
                    scale[2] + scale[3] * 2,
                    scale[1] + scale[3] * 2);

                r.material.color = new Color(
                    scale[0] / Big,
                    scale[2] / Big,
                    scale[1] / Big,
                    _alpha);
            });

            yield return null;
        }
    }

    private void HandleLightsChange(bool b)
    {
        _answer = (_answer + 0.5f) % 12;

        Log(b
            ? "The lights have turned on. The previous answer and new answer {0} are correct.".Form(ScaleMath.Axes[Mathf.CeilToInt(_answer)])
            : "The lights have turned off. The previous answer is now invalid.");
    }
}
