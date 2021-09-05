using System;
using UnityEngine;

namespace OneDimensionalChess
{
    [Serializable]
    public class CustomValues
    {
#pragma warning disable 649, IDE0044 // Add readonly modifier
        [SerializeField]
        private bool _isEnabled;

        [Range(Min, Max), SerializeField]
        private byte _length;

        [Range(1, 4), SerializeField]
        private byte _blackPieces, _whitePieces;

        [SerializeField]
        private string _position;
#pragma warning restore 649, IDE0044 // Add readonly modifier

        internal const int Min = 8, Max = 11;

        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; } }

        public byte Length { get { return _length; } }

        public byte BlackPieces { get { return _blackPieces; } }

        public byte WhitePieces { get { return _whitePieces; } }

        public string Position { get { return _position; } }
    }
}
