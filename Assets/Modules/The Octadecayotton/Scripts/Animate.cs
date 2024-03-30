using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

namespace TheOctadecayotton
{
    internal class Animate
    {
        internal Animate(InteractScript interact, TheOctadecayottonScript octadecayotton)
        {
            _interact = interact;
            _octadecayotton = octadecayotton;
        }

        private readonly InteractScript _interact;
        private readonly TheOctadecayottonScript _octadecayotton;
        private OctadecayottonHypercube _hypercube;
        private bool _isFinished;
        private int _dimension;

        internal IEnumerator CreateHypercube(int dimension, OctadecayottonHypercube hypercube, Coroutine spawnCube)
        {
            _dimension = dimension;
            _hypercube = hypercube;

            const float maxJitter = 1f / 6f;

            float duration = dimension > 9 ? 11f : 3f;
            float t = Time.time;
            Vector3 scale = Vector3.one * (dimension > 18 ? 3 : 1);
            hypercube.transform.parent.localPosition = new Vector3(0.5f, dimension > 18 ? 1.5f : .5f, 0.5f);
            while (Time.time - t < duration)
            {
                float progress = (Time.time - t) / duration;
                byte b = (byte)((1 - progress) * 255);
                _interact.ModuleRenderer.material.color = new Color32(b, b, b, 255);
                hypercube.transform.parent.localScale = scale * ((progress / 2f + 0.5f).ElasticInOut() * 2 - 1);
                hypercube.SetJitter((1f - progress) * maxJitter);
                yield return null;
            }
            hypercube.transform.parent.localScale = scale;
            hypercube.SetJitter();

            _interact.ModuleRenderer.material.mainTexture = _interact.EvilTexture;

            for (byte i = 5; i >= 5; i += 5)
            {
                _interact.ModuleRenderer.material.color = new Color32(i, i, i, 255);
                yield return new WaitForSecondsRealtime(0.02f);
            }

            yield return spawnCube;

            _interact.isRotating = !(_interact.isSubmitting = _interact.Rotations.Length == 0);
            _interact.isActive = true;
            _interact.isStarting = false;

            if (_interact.isSubmitting)
            {
                _octadecayotton.PlaySound(SFX.Oct.StartingSphere);
                _interact.StartCoroutine(_interact.FadeToSubmission(spawnStart: true));
            }

            //yield return Solve();
        }

        internal IEnumerator DestroyHypercube()
        {
            float t = Time.time;
            var sphereCount = 1 << _dimension;
            float duration = sphereCount / Mathf.Pow(2f, Mathf.Max(_interact.Dimension - 7f, 0f)) *
                0.02f * Mathf.Pow(2f, Mathf.Max(9f - _interact.Dimension, 0f));
            while (Time.time - t < duration)
            {
                _hypercube.DisableSpheres((uint)(sphereCount * (Time.time - t) / duration));
                yield return null;
            }
            _hypercube.DisableSpheres((uint)sphereCount);
            _hypercube.Cleanup();

            _isFinished = true;
        }

        internal IEnumerator Solve()
        {
            _octadecayotton.IsSolved = true;

            _interact.isActive = false;
            _interact.isRotating = false;
            _interact.isSubmitting = false;

            yield return new WaitForSecondsRealtime(1);

            Debug.LogFormat("[The Octadecayotton #{0}]: Module disarmed.", _octadecayotton.moduleId);
            _octadecayotton.PlaySound(_interact.Dimension > 15 ? SFX.Oct.SolveHarder : _interact.Dimension > 9 ? SFX.Oct.SolveHard : SFX.Oct.Solve);

            _hypercube.ForceColor(Color.white);
            _hypercube.UnhighlightSpheres();

            bool shortTime = false;
            if (_octadecayotton.Info.GetTime() < 60)
            {
                shortTime = true;
                _interact.StartCoroutine(DelayedSolve());
            }

            yield return ExpandSpheres(-4, 128f);
            yield return ShuffleSpheres(512f);

            if (!shortTime)
                _octadecayotton.Module.HandlePass();

            yield return DestroyHypercube();
        }

        private IEnumerator DelayedSolve()
        {
            yield return new WaitForSecondsRealtime(1);
            _octadecayotton.Module.HandlePass();
        }

        internal IEnumerator ExpandSpheres(int amp, float time)
        {
            var duration = time * 0.02f;
            float t = Time.time;
            var startScale = _hypercube.transform.parent.localScale;
            while (Time.time - t < duration)
            {
                _hypercube.transform.parent.localScale = startScale * Easing.InOutCubic(Time.time - t, 1, amp, duration);
                yield return null;
            }
            _hypercube.transform.parent.localScale = Vector3.one * amp;
        }

        internal IEnumerator ShuffleSpheres(float time)
        {
            var duration = time * 0.02f;
            _hypercube.SetSolving(true);
            float t = Time.time;
            while (Time.time - t < duration)
            {
                _hypercube.SetRotationProgress(Easing.InOutCubic(Time.time - t, 0, 1, duration));
                yield return null;
            }

            _hypercube.SetRotationProgress(1f);
        }

        internal IEnumerator Strike()
        {
            _interact.isSubmitting = false;
            _interact.isRotating = false;

            _octadecayotton.PlaySound(SFX.Oct.Strike);
            Debug.LogFormat("[The Octadecayotton #{0}]: Incorrect submission, strike and reset!", _octadecayotton.moduleId);

            _interact.ModuleRenderer.material.mainTexture = null;
            _hypercube.UnhighlightSpheres();

            for (int i = 0; i < 2; i++)
            {
                _interact.ModuleRenderer.material.color = i % 2 == 0 ? Color.white : Color.black;
                _hypercube.ForceColor(i % 2 == 0 ? Color.black : Color.white);
                yield return new WaitForSecondsRealtime(0.2f);
            }

            _interact.ModuleRenderer.material.color = Color.white;
            _interact.ModuleRenderer.material.mainTexture = _interact.NeutralTexture;

            _isFinished = false;
            _interact.StartCoroutine(DestroyHypercube());
            yield return ExpandSpheres(0, 512f);
            yield return new WaitUntil(() => _isFinished);

            _octadecayotton.Module.HandleStrike();
            _interact.isActive = false;
        }
    }
}
