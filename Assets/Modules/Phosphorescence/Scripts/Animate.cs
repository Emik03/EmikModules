using PhosphorescenceModule;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// This class handles any "pure" animations.
/// Every method in here is an IEnumerator which is meant to be called by Phosporescence itself.
/// </summary>
namespace PhosphorescenceModule
{
    internal class Animate
    {
        internal Animate(PhosphorescenceScript pho, Init init, Select select, Render render)
        {
            _pho = pho;
            _init = init;
            _select = select;
            _render = render;
        }

        private readonly PhosphorescenceScript _pho;
        private readonly Init _init;
        private readonly Select _select;
        private readonly Render _render;

        /// <summary>
        /// Used to cancel animations already running relating to button presses, such that IEnumerators don't clash with each other.
        /// </summary>
        private bool _isPushingButton;

        internal IEnumerator Run()
        {
            _init.isStriking = false;
            _init.isAnimated = true;

            if (Init.isFirstToGenerate)
                _pho.StartCoroutine(_pho.WordList.Init());

            _pho.StartCoroutine(Startup());
            yield return new WaitWhile(() => Words.ValidWords == null || _init.isAnimated);

            _init.isCountingDown = true;

            // Most of the time this will only need to run once. This is a failsafe to make sure that there are at least 3 answers.
            do _init.index = Rnd.Range(0, Words.ValidWords.GetLength(0));
            while (Words.ValidWords[_init.index].Length < Words.MinAcceptableWordSet);

        restart:
            // Pick any solution from the current index.
            _init.solution = Words.ValidWords[_init.index].PickRandom();

            Function.AllButtonTypes.ToArray().GetAllAnswers(_init.solution, _init.index).PickRandom().GenerateColoredButtons(out _select.buttons);
            string[] answers = _select.buttons.GetAllAnswers(_init.solution, _init.index);

            // I really don't want answers to contain this anywhere.
            if (answers.Contains("FAG") || answers.Contains("NIG"))
                goto restart;

            // Log the current answer.
            Debug.LogFormat("[Phosphorescence #{0}]: The buttons available are {1}.", _pho.moduleId, _select.buttons.Join(", "));
            Debug.LogFormat("[Phosphorescence #{0}]: The expected submission is {1}, deriving from the starting offset {2}.", _pho.moduleId, _init.solution, _init.index);
            Debug.LogFormat("[Phosphorescence #{0}]: All possible answers ({1}) are: {2}.", _pho.moduleId, answers.Length, answers.Join(", "));

            _pho.StartCoroutine(_render.Countdown());

            // This makes sure that the user doesn't accidentally press the buttons in the background as they use the markers.
            _pho.ButtonGroupRenderer.localScale = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// Transitions the module to an active state, by first playing an animation.
        /// </summary>
        internal IEnumerator Startup()
        {
            _pho.PlaySound(Sounds.Pho.Start);

            // This makes the display darker, since it always returns 0 in binary.
            _init.index = 0;

            // These variables have to reset so that the first press is always a reshuffle.
            _render.currentIndex = int.MaxValue - 1;

            float solved = _pho.Info.GetSolvedModuleNames().Count,
                  solvable = _pho.Info.GetSolvableModuleNames().Count,
                  deltaSolved = solved + 1 == solvable ? 1 : solved / solvable;

            // The more modules are solved, the more time given.
            int currentTime = Init.streamDelay + 300 + (int)(deltaSolved * 300);

            // Because of user-inputted mod settings, we need to make sure it doesn't go outside the confines of the 7-segment display.
            Render.currentTime = Mathf.Max(Mathf.Min(currentTime, 5999), 10);

            // This loop runs an animation of the timer increasing rapidly.
            for (int i = 0; i < Render.currentTime; i += (int)Mathf.Ceil((float)Render.currentTime / 100))
            {
                _render.UpdateDisplay(i);
                yield return new WaitForSecondsRealtime(0.02f);
            }

            _init.isAnimated = false;
        }

        /// <summary>
        /// Animation for pressing down the button.
        /// </summary>
        /// <param name="transform">The transform of the button.</param>
        internal IEnumerator PressButton(Transform transform)
        {
            // While messy, this ensures that any button already pushed will quit their animation.
            _init.isAnimated = _isPushingButton = true;
            yield return new WaitForSecondsRealtime(0.1f);
            _isPushingButton = _init.isAnimated = false;

            // ElasticIn ease of the button being pushed down.
            float k = 1;
            while (k > 0 && !_isPushingButton)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, -2 * Function.ElasticIn(k), transform.localPosition.z);
                k -= 0.0078125f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            // Resets the button's position.
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }

        /// <summary>
        /// Flashes all buttons white, fading them into different colors.
        /// </summary>
        internal IEnumerator ResetButtons()
        {
            // Similarily to PressButton, this ensures that any button already pushed will quit their animation.
            _init.isAnimated = _isPushingButton = true;
            yield return new WaitForSecondsRealtime(0.1f);
            _isPushingButton = _init.isAnimated = false;

            // Clear current submission.
            _init.submission = string.Empty;
            _init.buttonPresses = new ButtonType[0];

            _select.ShuffleButtons();
            _pho.PlaySound(Sounds.Pho.ShuffleButtons);
            _render.time -= _render.time / 4;

            // ElasticÍn ease of all buttons being pushed down.
            float k = 1;
            while (k > 0 && !_isPushingButton)
            {
                for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
                {
                    _pho.ButtonRenderers[i].transform.localPosition = new Vector3(_pho.ButtonRenderers[i].transform.localPosition.x, -2 * Function.ElasticIn(k), _pho.ButtonRenderers[i].transform.localPosition.z);
                    _pho.ButtonRenderers[i].SetIntertwinedColor(colorA: _select.buttons[i].GetColor(),
                                                                colorB: Color.white,
                                                                f: Math.Max((k - 0.75f) * 4, 0));
                }

                k -= 0.0078125f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            // Resets all buttons' positions.
            for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
            {
                _pho.ButtonRenderers[i].transform.localPosition = new Vector3(_pho.ButtonRenderers[i].transform.localPosition.x, 0, _pho.ButtonRenderers[i].transform.localPosition.z);
                _pho.ButtonRenderers[i].material.color = Function.GetColor(_select.buttons[i]);
            }

            yield return FadeButtons();
        }

        /// <summary>
        /// This method should get called to fade the buttons to black. Any button push will cut the animation short.
        /// </summary>
        internal IEnumerator FadeButtons()
        {
            yield return new WaitForSecondsRealtime(0.02f);

            // Gradually turns the buttons black.
            float k = 0;
            while (k <= 1 && !_isPushingButton)
            {
                for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
                    _pho.ButtonRenderers[i].SetIntertwinedColor(colorA: _select.buttons[i].GetColor(),
                                                                colorB: Color.black,
                                                                f: k);

                k += 0.001953125f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            // Sets them to black immeditely.
            for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
                _pho.ButtonRenderers[i].material.color = Color.black;
        }

        /// <summary>
        /// Covers the transition between an active state to an active submission state.
        /// </summary>
        internal IEnumerator EnterSubmit()
        {
            _init.isAnimated = true;
            _pho.StartCoroutine(FadeButtons());

            // ElasticOut ease of screen going down.
            float k = 1;
            while (k > 0)
            {
                _pho.Screen.transform.localPosition = new Vector3(-0.015f, (0.02f * Function.ElasticOut(k)) - 0.015f, -0.016f);
                k -= 0.015625f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            // ElasticOut ease of buttons going up.
            k = 0;
            while (k <= 1)
            {
                _pho.Panel.transform.localPosition = new Vector3(0, (0.035f * Function.ElasticOut(k)) - 0.025f, 0);
                k += 0.00390625f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            _init.isAnimated = false;
        }

        /// <summary>
        /// Covers the transition between an active submission state to a validation state.
        /// </summary>
        internal IEnumerator ExitSubmit()
        {
            _init.isAnimated = true;
            _init.isCountingDown = false;

            _pho.PlaySound(Sounds.Pho.EndSubmit);

            // ElasticOut ease of buttons going down.
            float k = 0;
            while (k <= 1)
            {
                _pho.Panel.transform.localPosition = new Vector3(0, (0.035f * Function.ElasticOut(1 - k)) - 0.025f, 0);
                k += 0.00390625f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            // ElasticOut ease of screen going up.
            k = 1;
            while (k > 0)
            {
                _pho.Screen.transform.localPosition = new Vector3(-0.015f, (0.02f * Function.ElasticOut(1 - k)) - 0.015f, -0.016f);
                k -= 0.015625f;
                yield return new WaitForSecondsRealtime(0.01f);
            }

            // Validation check.
            if (_init.solution == _init.submission)
                _pho.StartCoroutine(_init.Solve());
            else
                _pho.StartCoroutine(_init.BufferStrike());

            // Make the module inactive.
            _init.isAnimated = false;
            _init.isInSubmission = false;
            _init.isSelected = false;

            _pho.ButtonGroupRenderer.localScale = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// An animation used after the module is solved. This runs indefinitely.
        /// </summary>
        /// <param name="displayStates">An array that matches with pho's display renderer array.</param>
        internal IEnumerator PostSolve(PhosphorescenceScript pho, int[] displayStates)
        {
            while (true)
            {
                // For each corner.
                for (int i = 0; i < 4; i++)
                {
                    Color32 color = ((ButtonType)Rnd.Range(0, Enum.GetNames(typeof(ButtonType)).Length)).GetColor();
                    // 20 is an arbitrary number that works well here.
                    for (int j = 0; j < 20; j++)
                    {
                        // For each display.
                        for (int k = 0; k < displayStates.Length; k++)
                        {
                            // Should the screen change color right now?
                            if ((i % 4 == 0 && (k % 7) + (k / 7) == j) || // Top-left
                                (i % 4 == 3 && (k % 7) + (7 - (k / 7)) == j) || // Bottom-left
                                (i % 4 == 1 && 7 - (k % 7) + (k / 7) == j) || // Top-right
                                (i % 4 == 2 && 7 - (k % 7) + (7 - (k / 7)) == j)) // Bottom-right
                                pho.Tiles[k].material.color = color;
                        }

                        yield return new WaitForSecondsRealtime(0.2f);
                    }
                }
            }
        }
    }
}
