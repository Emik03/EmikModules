namespace TheOctadecayotton
{
    internal class Rotation
    {
        internal Rotation(bool isNegative, Axis axis)
        {
            IsNegative = isNegative;
            Axis = axis;
        }

        internal bool IsNegative { get; set; }
        internal Axis Axis { get; private set; }
    }
}
