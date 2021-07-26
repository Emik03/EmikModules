using KeepCoding;
using System;
using System.Collections;

public class TemplateTPScript : TPScript<TemplateScript>
{
    public override IEnumerator ForceSolve()
    {
        throw new NotImplementedException();
    }

    public override IEnumerator Process(string command)
    {
        yield return YieldUntil(true, () => Module.IsSolved);
    }
}
