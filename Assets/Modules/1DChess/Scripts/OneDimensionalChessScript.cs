using OneDimensionalChess;
using EmikBaseModules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Srnd = System.Random;
using Urnd = UnityEngine.Random;
using System.IO;

/// <summary>
/// On the Subject of 1D Chess - A modded "Keep Talking and Nobody Explodes" module created by Emik.
/// </summary>
public partial class OneDimensionalChessScript : ModuleScript
{
    public CustomValues Debugger;
    public KMSelectable[] Buttons;
    public Renderer[] BoardRenderers, ButtonRenderers;
    public TextMesh MovesLeftText, Title, Subtitle;
    public Texture[] BoardTextures, ButtonTextures;

    internal int MovesLeft
    {
        get { return _movesLeft; }
        set
        {
            MovesLeftText.text = value.ToString();
            _movesLeft = value;
        }
    }

    internal string Alphabet { get { return "abcdefghijklmnopqrstuvwxyz".Substring(0, Debugger.IsEnabled ? Debugger.Length : _boardLength); } }

    internal bool isReady;
    internal int? last;
    internal string position;
    internal PieceColor color;
    internal List<string> souvenirPositions;

    private string RandomPosition { get { return Debugger.IsEnabled ? Debugger.Position.IsNullOrEmpty() ? Position.Generate(Debugger.Length, 2) : Debugger.Position : Position.Generate(_boardLength, 2); } }

    private static readonly Color32[] _colorScheme =
    {
        new Color32(119, 149, 86, 255),
        new Color32(235, 236, 208, 255),
        new Color32(111, 133, 200, 255)
    };

    private bool _isWinning;
    private static bool _isUsingThreads, _isRustLoaded;
    private int _boardLength, _movesLeft;

    private Work<string, int, bool, CGameResult> _bestMove;

    private void Start()
    {
        // This will install the Rust library only once. It causes problems in the editor, so we need to check that too.
        if (!IsEditor && !_isRustLoaded)
        {
            _isRustLoaded = true;
            try
            {
                PathManager.LoadLibrary("EmikModules", Engine.LibraryName);
            }
            catch (FileNotFoundException e)
            {
                Panic("The Rust library failed to load! Please provide a FULL log file to @Emik#0001 on Discord", e);
                return;
            }
        }

        // This is a test to make sure the library works, if it isn't loaded the module will solve here.
        try
        {
            Engine.IsLegalMove("", 0, 0);
            Engine.Calculate("", 0, true);
        }
        catch (DllNotFoundException e)
        {
            Panic("The Rust library file couldn't be found! Please reinstall the module, and check for the integrity of the /dlls folder! There should be a .so, .dylib, and .bundle file in the root, as well as 2 folders containing a dll.", e);
            return;
        }

        _bestMove = new Work<string, int, bool, CGameResult>(
            work: Engine.Calculate,
            allowSimultaneousActive: true,
            maximumThreadsActive: 1);

        Get<KMBombModule>().Assign(this, () =>
        {
            // This ensures that the same positions generate if a bomb seed is used.
            Position.random = new Srnd(Urnd.Range(0, int.MaxValue));

            _boardLength = Position.random.Next(8, 10);

            _isUsingThreads = false;

            Buttons.Assign(onInteract: OnInteract);

            StartCoroutine(GetGoodPosition());

            BoardTiling();
        });
    }

    private void OnInteract(int arg)
    {
        // This prevents you from making a move while the computer is thinking.
        if (!isReady)
            return;

        // This prevents you from selecting an empty space as the origin.
        if (position[arg] == Position.PieceChars[0] && last == null)
            return;

        // This prevents you from selecting a piece that isn't yours as the origin.
        if (position[arg].GetPieceColor() != color && last == null)
            return;

        // Gives button feedback.
        Buttons[arg].Push(Get<KMAudio>(), 1, Sounds._1dch.Click);

        // Highlight the selected square.
        BoardRenderers[arg].material.color = _colorScheme[2];

        // The destination is selected.
        if (last != null)
        {
            // The user did not deselect.
            if (last != arg)
            {
                bool isLegalMove = Engine.IsLegalMove(position, (sbyte)last, (sbyte)arg);

                // Updates the position on a legal move, cancels the selection otherwise.
                if (isLegalMove)
                {
                    // Creates the desired move.
                    var move = new CGameResult
                    {
                        Piece = position[(int)last].Piece(),
                        Origin = (sbyte)last,
                        Destination = (sbyte)arg
                    };

                    position = move.Move(position, Get<KMAudio>());

                    Log("You play {0}, the position is now {1}.", ToLog(move), position);

                    souvenirPositions.Add(ToLog(move));

                    MovesLeft--;
                    StartCoroutine(GetEngineMove());
                }

                Get<KMAudio>().Play(transform, isLegalMove ? Sounds._1dch.Self : Sounds._1dch.Illegal);
            }

            RenderPosition(position);
        }

        // Toggle _lastSelect between null and the argument passed in to the method.
        last = last == null ? (int?)arg : null;
    }

