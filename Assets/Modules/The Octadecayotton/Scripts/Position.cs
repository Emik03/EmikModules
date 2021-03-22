using System;
using System.Linq;
using UnityEngine;

namespace TheOctadecayotton
{
    internal class Position
    {
        internal Position(float[] deviations, bool stretchToFit)
        {
            const float MaxDeviation = 0.00001f;
            _deviations = deviations.Select(f => f * MaxDeviation).ToArray();
            _stretchToFit = stretchToFit;
        }

        internal float[] Dimensions
        {
            get
            {
                return _dimensions;
            }
            set
            {
                if (value.Any(f => f < 0 || f > 1))
                    throw new ArgumentOutOfRangeException(value.Join(", "));
                _dimensions = value;
            }
        }

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
            { .5f, .5f, .5f }
        };

        internal bool[] InitialPosition { get; private set; }
        internal bool[] NewPosition { get; private set; }

        internal bool newVectorReady;

        private bool _stretchToFit;
        private readonly float[] _deviations;
        private float[] _dimensions;
        private Vector3 _initialVector, _newVector;

        internal Vector3 MergeDimensions(float f = 0.5f)
        {
            float negF = 1 - f;
            return new Vector3(
                _initialVector.x * negF + _newVector.x * f + _deviations[0],
                _initialVector.y * negF + _newVector.y * f + _deviations[1],
                _initialVector.z * negF + _newVector.z * f + _deviations[2]);
        }

        internal Vector3 MergeDimensions(Vector3 otherVector, float f = 0.5f)
        {
            float negF = 1 - f;
            return new Vector3(
                _initialVector.x * negF + otherVector.x * f + _deviations[0],
                _initialVector.y * negF + otherVector.y * f + _deviations[1],
                _initialVector.z * negF + otherVector.z * f + _deviations[2]);
        }

        internal void SetDimensions(float[] vs)
        {
            if (vs.Length > weights.GetLength(0))
                throw new ArgumentException("Cannot render a dimension higher than " + weights.GetLength(0) + ": " + vs.Length);

            Dimensions = new float[vs.Length];
            InitialPosition = new bool[vs.Length];

            for (int i = 0; i < vs.Length; i++)
                Dimensions[i] = vs[i];

            for (int i = 0; i < InitialPosition.Length; i++)
                InitialPosition[i] = vs[i] != 0;

            NewPosition = new bool[InitialPosition.Length];
            _initialVector = InitialPosition.ToVector3(_dimensions.Length, _stretchToFit);
        }

        internal void SetRotation(Rotation[][] rotations)
        {
            //if (_dimensions.Length < 3)
            //    throw new IndexOutOfRangeException("dimensions.Length: " + _dimensions.Length);
            //if (rotations.Any(i => i.Any(j => j.Axis < 0 || (int)j.Axis >= _dimensions.Length)))
            //    throw new IndexOutOfRangeException("axis: " + rotations.Select(i => i.Select(j => j.IsNegative ? "-" : "+" + j.Axis).Join("")).Join(", "));

            for (int i = 0; i < NewPosition.Length; i++)
                if (newVectorReady)
                    InitialPosition[i] = NewPosition[i];
                else
                    NewPosition[i] = InitialPosition[i];

            if (rotations.Any(a => a.Length == 0))
                return;

            for (int i = 0; i < rotations.Length; i++)
            {
                bool temp = InitialPosition[(int)rotations[i][0].Axis];
                for (int j = 0; j < rotations[i].Length - 1; j++)
                    NewPosition[(int)rotations[i][j].Axis] = rotations[i][j].IsNegative ^ InitialPosition[(int)rotations[i][j + 1].Axis];
                NewPosition[(int)rotations[i][rotations[i].Length - 1].Axis] = rotations[i][rotations[i].Length - 1].IsNegative ^ temp;
            }

            _initialVector = InitialPosition.ToVector3(_dimensions.Length, _stretchToFit);
            _newVector = NewPosition.ToVector3(_dimensions.Length, _stretchToFit);
            newVectorReady = true;
        }
    }
}
