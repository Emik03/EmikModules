using QuaverModule;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class RenderScript : MonoBehaviour
{
    public Renderer Cover, JudgementRenderer;
    public TextMesh SpeedText, GameplayDifficulty, GameplayMods, GameplayScroll;
    public Texture[] DifficultyTextures, PerColumnTextures;
    public Transform Gameplay, Selection, Speed, Timer, SongProgress, RatingProgress, UISelect, Judgement;
    public QuaverScript Quaver;

    internal int timer, scrollSpeed;
    internal float songProgress, ratingProgress, uiSelectPositionOffset;
    internal static float judgement;

    private bool transition;
    private byte alpha;
    private const int easeIntensity = 8;
    private const float speedEdges = 3f;
    private static readonly Vector3 show = new Vector3(-0.025f, 0.0152f, 0),
                                    hide = new Vector3(-0.025f, 0.0148f, 0);

    private void FixedUpdate()
    {
        Cover.material.color = new Color32(0, 0, 0, alpha);

        float destination = (Quaver.init.select.speed * speedEdges / 5) - speedEdges;
        float position = Speed.localPosition.x + ((destination - Speed.localPosition.x) / easeIntensity);
        Speed.localPosition = new Vector3(position, 0.0002f, 2.64f);

        SongProgress.localScale = new Vector3(SongProgress.localScale.x + (songProgress - SongProgress.localScale.x) / (float)Math.Pow(easeIntensity, 2), 1, 1);
        RatingProgress.localScale = new Vector3(RatingProgress.localScale.x + (Math.Min(ratingProgress, 1) - RatingProgress.localScale.x) / (float)Math.Pow(easeIntensity, 2), 1, 1);

        destination = (-3.35f * Quaver.init.select.ui) + 2.5f;
        position = UISelect.localPosition.z + ((destination - UISelect.localPosition.z) / easeIntensity);
        UISelect.localPosition = new Vector3(-4.35f, 0.0002f, position + ((float)Math.Sin(uiSelectPositionOffset += 0.05f) / 20));

        if (Quaver.init.moduleId == Init.moduleIdCounter)
        {
            judgement = Math.Max(judgement - 0.2f, 0);
            Judgement.localPosition = new Vector3(0, 0.0002f, -judgement / 3);
            JudgementRenderer.material.color = new Color32(255, 255, 255, (byte)(Math.Sqrt(judgement) * 255));
        }

        if (alpha > 0)
            alpha--;

        if (timer > 0)
        {
            timer--;
            Timer.localScale = new Vector3((float)timer / 100 * 0.99f, 0.1f, 0.03f);

            if (timer == 0)
            {
                Quaver.init.gameplay = false;
                Init.anotherQuaverReady = false;
                Quaver.init.ready = false;

                StartCoroutine(Transition());
            }
        }
    }

    internal void UpdateReceptorTotalText(int temp = -1)
    {
        if (temp == -1)
            temp = (int.Parse(Quaver.ReceptorTotalText.text) + 1) % (Quaver.init.select.difficulty == 3
                                                                  ? Quaver.init.select.perColumn ? 100 : 400
                                                                  : 250);

        Quaver.ReceptorTotalText.text = temp.ToString();
        Quaver.ReceptorTotalText.fontSize = (temp * 3) + 200;
        Quaver.ReceptorTotalText.color = new Color32(255, (byte)Math.Max(255 - (temp * 2), 0), (byte)Math.Max(255 - (temp * 2), 0), 255);
    }

    internal void CreateArrow(int i, int j)
    {
        if (i > 5 || i < 0)
            throw new IndexOutOfRangeException("Recieved " + i + ", Expected: 0-5");
        var obj = Instantiate(Quaver.Arrow, Quaver.ArrowBase.transform, false);
        obj.name = Quaver.init.select.difficulty == 3 ? i.ToString() + 'x' + j.ToString() : i.ToString() + j.ToString();
    }

    internal void UpdateSelection()
    {
        Quaver.DifficultyRenderer.material.mainTexture = DifficultyTextures[Quaver.init.select.difficulty];
        Quaver.PerColumnRenderer.material.mainTexture = PerColumnTextures[System.Convert.ToByte(Quaver.init.select.perColumn)];

        int num = Quaver.init.select.speed;
        SpeedText.text = num > 9 ? "2.0" : "1." + num;

        if (Quaver.init.gameplay)
            StartCoroutine(Quaver.init.generate.Play(this));
    }

    internal IEnumerator Transition()
    {
        yield return new WaitWhile(() => transition);
        transition = true;
        bool b = Quaver.init.gameplay;

        if (!b)
        {
            bool isCorrect = Quaver.init.generate.Validate();
            Quaver.Audio.PlaySoundAtTransform(SFX.Q.Submit(isCorrect), transform);

            if (!isCorrect)
            {
                if (Quaver.init.select.perColumn)
                    for (int j = 0; j < Quaver.ReceptorTexts.Length; j++)
                        Quaver.ReceptorTexts[j].text = (int.Parse(Quaver.ReceptorTexts[j].text) - ArrowScript.arrowsPerColumn[j]).ToString();

                else
                    Quaver.ReceptorTotalText.text = (int.Parse(Quaver.ReceptorTotalText.text) - ArrowScript.arrowsPerColumn.Sum()).ToString();
            }

            for (byte i = 255; i > 0; i -= 15)
            {
                for (int j = 0; j < Quaver.ReceptorTexts.Length; j++)
                {
                    bool isCorrectOnColumn = Quaver.ReceptorTexts[j].text == ArrowScript.arrowsPerColumn[j].ToString();
                    Quaver.ReceptorTexts[j].color = new Color32(isCorrectOnColumn ? i : (byte)255, isCorrectOnColumn ? (byte)255 : i, isCorrectOnColumn ? (byte)255 : i, 255);
                }

                Quaver.ReceptorTotalText.color = new Color32(isCorrect ? i : (byte)255, isCorrect ? (byte)255 : i, isCorrect ? (byte)255 : i, 255);

                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSecondsRealtime(0.5f);

            for (int j = 0; j < Quaver.ReceptorTexts.Length; j++)
                Quaver.ReceptorTexts[j].color = new Color32(255, 255, 255, 255);

            Quaver.init.generate.OnSubmit(isCorrect);
        }

        Quaver.init.canAdjustScroll = true;

        alpha = 255;
        songProgress = 0;

        if (b)
        {
            string[] difficulty = new string[] { "Normal", "Hard", "Insane", "Expert" };
            GameplayDifficulty.text = "Difficulty: " + difficulty[Quaver.init.select.difficulty];

            GameplayMods.text = "Mods: ";
            if (Quaver.init.select.perColumn)
                GameplayMods.text += "PC, ";

            int num = Quaver.init.select.speed;
            GameplayMods.text += num > 9 ? "2.0" : "1." + num;
            if (GameplayMods.text == "1.0")
                GameplayMods.text += "None";

            GameplayScroll.text = "Scroll Speed: " + ArrowScript.scrollSpeed;
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Gameplay.localPosition = b ? show : hide;
        Selection.localPosition = b ? hide : show;
        transition = false;

        if (b)
        {
            GameplayDifficulty.color = new Color32(254, 254, 254, 255);
            GameplayMods.color = new Color32(254, 254, 254, 255);
            GameplayScroll.color = new Color32(254, 254, 254, 255);

            yield return new WaitUntil(() => alpha == 0);

            for (byte i = 255; i > 0; i -= 5)
            {
                GameplayDifficulty.color = new Color32(i, i, i, i);
                GameplayMods.color = new Color32(i, i, i, i);
                GameplayScroll.color = new Color32(i, i, i, i);
                yield return new WaitForFixedUpdate();
            }

            GameplayDifficulty.color = new Color32();
            GameplayMods.color = new Color32();
            GameplayScroll.color = new Color32();
        }

        Quaver.init.canAdjustScroll = false;
    }
}
