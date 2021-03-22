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
        private bool _isFinished;

        internal IEnumerator CreateHypercube(int dimension)
        {
            _interact.Spheres = new List<SphereScript>();

            for (int i = 0; i < Math.Pow(2, dimension); i++)
            {
                var instance = UnityEngine.Object.Instantiate(_interact.Sphere, _interact.Polygon.transform, false).GetComponent<SphereScript>();
                instance.gameObject.SetActive(true);
                instance.pos = new Position(deviations: new[]
                {
                    Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f)
                }, stretchToFit: _octadecayotton.stretchToFit);
                instance.pos.SetDimensions(Convert.ToString(i, 2).PadLeft(dimension, '0').Select(c => float.Parse(c.ToString())).ToArray());
                instance.Interact = _interact;

                float f = 100f / Mathf.Pow(dimension, 2);
                instance.Sphere.transform.localScale = new Vector3(f, f, f);

                _interact.Spheres.Add(instance);

                for (int j = 0; dimension > 9 ? i % (dimension - 8) == 0 && j == 0 && i > Math.Pow(2, dimension) / 3 : j < Math.Pow(2, 9 - dimension) && i > Math.Pow(2, dimension) / 4; j += 4)
                {
                    float progress = 1 - ((i / (float)Math.Pow(2, dimension)) + (j / (float)Math.Pow(2, 9 - dimension) / (float)Math.Pow(2, dimension)));
                    byte b = (byte)(progress * 255);
                    _interact.ModuleRenderer.material.color = new Color32(b, b, b, 255);

                    for (int k = 0; k < _interact.Spheres.Count; k++)
                    {
                        var sphere = _interact.Spheres[k];
                        sphere.transform.localPosition = sphere.pos.MergeDimensions(new Vector3(
                            Rnd.Range(-0.01f, 0.01f) + 0.5f,
                            Rnd.Range(-0.01f, 0.01f) + 0.5f,
                            Rnd.Range(-0.01f, 0.01f) + 0.5f), progress.ElasticInOut());
                    }
                    if (!(dimension == 8 && i % 2 != 0 || dimension == 9 && i % 4 != 0))
                        yield return new WaitForSecondsRealtime(0.02f);
                }
            }

            _interact.ModuleRenderer.material.mainTexture = _interact.EvilTexture;

            for (byte i = 5; i >= 5; i += 5)
            {
                _interact.ModuleRenderer.material.color = new Color32(i, i, i, 255);
                yield return new WaitForSecondsRealtime(0.02f);
            }

            _interact.isRotating = !(_interact.isSubmitting = _interact.Rotations.Length == 0);
            _interact.isActive = true;
            _interact.isStarting = false;

            foreach (var sphere in _interact.Spheres)
            { 
                sphere.Light.enabled = false;

                if (_octadecayotton.colorAssist)
                {
                    sphere.SphereRenderer.material.color = new Color(sphere.transform.localPosition.x, sphere.transform.localPosition.y, sphere.transform.localPosition.z);
                    sphere.SphereRenderer.material.shader = _interact.AssistShader;
                }
            }

            if (_interact.isSubmitting)
            {
                _octadecayotton.PlaySound(Sounds.Oct.StartingSphere);
                for (int i = 0; i < _interact.Spheres.Count; i++)
                    _interact.Spheres[i].StartCoroutine(_interact.Spheres[i].UpdateValue());
            }
        }

        internal IEnumerator DestroyHypercube()
        {
            for (int i = 0; i < _interact.Spheres.Count; i++)
            {
                if (_interact.Spheres[i].Sphere.name.Contains("(Clone)"))
                    UnityEngine.Object.Destroy(_interact.Spheres[i].Sphere);

                if (i % Math.Pow(2, Math.Max(_interact.Dimension - 7, 0)) == 0)
                    yield return new WaitForSecondsRealtime(0.02f * (float)Math.Pow(2, Math.Max(9 - _interact.Dimension, 0)));
            }

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
            _octadecayotton.PlaySound(_interact.Dimension > 9 ? Sounds.Oct.SolveHard : Sounds.Oct.Solve);

            foreach (var sphere in _interact.Spheres)
                sphere.SphereRenderer.material.color = Color.white;

            bool shortTime = false;
            if (_octadecayotton.Info.GetTime() < 60)
            {
                shortTime = true;
                _octadecayotton.Module.HandlePass();
            }

            yield return ExpandSpheres(-4, 1 / 128f);
            yield return ShuffleSpheres(1 / 512f);

            if (!shortTime)
                _octadecayotton.Module.HandlePass();

            yield return DestroyHypercube();
        }

        internal IEnumerator ExpandSpheres(int amp, float speed)
        {
            var vectors = GetVectors(amp);

            float k = 0;
            while (k <= 1)
            {
                for (int i = 0; i < _interact.Spheres.Count; i++)
                    if (_interact.Spheres[i] != null)
                        _interact.Spheres[i].transform.localPosition = _interact.Spheres[i].pos.MergeDimensions(
                            vectors[i],
                            Easing.InOutCubic(k, 0, 1, 1));
                yield return new WaitForSecondsRealtime(0.02f);
                k += speed;
            }
        }

        internal IEnumerator ShuffleSpheres(float speed)
        {
            var oldVectors = GetVectors();
            var newVectors = GetVectors().Shuffle();

            for (float i = 0; i <= 1; i += speed)
            {
                for (int j = 0; j < _interact.Spheres.Count; j++)
                    _interact.Spheres[j].transform.localPosition = oldVectors[j].Merge(newVectors[j], Easing.InOutCubic(i, 0, 1, 1));
                yield return new WaitForSecondsRealtime(0.02f);
            }
        }

        private Vector3[] GetVectors(int amp)
        {
            Vector3[] vectors = new Vector3[_interact.Spheres.Count];
            float offset = amp == 0 ? 0.5f : 0.5f / Math.Abs(amp);
            for (int i = 0; i < vectors.Length; i++)
                vectors[i] = new Vector3(
                    amp * (_interact.Spheres[i].transform.localPosition.x - 0.5f) + offset,
                    amp * (_interact.Spheres[i].transform.localPosition.y - 0.5f) + offset,
                    amp * (_interact.Spheres[i].transform.localPosition.z - 0.5f) + offset);
            return vectors;
        }

        private Vector3[] GetVectors()
        {
            Vector3[] vectors = new Vector3[_interact.Spheres.Count];
            for (int i = 0; i < vectors.Length; i++)
                vectors[i] = _interact.Spheres[i].transform.localPosition;
            return vectors;
        }

        internal IEnumerator Strike()
        {
            _interact.isSubmitting = false;
            _interact.isRotating = false;

            _octadecayotton.PlaySound(Sounds.Oct.Strike);
            Debug.LogFormat("[The Octadecayotton #{0}]: Incorrect submission, strike and reset!", _octadecayotton.moduleId);

            _interact.ModuleRenderer.material.mainTexture = null;

            for (int i = 0; i < 2; i++)
            {
                _interact.ModuleRenderer.material.color = i % 2 == 0 ? Color.white : Color.black;
                for (int j = 0; j < _interact.Spheres.Count; j++)
                    _interact.Spheres[j].SphereRenderer.material.color = i % 2 == 0 ? Color.black : Color.white;
                yield return new WaitForSecondsRealtime(0.2f);
            }

            _interact.ModuleRenderer.material.color = Color.white;
            _interact.ModuleRenderer.material.mainTexture = _interact.NeutralTexture;

            _isFinished = false;
            _interact.StartCoroutine(DestroyHypercube());
            yield return ExpandSpheres(0, 1 / 512f);
            yield return new WaitUntil(() => _isFinished);

            _octadecayotton.Module.HandleStrike();
            _interact.isActive = false;
        }
    }
}
