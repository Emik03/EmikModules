using KeepCoding;
using OneDimensionalChess;
using System.Collections;
using System.Linq;
using System.Threading;

public class OneDimensionalChessTPScript : TPScript<OneDimensionalChessScript>
{
    public override IEnumerator Process(string command)
    {
        yield return null;

        // This cancels any selected square prior.
        if (Module.last.HasValue)
            Module.Buttons[Module.last.Value].OnInteract();

        command = command.ToLowerInvariant();

        bool[] stop = { command.Length != 2, command.Any(c => !Module.Alphabet.Contains(c)) };

        yield return Evaluate(stop[0], SendToChatError("Expected only 2 characters."));
        yield return Evaluate(stop[1], SendToChatError("Expected both characters to be within the range of the board."));

        if (stop.Any(b => b))
            yield break;

        while (!Module.isReady)
            yield return true;

        yield return new[]
        {
            Module.Buttons[Module.Alphabet.IndexOf(command[0])],
            Module.Buttons[Module.Alphabet.IndexOf(command[1])]
        };

        while (!Module.isReady)
            yield return true;

        if (!Module.isWinning)
            yield return Strike;
    }

    public override IEnumerator ForceSolve()
    {
        // This cancels any selected square prior.
        if (Module.last.HasValue)
            Module.Buttons[Module.last.Value].OnInteract();

        if (!Module.isWinning)
            yield return Module.GetGoodPosition();

        while (!Module.IsSolved)
        {
            var game = new CGameResult { };

            bool isReady = false;

            while (!Module.isReady)
                yield return true;

            new Thread(() =>
            {
                game = Engine.Calculate(Module.position, Position.Depth, Module.color == PieceColor.White);
                isReady = true;
            }).Start();

            while (!isReady)
                yield return true;

            int[] indices = new[] { (int)game.Origin, game.Destination };

            // Prevents invalid moves from being played in case if the library returns it.
            if (indices.Any(i => i == -1) || indices.Distinct().Count() == 1)
            {
                Module.Solve("The autosolver seemed to trip up a bit there. Force-solving now.");
                yield break;
            }

            yield return OnInteractSequence(Module.Buttons, 1 / 64f, indices);
        }
    }
}
