using KeepCoding;
using System;
using System.Collections;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// Initalizer class for Phosphorescence. Create a new instance of this class to initiate the module.
/// </summary>
namespace PhosphorescenceModule
{
    internal class Init
    {
        internal Init(PhosphorescenceScript pho)
        {
            this.pho = pho;
            render = new Render(pho, this);
            select = new Select(pho, this, render);

            Activate();
        }

        internal readonly PhosphorescenceScript pho;
        internal readonly Select select;
        internal readonly Render render;

        /// <summary>
        /// Souvenir Question: "What sequence of buttons were pressed?"
        /// Since there are (usually) multiple answers, having only the submitted word and offset doesn't guarantee knowing button presses.
        /// </summary>
        internal ButtonType[] buttonPresses;

        internal static bool isFirstToGenerate = true;
        internal bool isSolved, isCountingDown, isInSubmission, isSelected, isAnimated, isStriking;
        internal static int moduleIdCounter, streamDelay;
        internal int index;
        internal string solution, submission;

        /// <summary>
        /// The startup method for Init, which gets the module prepared to be interacted with.
        /// </summary>
        internal void Activate()
        {
            // Sets module ID.
            pho.moduleId = ++moduleIdCounter;

            SFX.LogVersionNumber(pho.Module, pho.moduleId);

            // Sets accessibility.
            if (ModSettingsJSON.LoadMission(pho, ref render.cruelMode, ref streamDelay))
                ModSettingsJSON.Get(pho, out render.cruelMode, out streamDelay);

            UpdateCruel();

            // This allows TP to read this class.
            pho.TP.Activate(this);

            // Plays voice lines only if it is the last one initiated. Not checking this causes multiple sounds to stack up.
            pho.Info.OnBombSolved += () => { if (pho.moduleId == moduleIdCounter) pho.PlaySound(SFX.Pho.Voice.BombDisarmed); };
            pho.Info.OnBombExploded += () => { if (pho.moduleId == moduleIdCounter) pho.PlaySound(SFX.Pho.Voice.GameOver); };

            pho.Number.OnInteract += select.NumberPress();
            pho.Color.OnInteract += select.ColorPress();
            pho.Buttons.OnInteractArray(select.ButtonPress);

            // Initalize markers, and use OnDefocus.
            if (!ModuleScript.IsVR)
            {
                pho.Color.OnDefocus += select.ColorRelease();
                pho.Markers.OnInteractArray(select.MarkerPress);
            }

            // Otherwise, remove markers and use OnInteractEnded.
            else
            {
                pho.Color.OnInteractEnded += select.ColorRelease();

                foreach (var highlight in pho.MarkerHighlightables)
                    highlight.transform.localPosition = new Vector3(highlight.transform.localPosition.x, -0.5f, highlight.transform.localPosition.z);
            }
        }

        /// <summary>
        /// Sets cruel mode to the tile textures, based on the cruel mode variable.
        /// </summary>
        private void UpdateCruel()
        {
            foreach (var tile in pho.Tiles)
                tile.material.mainTexture = render.cruelMode ? null : pho.TileTexture;
        }

        /// <summary>
        /// This method is called when the module is about to strike, but needs to wait until the animations are finished first.
        /// The small delay does make the module a bit more forgiving, as it will revalidate the answer before properly striking.
        /// </summary>
        internal IEnumerator BufferStrike()
        {
            yield return new WaitWhile(() => isAnimated);

            // The method causes another instance of Strike() to run, or solve if the submission is correct.
            if (isInSubmission)
                yield return select.animate.ExitSubmit();

            else
                yield return Strike();
        }

        /// <summary>
        /// Strikes the module, with a sparratic screen animation.
        /// </summary>
        private IEnumerator Strike()
        {
            if (isStriking)
                yield break;

            isStriking = true;
            isAnimated = true;

            Debug.LogFormat("[Phosphorescence #{0}]: Submission \"{1}\" did not match the expected \"{2}\"!", pho.moduleId, submission, solution);
            solution = string.Empty;

            pho.PlaySound(SFX.Pho.Strike);

            // Disable screen.
            render.UpdateDisplay(0);
            isCountingDown = false;

            pho.Module.HandleStrike();
            const ButtonType color = ButtonType.Red;

            // Increase in amount of reds.
            for (int i = 0; i <= 25; i++)
            {
                foreach (var tile in pho.Tiles)
                    tile.material.color = Rnd.Range(0, 25) >= i ? Color.black : color.GetColor();

                yield return new WaitForSecondsRealtime(0.02f);
            }

            // Decrease in amount of reds.
            for (int i = 0; i <= 25; i++)
            {
                foreach (var tile in pho.Tiles)
                    tile.material.color = Rnd.Range(0, 25) >= i ? color.GetColor() : Color.black;

                yield return new WaitForSecondsRealtime(0.02f);
            }

            isAnimated = false;
        }

        /// <summary>
        /// Gets called when module is about to solve, plays solve animation.
        /// </summary>
        internal IEnumerator Solve()
        {
            isSolved = true;
            Debug.LogFormat("[Phosphorescence #{0}]: The submisssion was correct, that is all.", pho.moduleId);
            pho.PlaySound(SFX.Pho.Success);

            // Removes the texture, since it doesn't matter at this stage.
            foreach (var tile in pho.Tiles)
                tile.material.mainTexture = null;

            // Keeps track of the current states of the screen.
            // This needs to be a 1-dimensional array for it to easily align with the Renderer array.
            int[] displayStates = new int[49];

            // Sets all text to be a light shade of green.
            foreach (var text in pho.ScreenText)
                text.color = new Color32(98, 196, 98, 255);

            // Flashes all colors.
            for (int i = 0; i < Enum.GetNames(typeof(ButtonType)).Length; i++)
            {
                foreach (var tile in pho.Tiles)
                    tile.material.color = ((ButtonType)i).GetColor();

                yield return new WaitForSeconds(0.1f);
            }

            // Solves the module.
            pho.PlaySound(SFX.Pho.Voice.ChallengeComplete);
            pho.Module.HandlePass();
            yield return select.animate.PostSolve(pho, displayStates);
        }
    }
}
