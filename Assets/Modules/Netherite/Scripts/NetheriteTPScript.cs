using KeepCoding;
using System.Collections;
using System.Linq;

public class NetheriteTPScript : TPScript<NetheriteScript>
{
    private const float Wait = 0.2f;

    public override IEnumerator Process(string command)
    {
        yield return null;

        int[] inputs = command.ToCharArray().ToNumbers(min: 1, max: 9);
        bool[] stop = { inputs == null, inputs.LengthOrDefault() > 11 - Module.Stage };

        yield return Evaluate(stop[0], SendToChatError("One or more of your characters was not a number."));
        yield return Evaluate(stop[1], SendToChatError("This exceeds the number of stages remaining."));

        if (stop.Any(b => b))
            yield break;

        yield return OnInteractSequence(Module.Buttons, Wait, inputs.Select(i => i - 1).ToArray());
    }

    public override IEnumerator ForceSolve()
    {
        int[] answer = Module.Sequence.Select(i => Module.ApplyRules(i - 1) - 1).Skip(Module.Stage).ToArray();
        yield return OnInteractSequence(Module.Buttons, Wait, answer);
    }
}
