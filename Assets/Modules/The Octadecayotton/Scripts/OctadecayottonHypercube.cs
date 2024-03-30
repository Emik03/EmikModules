using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace TheOctadecayotton
{
    public class OctadecayottonHypercube : MonoBehaviour
    {
        const float ScaleFactor = 16f, BigScaleFactor = 2f;

        const int MaxDimensions = 27;
        const int MaxRotations = 5;
        const int MaxAxesPerRotation = 5;

        static int LODBarrier
        {
            get
            {
                // The LOD isn't strictly necessary - there are no limits enforcing it - but you won't be able to tell anyways at this scale.
                return _experimentalRendering ? 18 : 12;
            }
        }
        static int MeshLimit
        {
            get
            {
                // It takes a good while for the CPU to parrot the largest meshes to the GPU (which Unity forces onto the main thread),
                // and we don't actually need to consolidate the biggest meshes, so the limit is 21 instead of 27.
                return _experimentalRendering ? 21 : 13;
            }
        }

        private MeshRenderer _renderer;
        private MeshFilter _filter;
        private MeshRenderer[] _allRenderers;
        private TheOctadecayottonScript _octa;

        private int _dimension;
        private bool _stretch;
        private static bool _experimentalRendering;

        private GameObject _sphereA, _sphereB;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            _filter = GetComponent<MeshFilter>();
        }

        private static List<Vector4> _basisVectors = Enumerable.Range(0, MaxDimensions).Select(i => new Vector4(Position.weights[i, 0], Position.weights[i, 1], Position.weights[i, 2], 0)).ToList();

        private Color[] _colorAssistColors;
        private MeshFilter[] _meshes;
        private int[] _meshSizes;

        private static Vector3 MaxAxes(int dim)
        {
            return Enumerable
            .Range(0, dim)
            .Select(i => new Vector3(Position.weights[i, 0], Position.weights[i, 1], Position.weights[i, 2]))
            .Aggregate((acc, x) => acc + x);
        }

        internal IEnumerator AddMeshes(int dim, bool stretch, TheOctadecayottonScript octa)
        {
            _stretch = stretch;
            _sphereA = octa.SphereA;
            _sphereB = octa.SphereB;
            _octa = octa;

            _dimension = dim;
            _renderer.sharedMaterial = new Material(_renderer.sharedMaterial);

            _renderer.material.SetFloat("_dimensions", dim);
            _renderer.material.SetVectorArray("_sphereBasis", BaseMesh(dim == 12 ? 1 : dim).Select(v => (Vector4)v).ToList());
            _renderer.material.SetVectorArray("_basis", _basisVectors);
            _renderer.material.SetInt("_skipSpheres", 0);
            _renderer.material.SetInt("_meshLimit", MeshLimit);

            var maxOffset = MaxAxes(_dimension)
                + new Vector3(2f, 2f, 2f) / (dim > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor);
            _renderer.material.SetVector("_maxOffset", maxOffset);
            if (stretch)
            {
                _renderer.transform.localScale = new Vector3(1f / maxOffset.x, 1f / maxOffset.y, 1f / maxOffset.z);
                _sphereA.transform.localScale = new Vector3(1f / maxOffset.x, 1f / maxOffset.y, 1f / maxOffset.z) / (dim > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor) * 2.5f;
                _sphereB.transform.localScale = new Vector3(1f / maxOffset.x, 1f / maxOffset.y, 1f / maxOffset.z) / (dim > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor) * 2.5f;
            }
            else
            {
                var scale = 1f / Mathf.Max(maxOffset.x, maxOffset.y, maxOffset.z);
                _renderer.transform.localScale = new Vector3(scale, scale, scale);
                _sphereA.transform.localScale = new Vector3(scale, scale, scale) / (dim > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor) * 2.5f;
                _sphereB.transform.localScale = new Vector3(scale, scale, scale) / (dim > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor) * 2.5f;
            }

            _sphereA.GetComponentInChildren<Light>().range = 2 / Mathf.Pow(_dimension, 2) * octa.Interact.transform.lossyScale.x;
            _sphereB.GetComponentInChildren<Light>().range = 2 / Mathf.Pow(_dimension, 2) * octa.Interact.transform.lossyScale.x;

            if (dim == 12 && !_experimentalRendering)
            {
                _filter.sharedMesh = _specialMesh = _specialMesh ?? (_asyncSpecialMesh ?? GenerateMeshOfDimensions(12, 1)).Mesh;

                var r2 = Instantiate(_renderer.gameObject, _renderer.transform.parent, true).GetComponent<MeshRenderer>();
                r2.GetComponent<MeshFilter>().sharedMesh = _specialMeshB = _specialMeshB ?? (_asyncSpecialMeshB ?? GenerateMeshOfDimensions(12, 2)).Mesh;
                r2.transform.localScale = _renderer.transform.localScale;
                r2.transform.localPosition = _renderer.transform.localPosition;
                _allRenderers = new MeshRenderer[] { _renderer, r2 };
            }
            else if (dim <= MeshLimit)
            {
                _filter.sharedMesh = MeshOfDimensions(dim);
                _allRenderers = new MeshRenderer[] { _renderer };
            }
            else
            {
                _filter.sharedMesh = MeshOfDimensions(MeshLimit);
                int amountNeeded = (1 << (dim - MeshLimit)) - 1;
                _allRenderers = new MeshRenderer[amountNeeded + 1];
                _allRenderers[0] = _renderer;
                for (int i = 1; i <= amountNeeded; i++)
                {
                    if (i % 16 == 0)
                        yield return null;
                    _allRenderers[i] = Instantiate(_renderer.gameObject, _renderer.transform.parent, true).GetComponent<MeshRenderer>();
                    _allRenderers[i].material.SetInt("_indexOffset", i);
                    _allRenderers[i].transform.localScale = _renderer.transform.localScale;
                    _allRenderers[i].transform.localPosition = _renderer.transform.localPosition;
                }
            }

            if (octa.colorAssist)
            {
                _meshes = _allRenderers.Select(r => r.GetComponent<MeshFilter>()).ToArray();
                _colorAssistColors = Enumerable.Range(0, 1 << _dimension).Select(i => ColorFromIndex(i)).ToArray();

                if (_dimension == 12 && !_experimentalRendering)
                    _meshSizes = new int[] { 1 << 11, 1 << 11 };
                else if (_dimension <= MeshLimit)
                    _meshSizes = new int[] { 1 << _dimension };
                else
                    _meshSizes = Enumerable.Repeat(1 << MeshLimit, 1 << (_dimension - MeshLimit)).ToArray();
                SetColors();
            }
        }

        private void SetColors()
        {
            for (int i = 0; i < _meshes.Length; i++)
                _meshes[i].mesh.SetColors(
                    _colorAssistColors
                        .Skip(_meshSizes.Take(i).Sum())
                        .Take(_meshSizes[i])
                        .SelectMany(c => Enumerable.Repeat(c, _dimension <= LODBarrier ? 17 : 6))
                        .ToList()
                    );

            foreach (var thing in _allRenderers)
                thing.material.SetFloat("_blendSphereColor", 1f);
        }

        internal void SwizzleColors(Rotation[][] rot)
        {
            var cols = new Color[_colorAssistColors.Length];
            var rots = rot.Select(r => r.Select(c => (int)c.Axis).ToArray()).ToArray();
            var roti = rot.Select(r => r.Select(c => c.IsNegative ? 1 : 0).ToArray()).ToArray();
            for (int ix = 0; ix < _colorAssistColors.Length; ix++)
            {
                int i = ix;
                for (int r = 0; r < rot.Length; r++)
                {
                    int temp = i & (1 << rots[r][0]);
                    i ^= temp;
                    temp >>= rots[r][0];
                    for (int j = 0; j < rot[r].Length - 1; j++)
                    {
                        i ^= i & (1 << rots[r][j]);
                        var thing = (i & (1 << rots[r][j + 1])) >> rots[r][j + 1];
                        i |= (roti[r][j] ^ thing) << rots[r][j];
                    }
                    i ^= i & (1 << rots[r][rot[r].Length - 1]);
                    i |= (roti[r][rot[r].Length - 1] ^ temp) << rots[r][rot[r].Length - 1];
                }
                cols[i] = _colorAssistColors[ix];
            }
            _colorAssistColors = cols;
            SetColors();
        }

        private Color ColorFromIndex(int ix)
        {
            Vector4 total = Vector4.zero;
            for (int i = 0; i < _dimension; i++)
                if ((ix & (1 << i)) != 0)
                    total += _basisVectors[i];
            return new Color(total.x / MaxAxes(_dimension).x, total.y / MaxAxes(_dimension).y, total.z / MaxAxes(_dimension).z);
        }

        internal int _usedSphere;
        internal void HighlightSphere(Dictionary<Axis, bool> which)
        {
            GameObject sphere1, sphere2;
            if (_usedSphere == 1)
            {
                sphere1 = _sphereA;
                sphere2 = _sphereB;
            }
            else
            {
                sphere1 = _sphereB;
                sphere2 = _sphereA;
            }

            var maxOffset = MaxAxes(_dimension)
                + new Vector3(2f, 2f, 2f) / (_dimension > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor);
            var pos = Enumerable
                .Range(0, _dimension)
                .Select(i => which[(Axis)i] ? _basisVectors[i] : Vector4.zero)
                .Aggregate((a, b) => a + b)
                + Vector4.one / (_dimension > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor);

            if (_stretch)
            {
                sphere1.transform.localPosition = new Vector3(pos.x / maxOffset.x, pos.y / maxOffset.y, pos.z / maxOffset.z);
            }
            else
            {
                var scale = 1f / Mathf.Max(maxOffset.x, maxOffset.y, maxOffset.z);
                sphere1.transform.localPosition = (Vector3)pos * scale;
            }
            //sphere1.GetComponent<Renderer>().material.color = ;
            StartCoroutine(FadeInOut(sphere1.GetComponent<Renderer>(), 1f));
            if (_usedSphere != 0)
                StartCoroutine(FadeInOut(sphere2.GetComponent<Renderer>(), 0f));
            sphere1.GetComponentInChildren<Light>().enabled = true;
            sphere2.GetComponentInChildren<Light>().enabled = false;

            if (_usedSphere == 1)
                _usedSphere = 2;
            else
                _usedSphere = 1;
        }
        internal void UnhighlightSpheres()
        {
            _sphereA.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0f);
            _sphereB.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0f);
            _sphereA.GetComponentInChildren<Light>().enabled = false;
            _sphereB.GetComponentInChildren<Light>().enabled = false;
            _usedSphere = 0;
        }

        private IEnumerator FadeInOut(Renderer fader, float endAmount)
        {
            const float duration = 0.8f;
            float t = Time.time;
            while (Time.time - t < duration)
            {
                fader.material.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f - endAmount, endAmount, (Time.time - t) / duration));
                yield return null;
            }
            fader.material.color = new Color(1f, 1f, 1f, endAmount);
        }

        internal void ForceColor(Color? col = null, float blend = 1f)
        {
            if (col == null)
            {
                foreach (var thing in _allRenderers)
                    thing.material.SetFloat("_blendFixedColor", 0f);
                return;
            }
            foreach (var thing in _allRenderers)
            {
                thing.material.SetColor("_fixedColor", col.Value);
                thing.material.SetFloat("_blendFixedColor", blend);
            }
        }

        internal void SetJitter(float jitter = 0f)
        {
            foreach (var thing in _allRenderers)
                if (thing != null)
                    thing.material.SetFloat("_jitterScale", jitter);
        }

        internal void SetSolving(bool yes)
        {
            foreach (var thing in _allRenderers)
                thing.material.SetInt("_solveAnimation", yes ? 1 : 0);
        }


        private IEnumerable<T> PadLimit<T>(IEnumerable<T> items, int limit)
        {
            int count = 0;
            foreach (var item in items)
            {
                yield return item;
                count++;
                if (count >= limit)
                    yield break;
            }
            while (count < limit)
            {
                yield return default(T);
                count++;
            }
        }

        internal void SetRotation(Rotation[][] rotations)
        {
            _rotations = rotations;

            var inv = PadLimit(rotations.SelectMany(r => PadLimit(r.Select(x => x.IsNegative ? 1f : 0f), 5)), MaxRotations * MaxAxesPerRotation).ToArray();
            var rot = PadLimit(rotations.SelectMany(r => PadLimit(r.Select(x => (float)(int)x.Axis), 5)), MaxRotations * MaxAxesPerRotation).ToArray();
            var szs = PadLimit(rotations.Select(r => (float)r.Length), MaxRotations).ToArray();
            var rs = rotations.Length;

            foreach (var thing in _allRenderers)
            {
                thing.material.SetFloatArray("_invert", inv);
                thing.material.SetFloatArray("_rotation", rot);
                thing.material.SetFloatArray("_rotationSizes", szs);
                thing.material.SetInt("_rotations", rs);
            }
        }

        internal void DisableSpheres(uint count)
        {
            int limit = _dimension == 12 && !_experimentalRendering ? 11 : MeshLimit;

            var offsets = count >> limit;
            var indices = count ^ (offsets << limit);
            for (int i = 0; i < _allRenderers.Length; i++)
            {
                MeshRenderer thing = _allRenderers[i];
                if (offsets > i)
                    thing.material.SetInt("_skipSpheres", 2 << MeshLimit);
                else if (offsets == i)
                    thing.material.SetInt("_skipSpheres", (int)indices);
                else
                    thing.material.SetInt("_skipSpheres", 0);
            }
        }

        internal void Cleanup()
        {
            foreach (var rend in _allRenderers)
                if (rend != _renderer)
                    Destroy(rend.gameObject);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _asyncMeshTemplates.Length; i++)
            {
                if (_meshTemplates[i] != null)
                    _asyncMeshTemplates[i] = null;
            }
            if (_specialMesh != null)
                _asyncSpecialMesh = null;
            if (_specialMeshB != null)
                _asyncSpecialMeshB = null;
        }

        private Rotation[][] _rotations;
        private float _prevProgress = 0f;
        internal void SetRotationProgress(float t)
        {
            if (_octa.colorAssist && _prevProgress - t > .6f)
                SwizzleColors(_rotations);
            _prevProgress = t;
            if (_allRenderers != null)
                foreach (var thing in _allRenderers)
                    thing.material.SetFloat("_t", t);
        }

        internal static Vector3[] BaseMesh(int dim)
        {
            if (dim > LODBarrier)
                return new Vector3[]
                {
                    new Vector3(1f,1f,0f),
                    new Vector3(1f,1f,2f),
                    new Vector3(1f,0f,1f),
                    new Vector3(1f,2f,1f),
                    new Vector3(0f,1f,1f),
                    new Vector3(2f,1f,1f)
                }.Select(v => v / ScaleFactor / BigScaleFactor).ToArray();
            return new Vector3[]
                {
                    new Vector3(1.000000f, 1.707107f, 0.292893f),
                    new Vector3(1.000000f, 2.000000f, 1.000000f),
                    new Vector3(1.000000f, 1.707107f, 1.707107f),
                    new Vector3(1.000000f, 1.000000f, 2.000000f),
                    new Vector3(1.672498f, 1.218508f, 0.292893f),
                    new Vector3(1.951056f, 1.309017f, 1.000000f),
                    new Vector3(1.672498f, 1.218508f, 1.707107f),
                    new Vector3(1.415627f, 0.427939f, 0.292893f),
                    new Vector3(1.587785f, 0.190983f, 1.000000f),
                    new Vector3(1.415627f, 0.427939f, 1.707107f),
                    new Vector3(1.000000f, 1.000000f, 0.000000f),
                    new Vector3(0.584373f, 0.427939f, 0.292893f),
                    new Vector3(0.412215f, 0.190983f, 1.000000f),
                    new Vector3(0.584373f, 0.427939f, 1.707107f),
                    new Vector3(0.327502f, 1.218508f, 0.292893f),
                    new Vector3(0.048943f, 1.309017f, 1.000000f),
                    new Vector3(0.327502f, 1.218508f, 1.707107f)
                }.Select(v => v / ScaleFactor).ToArray();
        }

        internal static int[] BaseTris(int dim)
        {
            if (dim > LODBarrier)
                return new int[]
                {
                    0, 2, 4,
                    1, 4, 2,
                    0, 4, 3,
                    1, 3, 4,
                    0, 5, 2,
                    1, 2, 5,
                    0, 3, 5,
                    1, 5, 3
                };
            return new int[]
                {
                    1, 6, 2,
                    3, 6, 7,
                    1, 11, 5,
                    4, 3, 7,
                    7, 9, 10,
                    5, 11, 8,
                    4, 7, 10,
                    5, 9, 6,
                    10, 13, 14,
                    8, 11, 12,
                    4, 10, 14,
                    8, 13, 9,
                    13, 15, 16,
                    13, 17, 14,
                    12, 11, 15,
                    4, 14, 17,
                    4, 17, 3,
                    15, 2, 16,
                    16, 3, 17,
                    15, 11, 1,
                    1, 5, 6,
                    3, 2, 6,
                    7, 6, 9,
                    5, 8, 9,
                    10, 9, 13,
                    8, 12, 13,
                    13, 12, 15,
                    13, 16, 17,
                    15, 1, 2,
                    16, 2, 3
                }.Select(i => i - 1).ToArray();
        }

        private static Mesh[] _meshTemplates;
        private static Mesh _specialMesh, _specialMeshB;
        private static MeshData[] _asyncMeshTemplates;
        private static MeshData _asyncSpecialMesh, _asyncSpecialMeshB;

        private void Awake()
        {
            if (_meshTemplates != null)
                return;

            _meshTemplates = new Mesh[MaxDimensions - 2];
            _asyncMeshTemplates = new MeshData[MaxDimensions - 2];
            _experimentalRendering = ModSettingsJSON.GetExperimentalRenderingEnabled();

            Debug.LogFormat("<The Octadecayotton> Experimental rendering is {0}.", _experimentalRendering ? "ON" : "OFF");
            if (_experimentalRendering)
                Debug.Log("<The Octadecayotton> Generating meshes on another thread: dimentions 3-18 in high detail, dimensions 19-21 low detail");
            else
                Debug.Log("<The Octadecayotton> Generating meshes on another thread: dimentions 3-11 in high detail, dimension 13 low detail, 2x dimension 12 in high detail");

            // Larger meshes take a while to generate, so we do that on another thread.
            new Thread(GenerateMeshes).Start();

            StartCoroutine(FinalizeMeshes());
        }

        private IEnumerator FinalizeMeshes()
        {
            if (!_experimentalRendering)
            {
                while (_asyncSpecialMesh == null && _specialMesh == null)
                    yield return new WaitForSeconds(0.1f);
                if (_specialMesh == null)
                {
                    yield return _asyncSpecialMesh.FinalizeAsync();
                    _specialMesh = _asyncSpecialMesh.Mesh;
                }

                while (_asyncSpecialMeshB == null && _specialMeshB == null)
                    yield return new WaitForSeconds(0.1f);
                if (_specialMeshB == null)
                {
                    yield return _asyncSpecialMeshB.FinalizeAsync();
                    _specialMeshB = _asyncSpecialMeshB.Mesh;
                }
            }

            for (int i = 3; i <= MeshLimit; i++)
            {
                if (!_experimentalRendering && i == 12)
                    continue;
                while (_asyncMeshTemplates[i - 3] == null && _meshTemplates[i - 3] == null)
                    yield return new WaitForSeconds(0.1f);
                if (_meshTemplates[i - 3] == null)
                {
                    yield return _asyncMeshTemplates[i - 3].FinalizeAsync();
                    _meshTemplates[i - 3] = _asyncMeshTemplates[i - 3].Mesh;
                }
            }
        }

        private void GenerateMeshes()
        {
            if (!_experimentalRendering)
            {
                if (_asyncSpecialMesh == null && _specialMesh == null)
                    _asyncSpecialMesh = GenerateMeshOfDimensions(12, 1);
                if (_asyncSpecialMeshB == null && _specialMeshB == null)
                    _asyncSpecialMeshB = GenerateMeshOfDimensions(12, 2);
            }

            for (int dim = 3; dim <= MeshLimit; dim++)
                if (_meshTemplates[dim - 3] == null && _asyncMeshTemplates[dim - 3] == null && (dim != 12 || _experimentalRendering))
                    _asyncMeshTemplates[dim - 3] = GenerateMeshOfDimensions(dim);
        }

        internal static Mesh MeshOfDimensions(int dim)
        {
            if (_meshTemplates[dim - 3] == null)
                _meshTemplates[dim - 3] = (_asyncMeshTemplates[dim - 3] ?? GenerateMeshOfDimensions(dim)).Mesh;
            return _meshTemplates[dim - 3];
        }

        private class MeshData
        {
            public Vector3[] Vertices;
            public int[] Triangles;
            public Vector2[] UV;
            public string Name;
            public Bounds Bounds;

            private Mesh _mesh;
            public Mesh Mesh
            {
                get
                {
                    return _mesh ?? Finalize();
                }
            }

            public IEnumerator FinalizeAsync()
            {
                Debug.Log("<The Octadecayotton> Finalizing mesh " + Name);
                _mesh = new Mesh
                {
                    name = Name,
                    bounds = Bounds
                };
                if (_experimentalRendering)
                    _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                _mesh.vertices = (Vertices);
                yield return null;
                _mesh.triangles = Triangles;
                yield return null;
                _mesh.uv = UV;
            }

            private Mesh Finalize()
            {
                var it = FinalizeAsync();
                while (it.MoveNext()) ;
                return _mesh;
            }
        }

        // In Unity, a mesh can only have 2^32 vertices due to how faces index into the list of vertices.
        // On most GPUs, this limit is much lower: 2^16 without expensive software emulation.
        // The experimental rendering option allows the module to use the 2^32 limit.
        // For added performance at high dimesnsions, we use a LOD system:
        // 12D Octas and smaller use a 17-vertex sphere model (a UV sphere which looks pentagonal from many angles).
        // 13D Octas and larger use a 6-vertex sphere model (an octahedron).
        // Importantly, on modern GPUs, vertices and tris (faces) are the largest limiting factor for a detailed mesh like this.
        // This is why we prefer an octahedron (6 verts, 8 tris) to a cube (8 verts, 12 tris).
        //
        // With the 2^32 limit, every size of Octa fits in one mesh at high detail.
        // With the 2^16 limit, high detail Octas fit into the indexing space of one mesh up to 11D,
        // while low detail Octas can fit into one mesh up to 13D. Dimensions above this
        // use multiple meshes to achieve the same effect (as well as 12D to use high detail meshes).
        private static MeshData GenerateMeshOfDimensions(int dim, int special = 0)
        {
            Vector3[] verts = BaseMesh(dim);
            int[] tris = BaseTris(dim);

            int sphereCount = 1 << Mathf.Min(dim, MeshLimit);

            if (special != 0)
                sphereCount /= 2;

            var total = MaxAxes(dim) + new Vector3(2f, 2f, 2f) / (dim > LODBarrier ? ScaleFactor * BigScaleFactor : ScaleFactor);

            int w = (int)Mathf.Pow(sphereCount, 1f / 3f);
            var steps = MaxAxes(dim) / (w - 1);
            Func<int, Vector3> offset = (i) =>
            {
                int x = i % w;
                i /= w;
                int y = i % w;
                i /= w;
                int z = i % w;
                return new Vector3(steps.x * x, steps.y * y, steps.z * z);
            };

            int vertLength = verts.Length;
            int trisLength = tris.Length;

            Vector3[] vertices = new Vector3[sphereCount * vertLength];
            for (int i = 0; i < sphereCount; i++)
            {
                var off = offset(i);
                var ix = i * vertLength;
                for (int j = 0; j < vertLength; j++)
                    vertices[ix + j] = verts[j] + off;
            }

            int[] triangles = new int[sphereCount * trisLength];
            for (int i = 0; i < sphereCount; i++)
            {
                int ix = i * vertLength, ix2 = i * trisLength;
                for (int j = 0; j < trisLength; j++)
                    triangles[ix2 + j] = tris[j] + ix;
            }

            var uvCount = sphereCount * vertLength;
            Vector2[] uvs = new Vector2[uvCount];
            if (special == 2)
            {
                for (int i = 0; i < sphereCount; i++)
                {
                    var ix = i * vertLength;
                    var ix2 = i + uvCount;
                    for (int j = 0; j < vertLength; j++)
                        uvs[ix + j] = new Vector2(ix2, j);
                }
            }
            else
            {
                for (int i = 0; i < sphereCount; i++)
                {
                    var ix = i * vertLength;
                    for (int j = 0; j < vertLength; j++)
                        uvs[ix + j] = new Vector2(i, j);
                }
            }

            return new MeshData
            {
                Vertices = vertices,
                Triangles = triangles,
                UV = uvs,
                Name = "Octa" + dim + "d" + (special == 0 ? "" : special == 1 ? "A" : "B"),
                Bounds = new Bounds(total / 2, total)
            };
        }
    }
}
