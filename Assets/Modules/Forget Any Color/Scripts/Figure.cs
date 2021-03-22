using System.Diagnostics;
using System.Linq;

namespace ForgetAnyColor
{
    /// <summary>
    /// Handles initalization and modification of cylinder arrays, also known as 'Figures'.
    /// </summary>
    static class Figure
    {
        internal static int[] Apply(int[][] cylinders, FACScript FAC)
        {
            int[] cylinderValues = new int[3];
            int[] sumValues = new int[3];

            for (int i = 0; i < cylinderValues.Length; i++)
            {
                cylinderValues[i] = Init.rules.GetLength(0) != 0 ? Init.rules[0][(i * 8) + Functions.GetColorIndex(i, FAC)].Number 
                                                                 : Arrays.ColorTable[i, Functions.GetColorIndex(i, FAC)];

                for (int j = 0; j < cylinders[i].Length; j++)
                    cylinders[i][j] += cylinderValues[i];

                sumValues[i] = cylinders[i].Sum() % 10;
            }

            return sumValues;
        }

        internal static int[][] Create(int[] decimals, ref int figure)
        {
            int[][] cylinders = new int[3][];

            switch (figure)
            {
                case 0:
                    cylinders[0] = new[] { decimals[0], decimals[1], decimals[2] };
                    cylinders[1] = new[] { decimals[3] };
                    cylinders[2] = new[] { decimals[4] };
                    break;

                case 1:
                    cylinders[0] = new[] { decimals[0] };
                    cylinders[1] = new[] { decimals[1], decimals[2], decimals[3] };
                    cylinders[2] = new[] { decimals[4] };
                    break;

                case 2:
                    cylinders[0] = new[] { decimals[0] };
                    cylinders[1] = new[] { decimals[1] };
                    cylinders[2] = new[] { decimals[2], decimals[3], decimals[4] };
                    break;

                case 3:
                    cylinders[0] = new[] { decimals[0] };
                    cylinders[1] = new[] { decimals[1], decimals[2] };
                    cylinders[2] = new[] { decimals[3], decimals[4] };
                    break;

                case 4:
                    cylinders[0] = new[] { decimals[0], decimals[1] };
                    cylinders[1] = new[] { decimals[2] };
                    cylinders[2] = new[] { decimals[3], decimals[4] };
                    break;

                case 5:
                    cylinders[0] = new[] { decimals[0], decimals[1] };
                    cylinders[1] = new[] { decimals[2], decimals[3] };
                    cylinders[2] = new[] { decimals[4] };
                    break;
            }

            return cylinders;
        }
    }
}