    private IEnumerator GetEngineMove()
    {
        // This prevents the player from making a move during the calculation.
        isReady = false;

        ChangeText("Thinking...", "");

        // This checks if it is stalemate for the engine.
        if (position.IsStalemate(color.Flip()))
        {
            StartCoroutine(HandleStrike("You lose!", "Stalemate!"));
            yield break;
        }

        // This checks if there are only kings on the board, which is always a draw.
        if (position.All(c => c == '_' || c.ToLower() == 'k'))
        {
            StartCoroutine(HandleStrike("You lose!", "Kings only!"));
            yield break;
        }

        yield return _bestMove.Start(position, (MovesLeft * 2) + 1, color == PieceColor.Black);

        // This looks convoluted, but it's only asking if the player has made a blunder, to the point of an unwinnable position.
        if (_isWinning &&
           ((color == PieceColor.White && _bestMove.Result.Evaluation != sbyte.MaxValue - (MovesLeft * 2)) ||
            (color == PieceColor.Black && _bestMove.Result.Evaluation != (MovesLeft * 2) + sbyte.MinValue)))
        {
            _isWinning = false;
            Log("Rustmate has evaluated that this position is now unwinnable for you. Congratulations.");
        }

        ChangeText("Mate in", "", MovesLeft.ToString());

        Get<KMAudio>().Play(transform, Sounds._1dch.Opponent);

        // This indicates if the game has ended.
        if (_bestMove.Result.IsEqual(Position.finishedGame))
        {
            isReady = false;

            Get<KMAudio>().Play(transform, Sounds._1dch.Check);

            // Stalemate.
            if (_bestMove.Result.Evaluation == 0)
                StartCoroutine(HandleStrike("You lose!", "Stalemate!"));

            // Checkmate against the player.
            else if (color == PieceColor.White ^ _bestMove.Result.Evaluation > 0)
                StartCoroutine(HandleStrike("You lose!", "Checkmate!"));

            // Checkmate for the player.
            else
            {
                Get<KMAudio>().Play(transform, Sounds._1dch.GameEnd, Sounds._1dch.Solve);

                string message = new[] { "Good game!", "Well played!" }.PickRandom();
                Solve(message);
                ChangeText("Solved!", message);
            }
        }

        else
        {
            // This allows the player to make a move again.
            isReady = true;

            position = _bestMove
                .Result
                .Move(position, Get<KMAudio>());

            Log("Rustmate plays {0}, the position is now {1}.", ToLog(_bestMove.Result), position);

            souvenirPositions.Add(ToLog(_bestMove.Result));

            // Having the game end after an engine move always means a loss for the player.
            if (position.IsGameEnd(color))
                StartCoroutine(HandleStrike("You lose!", position.IsStalemate(color) ? "Stalemate!" : "Checkmate!"));

            // This checks if there are only kings on the board, which is always a draw.
            else if (position.All(c => c == '_' || c.ToLower() == 'k'))
                StartCoroutine(HandleStrike("You lose!", "Kings only!"));

            else if (MovesLeft == 0)
                StartCoroutine(HandleStrike("You lose!", "Out of moves!"));
        }

        RenderPosition(position);
    }

