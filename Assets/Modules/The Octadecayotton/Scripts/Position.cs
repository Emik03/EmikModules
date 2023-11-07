using System;
using System.Linq;
using UnityEngine;

namespace TheOctadecayotton
{
    internal static class Position
    {
        internal static readonly float[,] weights =
        {
            // X = 0, Y = 1, Z = 2
            { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 },
            // W = 3, V = 4, U = 5
            { 0.8f, 0.2f, 0.5f }, { 0.2f, 0.5f, 0.8f }, { 0.5f, 0.8f, 0.2f },
            // R = 6, S = 7, T = 8
            { 2, 0.125f, 0.125f }, { 0.125f, 0.125f, 2, }, { 0.125f, 2, 0.125f },
            // O = 9, P = 10, Q = 11
            { 0.125f, 0.05f, 3.2f }, { 0.05f, 3.2f, 0.125f, }, { 3.2f, 0.125f, 0.05f },
            // THESE DIMENSIONS ARE UNUSED; THIS IS A PROOF OF CONCEPT
            // L = 12, M = 13, N = 14
            { 4, 2, 1 }, { 1, 4, 2, }, { 2, 1, 4 },
            // I = 15, J = 16, K = 17
            { 6.9f, 4.2f, 1.9f }, { 1.9f, 6.9f, 4.2f, }, { 4.2f, 1.9f, 6.9f },

            // these dimensions are temporary
            { 19f, 0f, 0f }, { 0f, 19f, 0f }, { 0f, 0f, 19f },
            { 15.2f, 3.8f, 9.5f }, { 3.8f, 9.5f, 15.2f }, { 9.5f, 15.2f, 3.8f },
            { 38, 2.375f, 2.375f }, { 2.375f, 2.375f, 38 }, { 2.375f, 38, 2.375f },
            { 2.375f, 0.95f, 60.8f }, { 0.95f, 60.8f, 2.375f }, { 60.8f, 2.375f, 0.95f },
        };
    }
}
