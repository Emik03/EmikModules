using KeepCoding;
using System.Collections;
using UnityEngine;

public class PointlessMachinesTPScript : TPScript<PointlessMachinesScript>
{
    public override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        yield return Solve;
    }

    public override IEnumerator TwitchHandleForcedSolve()
    {
        foreach (var view in Module.answer)
        {
            Module.HandleView(view);
            yield return new WaitForSecondsRealtime(0.4f);
        }

        while (!Module.IsSolved)
            yield return true;
    }
}
