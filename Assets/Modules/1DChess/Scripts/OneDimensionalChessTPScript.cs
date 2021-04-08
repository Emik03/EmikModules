using KeepCodingAndNobodyExplodes;
using OneDimensionalChess;
using System.Collections;
using System.Linq;
using System.Threading;

public class OneDimensionalChessTPScript : TPScript<OneDimensionalChessScript>
{
    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;

        // This cancels any selected square prior.
        if (Module.last != null)
            Module.Buttons[(int)Module.last].OnInteract();

        command = command.ToLowerInvariant();

        bool[] stop = { command.Length != 2, command.Any(c => !Module.Alphabet.Contains(c)) };

        yield return Evaluate(stop[0], SendToChatError("Expected only 2 characters."));
        yield return Evaluate(stop[1], SendToChatError("Expected both characters to be a, b, c, d, e, f, g, h, or i."));

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

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        // This cancels any selected square prior.
        if (Module.last != null)
            Module.Buttons[(int)Module.last].OnInteract();

        while (!Module.IsSolved)
        {
            var game = new CGameResult { };

            bool isReady = false;

            while (!Module.isReady)
            {
                yield return true;
            }

            new Thread(() =>
            {
                game = Engine.Calculate(Module.position, Position.Depth, Module.color == PieceColor.White);
                isReady = true;
            }).Start();

            while (!isReady)
                yield return true;

            int[] indices = new[] { (int)game.Origin, game.Destination };

            if (indices.Any(i => i == -1))
                Module.Solve("The autosolver seemed to trip up a bit there. Force-solving now.");
            else
                yield return OnInteractSequence(Module.Buttons, 1 / 64f, indices);
        }
    }
}
