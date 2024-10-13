using KeepCoding.v13;
using System.Collections;
using UnityEngine;

public class PointlessMachinesTPScript : TPScript<PointlessMachinesScript>
{
    protected override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        yield return Solve;
    }

    protected override IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        yield return Solve;
    }
}
