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

            float duration = dimension > 15 ? 5f : dimension > 9 ? 11f : 3f;
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
            if (_dimension > 15)
                duration *= 4;
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

            if (_interact.Dimension > 15)
            {
                yield return ExpandSpheres(_interact.Dimension > 18 ? -2 : -4, 2.56f);
                yield return ShuffleSpheres(10.24f);
                yield return ShuffleSpheres(10.24f);
                yield return ShuffleSpheres(10.24f);
                yield return ExpandSpheres(-2, 2.56f);
                var shuffle = _interact.StartCoroutine(ShuffleSpheres(40.96f));
                yield return new WaitForSeconds(5f);
                yield return ExpandSpheres(0.75f, 30f);
                yield return shuffle;
            }
            else
            {
                yield return ExpandSpheres(-4, 2.56f);
                yield return ShuffleSpheres(10.24f);
            }

            if (!shortTime)
                _octadecayotton.Module.HandlePass();

            yield return DestroyHypercube();
        }

        private IEnumerator DelayedSolve()
        {
            yield return new WaitForSecondsRealtime(1);
            _octadecayotton.Module.HandlePass();
        }

        internal IEnumerator ExpandSpheres(float amp, float time)
        {
            float t = Time.time;
            var startScale = _hypercube.transform.parent.localScale;
            while (Time.time - t < time)
            {
                _hypercube.transform.parent.localScale = startScale * Easing.InOutCubic(Time.time - t, 1, amp, time);
                yield return null;
            }
            _hypercube.transform.parent.localScale = startScale * amp;
        }

        internal IEnumerator ShuffleSpheres(float time)
        {
            int[] primes = new int[]
            {
                10000019, 10000079, 10000103, 10000121, 10000139, 10000141, 10000169, 10000189, 10000223, 10000229,
                10000247, 10000253, 10000261, 10000271, 10000303, 10000339, 10000349, 10000357, 10000363, 10000379,
                10000439, 10000451, 10000453, 10000457, 10000481, 10000511, 10000537, 10000583, 10000591, 10000609,
                10000643, 10000651, 10000657, 10000667, 10000687, 10000691, 10000721, 10000723, 10000733, 10000741,
                10000747, 10000759, 10000763, 10000769, 10000789, 10000799, 10000813, 10000819, 10000831, 10000849,
                10000867, 10000871, 10000873, 10000877, 10000891, 10000931, 10000943, 10000961, 10000967, 10000987,
                10000993, 10001009, 10001053, 10001081, 10001093, 10001107, 10001119, 10001203, 10001207, 10001209,
                10001213, 10001221, 10001227, 10001231, 10001237, 10001261, 10001269, 10001281, 10001311, 10001347,
                10001357, 10001363, 10001399, 10001401, 10001419, 10001441, 10001443, 10001461, 10001473, 10001483,
                10001501, 10001521, 10001531, 10001533, 10001567, 10001569, 10001587, 10001603, 10001617, 10001659,
                10001687, 10001701, 10001707, 10001713, 10001759, 10001777, 10001779, 10001791, 10001801, 10001807,
                10001813, 10001819, 10001821, 10001833, 10001837, 10001861, 10001881, 10001891, 10001903, 10001921,
                10001963, 10002007, 10002017, 10002019, 10002029, 10002053, 10002059, 10002061, 10002067, 10002077,
                10002121, 10002127, 10002133, 10002149, 10002191, 10002197, 10002199, 10002203, 10002257, 10002259,
                10002277, 10002283, 10002287, 10002323, 10002331, 10002347, 10002397, 10002403, 10002407, 10002431,
                10002437, 10002439, 10002449, 10002463, 10002481, 10002521, 10002527, 10002529, 10002547, 10002563,
                10002571, 10002589, 10002599, 10002623, 10002649, 10002653, 10002659, 10002661, 10002667, 10002731,
                10002761, 10002763, 10002779, 10002791, 10002803, 10002809, 10002823, 10002829, 10002833, 10002847,
                10002859, 10002871, 10002887, 10002929, 10002943, 10002959, 10002983, 10003001, 10003003, 10003027,
                10003031, 10003043, 10003073, 10003087, 10003121, 10003127, 10003159, 10003181, 10003193, 10003199
            };
            _hypercube.SetPrimes(primes.Shuffle().Take(4).ToArray());
            _hypercube.SetSolving(true);
            float t = Time.time;
            while (Time.time - t < time)
            {
                _hypercube.SetRotationProgress(Easing.InOutCubic(Time.time - t, 0, 1, time));
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
            yield return ExpandSpheres(0, 10.24f);
            yield return new WaitUntil(() => _isFinished);

            _octadecayotton.Module.HandleStrike();
            _interact.isActive = false;
        }
    }
}
