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

        [Range(5, Max), SerializeField]
        private byte _length;

        [Range(1, Max), SerializeField]
        private byte _mate;

        [Range(1, 3), SerializeField]
        private byte _blackPieces, _whitePieces;

        [SerializeField]
        private string _position;
#pragma warning restore 649, IDE0044 // Add readonly modifier

        internal const int Min = 8, Max = 11;

        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; } }

        public byte Length { get { return _length; } }
        
        public byte Mate { get { return _mate; } }

        public byte BlackPieces { get { return _blackPieces; } }

        public byte WhitePieces { get { return _whitePieces; } }

        public string Position { get { return _position; } }
    }
}
