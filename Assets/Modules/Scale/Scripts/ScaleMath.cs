using System.Linq;

namespace Scale
{
    internal static class ScaleMath
    {
        internal static Axis[][] Axes 
        { 
            get 
            { 
                return new[] 
                {
                    new[] { Axis.X, Axis.Y },
                    new[] { Axis.X, Axis.Z },
                    new[] { Axis.X, Axis.W }, 
                    new[] { Axis.Y, Axis.Z }, 
                    new[] { Axis.Y, Axis.W }, 
                    new[] { Axis.Z, Axis.W }, 
                    new[] { Axis.Y, Axis.X }, 
                    new[] { Axis.Z, Axis.X }, 
                    new[] { Axis.W, Axis.X }, 
                    new[] { Axis.Z, Axis.Y }, 
                    new[] { Axis.W, Axis.Y }, 
                    new[] { Axis.W, Axis.Z } 
                };
            } 
        }

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
}

