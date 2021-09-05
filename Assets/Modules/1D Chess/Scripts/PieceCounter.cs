using System;

namespace OneDimensionalChess
{
    internal struct PieceCounter
    {
        private readonly int _black, _white;

        internal PieceCounter(Random random, int length)
        {
            _black = random.Next(2, length / 2);
            _white = random.Next(2, length / 2);
        }

        internal int Black { get { return _black; } }

        internal int White { get { return _white; } }
    }
}
