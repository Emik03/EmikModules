using EmikBaseModules;
using OneDimensionalChess;
using System.Collections;
using System.Linq;
using System.Threading;

public class OneDimensionalChessTPScript : TPScript 
{
    public OneDimensionalChessScript Module;
    public override ModuleScript ModuleScript { get { return Module; } }

#pragma warning disable 414
    new private string TwitchHelpMessage = @"!{0} <##> (# is a-h) | Moves piece in first character to second character. | Example: !{0} ab";
#pragma warning restore 414

    public override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;

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

    public override IEnumerator TwitchHandleForcedSolve()
    {
        while (!Module.IsSolve)
        {
            var game = new CGameResult { };

            bool isReady = false;

            while (!Module.isReady)
                yield return true;

            new Thread(() => 
            {
                game = Engine.Calculate(Module.position, Module.movesLeft * 2, Module.color == PieceColor.White);
                isReady = true;
            }).Start();

            while (!isReady)
                yield return true;

            int[] indices = new[] { (int)game.SuggestedMove.Origin, game.SuggestedMove.Destination };


            if (indices.Any(i => i == -1))
                Module.Module.HandlePass();
            else
                yield return OnInteractSequence(Module.Buttons, indices, 1 / 64f);
        }
    }
}
