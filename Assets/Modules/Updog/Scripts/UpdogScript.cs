using KeepCoding;
using System.Collections;
using UnityEngine;
using Updog;
using IntTuple = KeepCoding.Tuple<int, int>;
using Sound = KMSoundOverride.SoundEffect;
using String = System.Text.StringBuilder;

public class UpdogScript : ModuleScript
{
    public KMSelectable[] Arrows, Center;
    public Renderer CenterRenderer;
    public TextMesh Text;
    public Texture2D[] TobySprites;
    public Renderer[] DogButtonRenderers;
    public SpriteRenderer ExplosionRenderer;
    public Sprite[] ExplosionSprites;

    internal bool IsOnBone { get { return _maze[_position.Item1][_position.Item2] == 'x'; } }
    internal bool[] ValidMoves { get { return _maze.GetValidMovements(ref _position); } }
    internal int OrderOffset { get { return _order[_interactCount % 4] ? 4 : 0; } }

    private IntTuple _position, _initialPosition;
    private String[] _maze, _initialMaze;

    private int[] _animationIndex = new int[4];

    private bool[] _order;
    private int _interactCount;
#pragma warning disable 414 // Used via reflection
    private string _souvenirWord;
    private Color[] _souvenirColors;
#pragma warning restore
    public override void OnActivate()
    {
        Arrows.Assign(onInteract: ArrowsInteract);
        Center.Assign(onInteract: CenterInteract);

        var colors = Colors.GetFinal;
        var word = Words.GetRandom;
        var maze = Mazes.Get(word.Value, colors[2], colors[4]);

        StartCoroutine(Flash(colors, word.Key));

        _position = _initialPosition = maze.Find(colors[0]);
        _order = Words.GetOrder(colors[6], word.Value.Item2);
        _initialMaze = maze.InsertBones();

        _maze = new String[_initialMaze.Length];
        _initialMaze.Copy(_maze);

        _order.ToLog(this);
        _maze.ToLog(this, _position);
    }

    private void CenterInteract(int i)
    {
        if (IsSolved)
            return;

        ButtonEffect(Center[0], 2, Sound.BigButtonPress);

        if (!IsOnBone)
        {
            OnStrike("The current tile ({0},{1}) does not contain a bone and now the dog is sad, strike!"
                .Form(_position.Item1.ElevenToFiveIndex(), _position.Item2.ElevenToFiveIndex()));
            return;
        }

        Log("A bone has been picked up! {0} bone{1} remaining..."
            .Form(_maze.CountBones() - 1, _maze.CountBones() - 1 == 1 ? "" : "s"));

        _maze[_position.Item1][_position.Item2] = ' ';

        if (!_maze.IsSolved())
        {
            PlaySound(SFX.Ud.Heal);
            return;
        }

        PlaySound(SFX.Ud.Solve);

        PlaySound(SFX.Ud.Explosion);
        StartCoroutine(AnimateExplosion());

        Solve("Solved! :)");
        for (int ix = 0; ix < 4; ix++)
            StartCoroutine(AnimateSolve(ix));
    }

    public override void OnColorblindChanged(bool isEnabled) { }

    private void ArrowsInteract(int i)
    {
        if (IsSolved)
            return;

        ButtonEffect(Arrows[i], 1, i >= 4 ? (KeepCoding.Sound)SFX.Ud.Dog : Sound.ButtonPress);

        if (i >= 4)
        {
            StartCoroutine(AnimateDog(i - 4));
        }

        if (_order[_interactCount % 4] ^ i >= 4)
            OnStrike("The wrong type of button has been pushed, causing the dog to trip and fall. Strike for being unable to walk correctly!");

        else if (!_maze.IsValidMove(this, ref _position, i % 4))
            OnStrike("The dog violently crashed into the wall, hurting him. Strike for animal cruelty!");

        else
            _interactCount++;
    }

    private IEnumerator AnimateDog(int ix)
    {
        const float Delay = 0.2f;

        var mat = DogButtonRenderers[ix].material;

        _animationIndex[ix]++;
        var anim = _animationIndex[ix];

        mat.mainTexture = TobySprites[3];
        yield return new WaitForSeconds(Delay * 2);
        if (_animationIndex[ix] != anim)
            yield break;

        for (int i = 0; i < 6; i++)
        {
            mat.mainTexture = TobySprites[1];
            yield return new WaitForSeconds(Delay);
            if (_animationIndex[ix] != anim)
                yield break;
            mat.mainTexture = TobySprites[2];
            yield return new WaitForSeconds(Delay);
            if (_animationIndex[ix] != anim)
                yield break;
        }
        mat.mainTexture = TobySprites[0];
    }

    private IEnumerator AnimateSolve(int ix)
    {
        const float Delay = 0.66f;

        yield return AnimateDog(ix);

        var offset = Random.Range(7f, 30f);
        yield return new WaitForSeconds(offset);

        var mat = DogButtonRenderers[ix].material;

        mat.mainTextureScale = new Vector2(0.85f, 0.85f);
        mat.mainTextureOffset = new Vector2(0.075f, 0.075f);

        while (true)
        {
            mat.mainTexture = TobySprites[4];
            yield return new WaitForSeconds(Delay);
            mat.mainTexture = TobySprites[5];
            yield return new WaitForSeconds(Delay);
        }
    }

    private IEnumerator AnimateExplosion()
    {
        const float Delay = 0.05f;

        ExplosionRenderer.gameObject.SetActive(true);

        for (int i = 0; i < ExplosionSprites.Length; i++)
        {
            ExplosionRenderer.sprite = ExplosionSprites[i];
            yield return new WaitForSeconds(Delay);
        }

        ExplosionRenderer.gameObject.SetActive(false);
    }

    private IEnumerator Flash(Color[] colors, string text)
    {
        _souvenirWord = text;
        _souvenirColors = colors;

        const float Time = 0.375f;

        string[] colorblind = colors.AsString(text);
        int i = 0;

        while (!IsSolved)
        {
            UpdateCenter(IsColorblind ? colorblind.ElementAtWrap(i) : text, colors.ElementAtWrap(i), IsColorblind ? 1f : 1.25f);
            yield return new WaitForSecondsRealtime(Time);
            i++;
        }

        UpdateCenter(Colors.White, Colors.white, 1f);
    }

    private void UpdateCenter(string str, Color color, float scale)
    {
        var lightColors = new[] { Colors.yellow, Colors.green, Colors.white };

        Text.text = str;
        Text.color = lightColors.IsAnyEqual(color) ? Color.black : Color.white;
        CenterRenderer.material.color = color;
        Text.transform.localScale = Text.transform.localScale.Set(x: scale);
    }

    private void OnStrike(string message)
    {
        _interactCount = 0;
        _position = _initialPosition;

        _initialMaze.Copy(_maze);

        PlaySound(SFX.Ud.Strike);

        Strike(message);
    }
}
