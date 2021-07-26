using KeepCoding;
using System.Collections;

public class CoinageTPScript : TPScript<CoinageScript>
{
    public override IEnumerator Process(string command)
    {
        yield return null;

        const string FirstChars = "abcdefgh",
            SecondChars = "87654321";

        if (command.Length != 2)
            yield return SendToChatError("Expected only 2 characters.");

        else if (!FirstChars.Contains(command[0].ToLower().ToString()))
            yield return SendToChatError("Expected first character to be a-h.");

        else if (!SecondChars.Contains(command[1].ToLower().ToString()))
            yield return SendToChatError("Expected second character to be 1-8.");

        else
            yield return new[] { Module.Coins[FirstChars.IndexOf(command[0].ToLower()) + (SecondChars.IndexOf(command[1].ToLower()) * 8)] };
    }

    public override IEnumerator ForceSolve()
    {
        Module.Coins[Module.GetExampleAnswer()].OnInteract();

        while (!Module.IsSolved)
            yield return true;
    }
}
