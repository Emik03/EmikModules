namespace TheOctadecayotton
{
    internal class RotationOptions
    {
        internal RotationOptions(int dimension = 9, int rotationCount = 3, int minRotations = 2, int maxRotations = 5, int minLengthPerRotation = 1, int maxLengthPerRotation = 5, float chanceToRepeat = 0.66874f, float chanceForNegativeRotation = 0.5f)
        {
            Dimension = dimension;
            RotationCount = rotationCount;
            MinRotations = minRotations;
            MaxRotations = maxRotations;
            MinLengthPerRotation = minLengthPerRotation;
            MaxLengthPerRotation = maxLengthPerRotation;
            ChanceToRepeat = chanceToRepeat;
            ChanceForNegativeRotation = chanceForNegativeRotation;
        }

        internal int Dimension { get; private set; }
        internal int RotationCount { get; private set; }
        internal int MinRotations { get; private set; }
        internal int MaxRotations { get; private set; }
        internal int MinLengthPerRotation { get; private set; }
        internal int MaxLengthPerRotation { get; private set; }
        internal float ChanceToRepeat { get; private set; }
        internal float ChanceForNegativeRotation { get; private set; }
    }
}
