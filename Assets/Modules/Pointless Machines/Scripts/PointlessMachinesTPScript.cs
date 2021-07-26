using KeepCoding;
using PointlessMachines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointlessMachinesTPScript : TPScript<PointlessMachinesScript>
{
    private static readonly Dictionary<char, View> _toView = new Dictionary<char, View>()
    {
        { 'l', View.Left },
        { 'd', View.Down },
        { 'u', View.Up },
        { 'r', View.Right },
    };

    public override IEnumerator Process(string command)
    {
        foreach (var view in command
            .ToLowerInvariant()
            .Split()
            .Where(s => 
                s.All(c => "ldur".Contains(c)) && 
                s.Length.IsBetween(1, 2) && 
                !(s.Contains("u") && s.Contains("d") && 
                !(s.Contains("l") && s.Contains("r"))))
            .Select(s => (View)s.Select(c => (int)_toView[c]).Sum()))
            yield return Submit(view);

        yield return Solve;

        yield return Get<KMSelectable>();
    }

    public override IEnumerator ForceSolve()
    {
        foreach (var view in Module.answer)
            yield return Submit(view);

        while (!Module.IsSolved)
            yield return true;
    }

    private IEnumerator Submit(View view)
    {
        Module.HandleView(view);
        yield return new WaitForSecondsRealtime(0.2f);
        Module.HandleView(View.None);
        yield return new WaitForSecondsRealtime(0.2f);
    }
}
