using KeepCoding;
using UnityEngine;

public class TemplateScript : ModuleScript 
{
    [SerializeField]
    private TextAsset _text;

    private void Start()
    {
        Log(_text.text.Split('\n'));
    }
}
