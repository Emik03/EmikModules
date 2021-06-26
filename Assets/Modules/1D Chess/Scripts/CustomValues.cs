using System;
using UnityEngine;

namespace OneDimensionalChess
{
    [Serializable]
    public class CustomValues
    {
        public bool IsEnabled;
        [Range(6, 9)]
        public byte Length;
        public string Position;
    }
}
