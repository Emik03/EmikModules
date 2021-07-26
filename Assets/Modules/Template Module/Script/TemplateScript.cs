using KeepCoding;
using System.Linq;
using UnityEngine;

public class TemplateScript : ModuleScript 
{
    private int i;

    private void Start()
    {
        Get<KMSelectable>().Assign(onInteract: () => Debug.Log(++i));
        // 0.For(i => Debug.Log(i), i => i < 10, i => ++i);
    }
}
