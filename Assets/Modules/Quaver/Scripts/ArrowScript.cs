using System.Collections;
using System.Linq;
using UnityEngine;
using QE = QuaverModule.Convert;

public class ArrowScript : MonoBehaviour
{
    public GameObject Arrow;
    public KMAudio Audio;
    public Renderer ArrowRenderer;
    public Renderer[] Receptors;
    public Texture[] ArrowTextures, ReceptorTextures;

    internal int color;
    internal static int scrollSpeed = 10;
    internal static int[] positionsUsed = new int[4], arrowsPerColumn = new int[4];

    private static readonly Vector3[] spawnPositions =
    {
        new Vector3(-1.87f, 0.0004f, 2.82f),
        new Vector3(-0.62f, 0.0004f, 2.82f),
        new Vector3(0.62f, 0.0004f, 2.82f),
        new Vector3(1.87f, 0.0004f, 2.82f)
    };

    private bool isClone;
    private byte alpha;
    private int position;
    private static bool playSound;
    private const float speedZ = 0.15f, deleteZ = -2.1f;
    private static readonly int[] rotations = { 270, 180, 0, 90 };

    private void Start()
    {
        if (!char.IsDigit(name.Last()))
            return;

        isClone = true;

        int index = QE.CharToInt(name.Last());
        ArrowRenderer.material.mainTexture = ArrowTextures[index];

        do position = Random.Range(0, positionsUsed.Length);
        while (positionsUsed[position] != positionsUsed.Max());

        positionsUsed[position] = 0;

        Arrow.transform.localPosition = spawnPositions[position];
        Arrow.transform.localRotation = Quaternion.Euler(0, rotations[position], 0);
    }

    private void FixedUpdate()
    {
        if (!isClone)
        {
            playSound = true;
            for (int i = 0; i < positionsUsed.Length; i++)
                positionsUsed[i]++;
            return;
        }

        if (alpha != 255)
            alpha += 85;

        ArrowRenderer.material.color = new Color32(255, 255, 255, alpha);

        var pos = Arrow.transform.localPosition;
        Arrow.transform.localPosition = new Vector3(pos.x, pos.y, pos.z - (speedZ * scrollSpeed / 10));

        if (pos.z <= deleteZ)
        {
            if (playSound)
            {
                Audio.PlaySoundAtTransform(Sounds.Q.Note(ArrowRenderer.name.First().ToString()), transform);
                playSound = false;
            }

            isClone = false;
            StartCoroutine(FlashReceptor(pos));
        }
    }

    private IEnumerator FlashReceptor(Vector3 pos)
    {
        arrowsPerColumn[position]++;

        Arrow.transform.localPosition = new Vector3(pos.x, pos.y * -2, pos.z);
        ArrowRenderer.material = null;

        Receptors[position].material.mainTexture = ReceptorTextures[1];
        yield return new WaitForSecondsRealtime(0.1f);
        Receptors[position].material.mainTexture = ReceptorTextures[0];

        RenderScript.judgement = 1;
        Destroy(Arrow);
    }
}
