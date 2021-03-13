using EmikBaseModules;
using System.Collections;

public class CoinageTPScript : TPScript
{
    public CoinageScript Module;

#pragma warning disable 414
#pragma warning disable IDE0051 // Remove unused private members
    new private const string TwitchHelpMessage = @"!{0} <a-h><1-8> | Based on chess coordinates, where a-h represent left-to-right and 1-8 represent down-to-up. | Example: !{0} d4";
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore 414

    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;

        const string FirstChars = "abcdefgh",
            SecondChars = "87654321";

        Module.Log(command);

        if (command.Length != 2)
            yield return SendToChatError("Expected only 2 characters.");

        else if (!FirstChars.Contains(command[0].ToLower().ToString()))
            yield return SendToChatError("Expected first character to be a-h.");

        else if (!SecondChars.Contains(command[1].ToLower().ToString()))
            yield return SendToChatError("Expected second character to be 1-8.");

        else
            yield return new[] { Module.Coins[FirstChars.IndexOf(command[0].ToLower()) + (SecondChars.IndexOf(command[1].ToLower()) * 8)] };
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Module.Coins[Module.GetExampleAnswer()].OnInteract();
    }
}
