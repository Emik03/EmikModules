using KeepCoding;
using System.Collections;
using System.Linq;
using UnityEngine;

public class NetheriteTPScript : TPScript<NetheriteScript>
{
    private const float Wait = 0.2f;

    public override IEnumerator Process(string command)
    {
        string check = "";
        if (Module._is2FA)
            check = KModkit.KMBombInfoExtensions.GetTwoFactorCodes(Module.Get<KMBombInfo>()).Join(".");

        yield return null;

        int[] inputs = command.ToCharArray().ToNumbers(min: 1, max: 9);
        bool[] stop = { inputs == null, inputs.LengthOrDefault() > 11 - Module.Stage };

        yield return Evaluate(stop[0], SendToChatError("One or more of your characters was not a number."));
        yield return Evaluate(stop[1], SendToChatError("This exceeds the number of stages remaining."));

        if (stop.Any(b => b))
            yield break;

        for (int i = 0; i < inputs.Length; i++)
        {
            Module.Buttons[inputs[i] - 1].OnInteract();
            if (i < inputs.Length - 1)
                yield return new WaitForSecondsRealtime(0.2f);
            if (Module._is2FA && check != KModkit.KMBombInfoExtensions.GetTwoFactorCodes(Module.Get<KMBombInfo>()).Join("."))
            {
                yield return SendToChatError("A Two-Factor code changed! {0} inputs were processed.".Form(i+1));
                yield break;
            }
        }
    }

    public override IEnumerator ForceSolve()
    {
        int[] answer = Module.Sequence.Skip(Module.Stage).ToArray();
        for (int i = 0; i < answer.Length; i++)
        {
            Module.Buttons[Enumerable.Range(0, 9).First(b => Module.ApplyRules(b) == answer[i])].OnInteract();
            if (i < answer.Length - 1)
                yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}
