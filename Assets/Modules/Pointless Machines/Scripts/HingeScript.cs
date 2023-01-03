using KeepCoding;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeScript : MonoBehaviour
{
    private static readonly Color32 _color = new Color32(20, 29, 47, 255);
    private Renderer _renderer;
    private Routine _routine;
    private Vector3 _scale;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _routine = new Routine(Animate, this);
        _scale = transform.localScale;
    }

    internal void Flash()
    {
        _routine.StartOrRestart();
    }

    private IEnumerator Animate()
    {
        float f = 0;

        while (f < 1)
        {
            f += Time.deltaTime;

            float ease = 1 - Easing.InOutExpo(f, 0, 1, 1);

            _renderer.material.color = new Color32(
                (byte)(_color.r + _color.r * ease * 4), 
                (byte)(_color.g + _color.g * ease * 4), 
                (byte)(_color.b + _color.b * ease * 4), 
                0);
            transform.localScale = _scale + _scale * ease / 2;

            yield return null;
        }

        _renderer.material.color = _color;
        transform.localScale = _scale;
    }
}

/// <summary>
/// Toki Pona is a constructed language by jan Sonja created in 2001 and later published as the book "Toki Pona: The Language of Good" (2001).
/// It is a minimalistic language with a focus on vocabulary and grammar.
/// It is also an engineered language, with a focus on vocabulary and grammar.
/// </summary>
public sealed class TokiPona
{
    /// <summary>
    /// Translates any english transcript into toki pona.
    /// </summary>
    /// <param name="englishText">The english text to translate.</param>
    /// <returns>A <see cref="string"/> representing the translation of <paramref name="englishText"/> in toki pona from English.</returns>
    public string Translate(string englishText)
    {
        // Dictionary of words
        Dictionary<string, string> words = new Dictionary<string, string>();

        words.Add("language", "toki");
        words.Add("good", "pona");
        words.Add("power", "wawa");
        words.Add("emotion", "pilin");
        words.Add("creator", "mama");
        words.Add("knowledge", "sona");
        words.Add("color", "kule");
        words.Add("time", "tenpo");
        words.Add("space", "enko");
        words.Add("mind", "lawa");
        words.Add("body", "sijelo");
        words.Add("finish", "pini");

        return Translate(englishText, words);
    }

    private static string Translate(string englishText, Dictionary<string, string> words)
    {
        string[] englishWords = englishText.Split(' ');
        string[] tokiPonaWords = new string[englishWords.Length];

        for (int i = 0; i < englishWords.Length; i++)
        {
            string englishWord = englishWords[i];
            string tokiPonaWord = words.ContainsKey(englishWord) ? words[englishWord] : englishWord;
            tokiPonaWords[i] = tokiPonaWord;
        }

        return string.Join(" ", tokiPonaWords);
    }
}
