﻿using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FloatingScript : MonoBehaviour 
{
    [Range(1 / 32f, 32f)]
    [SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
    private float _duration = 1, _movementIntensity = 1, _rotationIntensity = 1;
#pragma warning restore IDE0044 // Add readonly modifier

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.localPosition;

        StartCoroutine(ChangeTarget());
    }

    private IEnumerator ChangeTarget()
    {
        while (true)
        {
            var initialRotation = transform.localRotation;
            var initialPosition = transform.localPosition;

            var targetRotation = Quaternion.Euler(NextTarget(_rotationIntensity));
            var targetPosition = _startPosition + NextTarget(_movementIntensity / 100);

            float f = 0;

            while (f < _duration)
            {
                f += Time.deltaTime;

                float ease = Easing.InOutQuad(f, 0, 1, _duration);

                transform.localRotation = Quaternion.Lerp(initialRotation, targetRotation, ease);
                transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, ease);

                yield return null;
            }
        }
    }

    private static float Next(float f)
    {
        return Random.Range(-f, f);
    }

    private static Vector3 NextTarget(float f)
    {
        return new Vector3(Next(f), Next(f), Next(f));
    }
}
