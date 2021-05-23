using ForgetAnyColor;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles running coroutines for Forget Any Color since Unity requires it to be a GameObject.
/// </summary>
public class ForgetAnyColorCoroutineScript : MonoBehaviour
{
    public FACScript FAC;
    public ForgetAnyColorTPScript TP;

    internal bool animating = true, flashing;

    private Calculate calculate;
    private Init init;
    private Render render;

    private int amountOfSolved, gearRot;
    private float gearPos = 0.025f;

    private void Start()
    {
        init = FAC.init;
        calculate = init.calculate;
        render = init.render;
    }

    private void FixedUpdate()
    {
        const int intensity = 5;
        float x = Mathf.Sin(Time.time) * intensity,
              z = Mathf.Cos(Time.time) * intensity;
        FAC.Gear.localRotation = Quaternion.Euler(x, 0, z);

        if (gearPos < 100 && init.currentStage / Init.modulesPerStage == init.finalStage / Init.modulesPerStage)
        {
            gearPos += (gearPos / 20) + 0.0001f;
            gearRot += (gearRot / 10) + 1;
            FAC.Gear.localPosition = new Vector3(FAC.Gear.localPosition.x, gearPos, FAC.Gear.localPosition.z);
            FAC.Gear.localRotation = Quaternion.Euler(FAC.Gear.localRotation.x, gearRot, FAC.Gear.localRotation.z);
        }

        int amountOfSolves = FAC.Info.GetSolvedModuleNames().Where(m => !Arrays.Ignore.Contains(m)).Count();

        if (render.Animate(animating))
        {
            amountOfSolved = amountOfSolves;
            init.currentStage += Init.modulesPerStage;
            init.stage++;
            StartNewStage();
        }
        else if (amountOfSolved != amountOfSolves)
        {
            amountOfSolved = amountOfSolves;
            StartFlash();
        }
    }

    internal void StartFlash()
    {
        if (!flashing)
            StartCoroutine(Flash());
    }

    internal void StartNewStage()
    {
        animating = true;
        StartCoroutine(NewStage());
    }

    private IEnumerator Flash()
    {
        flashing = true;

        const int flash = 2;
        for (int i = 0; i < flash; i++)
        {
            render.AssignRandom(false);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        render.Assign(null, null, null, null, false);
        render.SetNixieAsInputs();

        flashing = false;
    }

    private IEnumerator NewStage()
    {
        const int nextStage = 5, specialStage = 20;
        bool isSpecialStage = init.currentStage / Init.modulesPerStage == 0 || init.currentStage / Init.modulesPerStage == init.finalStage / Init.modulesPerStage;

        render.Colorblind(render.colorblind);

        if (init.moduleId == Init.moduleIdCounter)
        {
            FAC.Audio.PlaySoundAtTransform(SFX.Fac.Next(init.currentStage / Init.modulesPerStage % 4), FAC.Module.transform);
            if (init.currentStage != 0)
                FAC.Audio.PlaySoundAtTransform(SFX.Ftc.NextStage, FAC.Module.transform);
            if (init.currentStage / Init.modulesPerStage == init.finalStage / Init.modulesPerStage)
                FAC.Audio.PlaySoundAtTransform(SFX.Fac.FinalStage, FAC.Module.transform);
        }

        for (int i = 0; i < (isSpecialStage ? specialStage : nextStage); i++)
        {
            render.AssignRandom(false);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        render.AssignRandom(true);

        if (init.currentStage / Init.modulesPerStage == init.finalStage / Init.modulesPerStage)
        {
            render.Assign(null, null, null, null, false);

            Debug.LogFormat("[Forget Any Color #{0}]: {1}{2}.",
                init.moduleId,
                calculate.modifiedSequences.Count > 0 ? "The remaining sequence is " : "There is no sequence. Turn the key",
                string.Join(", ", calculate.modifiedSequences.Select(x => x ? "Right" : "Left").ToArray()));
        }
        else
            calculate.Current();

        animating = false;

        render.Colorblind(render.colorblind);
    }
}
