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

    internal bool IsOnBone { get { return _maze[_position.Item1][_position.Item2] == 'x'; } }
    internal bool[] ValidMoves { get { return _maze.GetValidMovements(ref _position); } }
    internal int OrderOffset { get { return _order[_interactCount % 4] ? 4 : 0; } }

    internal bool isColorblind, isStrike;

    private IntTuple _position, _initialPosition;
    private String[] _maze, _initialMaze;

    private bool[] _order;
    private int _interactCount;

    public override void OnActivate()
    {
        isColorblind = Get<KMColorblindMode>().ColorblindModeActive;

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

        if (_maze.IsSolved())
        {
            PlaySound(SFX.Ud.Solve);
            Solve("Solved! :)");
        }
    }

    private void ArrowsInteract(int i)
    {
        if (IsSolved)
            return;

        ButtonEffect(Arrows[i], 1, Sound.ButtonPress);

        if (_order[_interactCount % 4] ^ i >= 4)
            OnStrike("The wrong type of button has been pushed, causing the dog to trip and fall. Strike for being unable to walk correctly!");
        
        else if (!_maze.IsValidMove(this, ref _position, i % 4))
            OnStrike("The dog violently crashed into the wall, hurting him. Strike for animal cruelty!");
        
        else
            _interactCount++;
    }

    private IEnumerator Flash(Color[] colors, string text)
    {
        const float Time = 0.375f;

        string[] colorblind = colors.AsString(text);
        int i = 0;

        while (!IsSolved)
        {
            UpdateCenter(isColorblind ? colorblind.ElementAtWrap(i) : text, colors.ElementAtWrap(i));
            yield return new WaitForSecondsRealtime(Time);
            i++;
        }

        UpdateCenter(Colors.White, Colors.white);
    }

    private void UpdateCenter(string str, Color color)
    {
        var lightColors = new[] { Colors.yellow, Colors.green, Colors.white};

        Text.text = str;
        Text.color = lightColors.IsAnyEqual(color) ? Color.black : Color.white;
        CenterRenderer.material.color = color;
    }

    private void OnStrike(string message)
    {
        _interactCount = 0;
        _position = _initialPosition;

        _initialMaze.Copy(_maze);
        isStrike = true;

        Strike(message);
    }
}
