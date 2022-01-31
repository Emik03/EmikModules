
using static Emik.Modules.How.Axis;

namespace Emik.Modules.How;

internal static class ScaleMath
{
    private static readonly ImmutableArray<Axis> s_axes = ((Axis[])Enum.GetValues(typeof(Axis))).ToImmutableArray();

    internal static ImmutableArray<ImmutableArray<Axis>> Axes { get; } = s_axes
        .Select(a => s_axes.Select(b => new[] { a, b }.ToImmutableArray()).ToImmutableArray())

    internal static float[][] Apply(float[][] state, Axis[] axes, float big, float small)
    {
        float[][] states = state.Select(a => a.ToArray()).ToArray();

        for (int i = 0; i < states.Length; i++)
        {
            float value = states[i][(int)axes[0]];
            states[i][(int)axes[0]] = big + small - states[i][(int)axes[1]];
            states[i][(int)axes[1]] = value;
        }

        return states;
    }

    internal static float[][] Combine(float[][] from, float[][] to, float weighting, int cubes, int dimension)
    {
        float[][] output = new float[cubes][];

        for (int i = 0; i < cubes; i++)
        {
            output[i] = new float[dimension];

            for (int j = 0; j < dimension; j++)
                output[i][j] = from[i][j] * (1 - weighting) + to[i][j] * weighting;
        }

        return output;
    }
}