    private IEnumerator GetGoodPosition()
    {
        _isWinning = true;
        isReady = false;

        ChangeText("Waiting for", "other modules...");

        // This waits for an arbitrary amount of time, to let other copies of this module through at different rates.
        yield return new WaitForSecondsRealtime((ModuleIdCounter - ModuleId) / Mathf.PI);

        // This waits until another module that uses threads in this exact method is finished.
        yield return new WaitWhile(() => _isUsingThreads);

        _isUsingThreads = true;

        bool isEvaluating = false;

        string position = "";

        ChangeText("Preparing...", "Please wait.");

        var game = new CGameResult { };

        new Thread(() =>
        {
            // Find a game that takes 6-8 moves to complete in ideal play, using the Rust library.
            while (true)
            {
                position = RandomPosition;
                isEvaluating = true;
                game = Engine.Calculate(position, Position.Depth, true);

                if (!Mathf.Abs(game.Evaluation).InRange(112, 116))
                    continue;

                // Sometimes the evaluation is in favor of black, we need to advance the game by one move so that it is black to move.
                if (game.Evaluation < 0)
                    position = game.Move(position);

                // Set the player side to always whichever one is winning.
                color = (PieceColor)Convert.ToInt32(game.Evaluation < 0);

                var colorMut = color;
                var moves = new List<CGameResult>();
                string positionMut = position;
                bool isGameCorrectlyOver = false;

                // The bot plays against itself until the perfect game is constructed.
                for (int depth = Position.Depth; depth > 0; depth--)
                {
                    var gameMut = Engine.Calculate(positionMut, depth, colorMut == PieceColor.White);

                    if ((gameMut.Evaluation == sbyte.MaxValue && color == PieceColor.White) ||
                        (gameMut.Evaluation == sbyte.MinValue && color == PieceColor.Black))
                        isGameCorrectlyOver = true;

                    try
                    {
                        positionMut = gameMut.Move(positionMut);
                    }
                    // There are no moves to play when this exception is triggered.
                    catch (IndexOutOfRangeException)
                    {
                        break;
                    }

                    colorMut = colorMut.Flip();

                    moves.Add(gameMut);
                }

                // Ensures that what it logs is indeed a checkmate. This reverifies that the puzzle is possible.
                if (isGameCorrectlyOver && moves.Count == (128 - Math.Abs(game.Evaluation) - (color == PieceColor.White ? 1 : 0)))
                {
                    Log("The position is {0}; {1} to play, mate in {2}. To beat Rustmate, the best sequence of moves are {3}.", position, color, (128 - Math.Abs(game.Evaluation)) / 2, ToLog(moves));
                    break;
                }
            }

            this.position = position;

            souvenirPositions = new List<string>();

            isReady = true;
        }).Start();

        // As long as the thread is running, it should generate and render random positions to distract the player.
        while (!isReady)
        {
            yield return new WaitUntil(() => isEvaluating || isReady);
            isEvaluating = false;

            RenderPosition(position);

            Get<KMAudio>().Play(transform, new[] { Sounds._1dch.Capture, Sounds._1dch.Check, Sounds._1dch.Opponent, Sounds._1dch.Self }.PickRandom());
        }

        MovesLeft = (128 - Math.Abs(game.Evaluation)) / 2;

        _isUsingThreads = false;

        Get<KMAudio>().Play(transform, Sounds._1dch.GameStart);

        ChangeText("Mate in", "", MovesLeft.ToString());
    }

    private IEnumerator HandleStrike(string title, string subtitle)
    {
        Get<KMAudio>().Play(transform, Sounds._1dch.GameEnd);
        Get<KMAudio>().Play(transform, Sounds._1dch.Strike);

        ChangeText(title, subtitle);

        isReady = false;

        yield return new WaitForSecondsRealtime(3);

        Strike("Rustmate has bested you with: {0} That's a strike and reset!".Form(subtitle));

        StartCoroutine(GetGoodPosition());
    }

    private void ChangeText(string title, string subtitle, string movesLeft = "")
    {
        Title.text = title;
        Subtitle.text = subtitle;
        MovesLeftText.text = movesLeft;
    }

    private void RenderPosition(string position)
    {
        for (int i = 0; i < ButtonRenderers.Length; i++)
        {
            if (i >= position.Length)
            {
                // Makes the current tiles invisible.
                BoardRenderers[i].transform.localScale = new Vector3(0, 0, 0);
                continue;
            }

            // Makes the current tiles visible.
            BoardRenderers[i].transform.localScale = new Vector3(50, 50, 50);

            ButtonRenderers[i].material.mainTexture = ButtonTextures[Position.PieceChars.IndexOf(position[i])];
            BoardRenderers[i].material.color = _colorScheme[i % 2];
        }
    }

    private void Panic(string message, Exception exception)
    {
        Log("Oops! Looks like Rustmate didn't show up! You can claim a win by selecting the module. ({0})".Form(message));
        Log("Exception: {0}".Form(exception));

        ChangeText("Rustmate", "failed to load!");

        Get<KMSelectable>().Assign(onInteract: () =>
        {
            if (!IsSolved)
                Solve("Module was selected, that's a lousy solve.");
        });
    }

    private void BoardTiling()
    {
        Texture texture;

        for (int i = 0; i < BoardRenderers.Length; i++)
        {
            // This prevents a tile from looking the same as its 2 previous neighbours.
            do texture = BoardTextures.PickRandom();
            while (i >= 2 && (
                BoardRenderers[i - 1].material.mainTexture == texture ||
                BoardRenderers[i - 2].material.mainTexture == texture));

            BoardRenderers[i].material.mainTexture = texture;
        }
    }

    private string ToLog(List<CGameResult> moves)
    {
        return moves.Select(m => ToLog(m)).Join(", ");
    }

    private string ToLog(CGameResult move)
    {
        try
        {
            string log = "{0} {1}→{2}".Form(move.Piece.Symbol(), Alphabet[move.Origin], Alphabet[move.Destination]);
            return move.Piece.Color == PieceColor.White ? log : "[{0}]".Form(log);
        }
        catch (IndexOutOfRangeException)
        {
            return "<unknown>";
        }
    }
}
