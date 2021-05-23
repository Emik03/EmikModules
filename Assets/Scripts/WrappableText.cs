using KeepCoding;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class WrappableText : MonoBehaviour 
{
    public ushort MaxWidth;

    public string Text { get { return _text; } set { _text = value; _textMesh.text = InsertNewlines(value, MaxWidth); } }
    private string _text;

    private TextMesh _textMesh;

    private void Awake()
    {
        _textMesh = GetComponent<TextMesh>();

        if (_textMesh == null)
            throw new MissingComponentException("The type TextMesh could not be found! Ensure that a TextMesh component is on the same game object as this script!");

        Text = _textMesh.text;
    }

    private static string InsertNewlines(string text, ushort maxWidth)
    {
        if (maxWidth < 2)
            throw new ArithmeticException("The max width cannot be 0 as this doesn't allow line breaks to be inserted.");

        var builder = new StringBuilder(new string(text.Select(c => IsWrappable(c) ? ' ' : c).ToArray()));

        int floor = -1;

        for (ushort i = maxWidth; i < text.Length; i--)
        {
            if (floor >= i)
            {
                i = (ushort)((floor += maxWidth + 1) + maxWidth);

                builder.Insert(floor, "\n");
            }

            else if (IsWrappable(text[i]))
            {
                builder[i] = '\n';

                floor = ++i;
                i += maxWidth;
            }
        }

        return builder.ToString();
    }

    private static bool IsWrappable(char c)
    {
        switch (c)
        {
            case '\u00a0':
            case '\u202f':
                return false;
            case '\u200b':
                return true;
            default:
                return char.IsWhiteSpace(c);
        }
    }
}
