using EmikBaseModules;
using OneDimensionalChess;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;

public class OneDimensionalChessTPScript : TPScript 
{
    public OneDimensionalChessScript Module;

#pragma warning disable 414
    new private string TwitchHelpMessage = @"!{0} <##> (# is a-h) | Moves piece in first character to second character. | Example: !{0} ab";
#pragma warning restore 414

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
                yield return new WaitForSecondsRealtime(1);
            }

            new Thread(() => 
            {
                game = Engine.Calculate(Module.position, Module.movesLeft * 2, Module.color == PieceColor.White);
                isReady = true;
            }).Start();

            while (!isReady)
                yield return true;

            int[] indices = new[] { (int)game.SuggestedMove.Origin, game.SuggestedMove.Destination };

            if (indices.Any(i => i == -1))
                Module.Solve("The autosolver seemed to trip up a bit there. Force-solving now.");
            else
                yield return OnInteractSequence(Module.Buttons, indices, 1 / 64f);
        }
    }
}
