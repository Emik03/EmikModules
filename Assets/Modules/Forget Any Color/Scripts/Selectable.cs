using System;
using UnityEngine;

namespace ForgetAnyColor
{
    /// <summary>
    /// Handles the KMSelectables present in the module. This is where strike/solve logic exists.
    /// </summary>
    public class Selectable
    {
        public Selectable(Calculate calculate, ForgetAnyColorCoroutineScript coroutine, FACScript FAC, Init init, Render render)
        {
            this.calculate = calculate;
            this.coroutine = coroutine;
            this.FAC = FAC;
            this.init = init;
            this.render = render;
        }

        internal bool strike, hasInteracted;
        internal int stagesCompleted;

        private readonly Calculate calculate;
        private readonly ForgetAnyColorCoroutineScript coroutine;
        private readonly FACScript FAC;
        private readonly Init init;
        private readonly Render render;

        internal KMSelectable.OnInteractHandler Interact(byte index)
        {
            return delegate ()
            {
                if (coroutine.animating)
                    return false;

                var seq = calculate.modifiedSequences;

                switch (index)
                {
                    case 0:
                    case 1:
                        FAC.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, FAC.Selectables[index].transform);
                        FAC.Selectables[index].AddInteractionPunch();

                        if (init.solved)
                            return false;

                        hasInteracted = true;

                        if (seq.Count > 0)
                            if (seq[0] == Convert.ToBoolean(index))
                            {
                                FAC.Audio.PlaySoundAtTransform(SFX.Fac.Stage(stagesCompleted % 4), FAC.Selectables[index].transform);
                                stagesCompleted++;
                                seq.RemoveAt(0);
                            }
                            else
                            {
                                Debug.LogFormat("[Forget Any Color #{0}]: {1} was incorrectly pushed during stage {2}.", init.moduleId, index == 1 ? "Right" : "Left", stagesCompleted + 1);

                                FAC.Audio.PlaySoundAtTransform(SFX.Fac.Strike, FAC.Selectables[index].transform);
                                strike = true;
                                FAC.Module.HandleStrike();
                            }

                        coroutine.StartFlash();
                        break;

                    case 2:
                        if (seq.Count == 0 && init.currentStage / Init.modulesPerStage == init.finalStage / Init.modulesPerStage && !init.solved)
                        {
                            FAC.StartCoroutine(render.SolveAnimation());
                            break;
                        }
                        else if (!render.turnKey)
                        {
                            if (!hasInteracted)
                            {
                                Init.modulesPerStage = Math.Max(--Init.modulesPerStage, 1);
                                FAC.StartCoroutine(render.SetDisplayAsStages());
                            }

                            FAC.Audio.PlaySoundAtTransform(SFX.Ftc.Key, FAC.Module.transform);
                            render.turnKey = true;
                        }
                        break;
                }

                return false;
            };
        }
    }
}
