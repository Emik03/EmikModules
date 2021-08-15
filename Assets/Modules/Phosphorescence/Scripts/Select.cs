using KeepCoding;
using PhosphorescenceModule;
using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the event triggers.
/// </summary>
namespace PhosphorescenceModule
{
    internal class Select
    {
        internal Select(PhosphorescenceScript pho, Init init, Render render)
        {
            _pho = pho;
            _init = init;
            _render = render;
            animate = new Animate(pho, init, this, render);
        }

        internal readonly Animate animate;

        /// <summary>
        /// Contains current status on the buttons' colors, since colors of the buttons might fade/vary.
        /// </summary>
        internal ButtonType[] buttons;

        private readonly Init _init;
        private readonly PhosphorescenceScript _pho;
        private readonly Render _render;

        /// <summary>
        /// The event handler for pressing the 7-segment display.
        /// </summary>
        /// <returns>Always false, as the button does not have children.</returns>
        internal KMSelectable.OnInteractHandler NumberPress()
        {
            return delegate ()
            {
                PressFeedback(_pho.Number.transform);

                if (_init.isSolved || _init.isAnimated)
                {
                    _pho.PlaySound(SFX.Pho.InvalidButton);
                    return false;
                }

                if (!_init.isCountingDown) // Is it inactive? Make it active.
                    _pho.StartCoroutine(animate.Run());
                else if (!_init.isAnimated) // Is it not animating? Continue.
                    if (_init.isInSubmission) // Is it in submission? Submit and validate.
                        _pho.StartCoroutine(animate.ExitSubmit());
                    else // Otherwise, enter submission.
                        EnterSubmit();
                return false;
            };
        }

        /// <summary>
        /// The event handler for pressing 1 of the 8 colored buttons beneath the module initally.
        /// </summary>
        /// <param name="btn">In reading order, the index in the buttons array that was pushed.</param>
        /// <returns>Always false, as the buttons do not have children.</returns>
        internal KMSelectable.OnInteractHandler ButtonPress(int btn)
        {
            return delegate ()
            {
                PressFeedback(_pho.Number.transform);

                if (_init.isSolved || !_init.isCountingDown || !_init.isInSubmission || _init.isAnimated)
                {
                    _pho.PlaySound(SFX.Pho.InvalidButton);
                    return false;
                }

                // Is the user trying to submit a character?
                else if (_init.submission.Length < 6)
                {
                    // Get the current button and index.
                    string currentSubmit = buttons[btn].ToString();
                    int currentIndex = _init.index + _init.submission.Length;

                    // Append the corresponding character.
                    _init.submission += currentSubmit[currentIndex % currentSubmit.Length].ToString().ToLowerInvariant();

                    // Append the next button press.
                    int oldLength = _init.buttonPresses.Length;
                    Array.Resize(ref _init.buttonPresses, oldLength + 1);
                    _init.buttonPresses[oldLength] = buttons[btn];

                    _pho.PlaySound(SFX.Pho.Submit(_init.submission.Length));
                    _pho.StartCoroutine(animate.PressButton(_pho.ButtonRenderers[btn].transform));
                    return false;
                }

                // Otherwise, clear submission.
                _pho.StartCoroutine(animate.ResetButtons());
                return false;
            };
        }

        /// <summary>
        /// The event handler for pressing any of the markers.
        /// </summary>
        /// <param name="btn">The index of the markers.</param>
        /// <returns>False, as the markers do not have children.</returns>
        internal KMSelectable.OnInteractHandler MarkerPress(int btn)
        {
            return delegate ()
            {
                PressFeedback(_pho.Number.transform, 0.1f);

                if (_init.isAnimated || _init.isInSubmission)
                {
                    _pho.PlaySound(SFX.Pho.InvalidButton);
                    return false;
                }

                bool isNowInvisible = _pho.MarkerRenderers[btn].transform.localScale.x != 0;

                // Inverts their scale, toggling whether they are visible or not.
                _pho.MarkerRenderers[btn].transform.localScale = isNowInvisible ? new Vector3(0, 0, 0) : new Vector3(0.5f, 1, 0.5f);

                _pho.PlaySound(isNowInvisible ? SFX.Pho.MarkerOff : SFX.Pho.MarkerOn);
                return false;
            };
        }

        /// <summary>
        /// The event handler for pressing the bottom display.
        /// </summary>
        /// <returns>The inverse of VRMode, since it might or might not have children. Shrödinger's Children, if you will.</returns>
        internal KMSelectable.OnInteractHandler ColorPress()
        {
            return delegate ()
            {
                ColorRelease();
                PressFeedback(_pho.Color.transform, 2);

                if (_init.isSolved || _init.isAnimated || _init.isInSubmission)
                {
                    _pho.PlaySound(SFX.Pho.InvalidButton);
                    return !ModuleScript.IsVR;
                }

                ResetMarkers();
                _pho.PlaySound(SFX.Pho.ScreenPress);
                _pho.StartCoroutine(_render.UpdateCubes());
                return !ModuleScript.IsVR;
            };
        }

        /// <summary>
        /// The event handler for releasing the display with VRMode enabled. Shuts off the display.
        /// </summary>
        internal Action ColorRelease()
        {
            return delegate ()
            {
                PressFeedback(_pho.Color.transform, 0.5f);

                if (_init.isSolved || _init.isAnimated || _init.isInSubmission)
                {
                    _pho.PlaySound(SFX.Pho.InvalidButton);
                    return;
                }

                _pho.PlaySound(SFX.Pho.ScreenRelease);
                _init.isSelected = false;

                // Sets the entire 7x7 grid to be completely black.
                for (int i = 0; i < _pho.Tiles.Length; i++)
                    _pho.Tiles[i].material.color = Color.black;
            };
        }

        /// <summary>
        /// Shuffles the buttons in a random order.
        /// </summary>
        internal void ShuffleButtons()
        {
            // Shuffles the buttons, unsurprisingly.
            buttons = buttons.Shuffle();

            // Renders letters if cruel mode is off.
            for (int i = 0; i < _pho.ButtonText.Length; i++)
                _pho.ButtonText[i].text = _render.cruelMode ? string.Empty : buttons[i].ToString()[0].ToString();

            // Sets all of the buttons to be the appropriate color, based on the new shuffled array.
            for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
                _pho.ButtonRenderers[i].material.color = buttons[i].GetColor();
        }

        /// <summary>
        /// This is called whenever submission needs to be entered.
        /// </summary>
        private void EnterSubmit()
        {
            _init.isInSubmission = true;
            _init.submission = string.Empty;
            _init.buttonPresses = new ButtonType[0];

            _pho.ButtonGroupRenderer.localScale = new Vector3(0.038f, 0.01f, 0.038f);

            _pho.PlaySound(SFX.Pho.StartSubmit);
            ResetMarkers();
            ShuffleButtons();

            _pho.StartCoroutine(animate.EnterSubmit());
        }

        /// <summary>
        /// Resets placement of the markers.
        /// </summary>
        private void ResetMarkers()
        {
            foreach (var renderer in _pho.MarkerRenderers)
                renderer.transform.localScale = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// Gives feedback in-game, with controller vibration, camera shake, and a button press sound.
        /// </summary>
        /// <param name="transform">Where the sound will be located.</param>
        /// <param name="intensity">The intensity of the shake/vibration.</param>
        private void PressFeedback(Transform transform, float intensity = 1)
        {
            _pho.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            _pho.Number.AddInteractionPunch(intensity);
        }
    }
}
