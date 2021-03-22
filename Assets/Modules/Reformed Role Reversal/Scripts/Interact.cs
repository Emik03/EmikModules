using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the user interactions with the module, striking or solving if necessary.
/// </summary>
namespace ReformedRoleReversalModule
{
    internal class Interact
    {
        internal Interact(Init init)
        {
            this.init = init;
            coroutines = init.Coroutines;
            reversal = init.Reversal;
        }

        protected internal int? CorrectAnswer;
        protected internal int Instruction, WireSelected = new System.Random().Next(1, 10);
        protected internal readonly List<int> ButtonOrder = Enumerable.Range(0, 4).ToList().Shuffle();

        private readonly ReformedRoleReversalCoroutineHandler coroutines;
        private readonly Init init;
        private readonly ReformedRoleReversal reversal;

        private bool selectWire;

        /// <summary>
        /// Cycles through the UI depending on the button pushed.
        /// </summary>
        /// <param name="num">The index for the 4 buttons.</param>
        protected internal void PressButton(ref byte num)
        {
            // Plays button sound effect.
            reversal.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, reversal.Buttons[num].transform);
            reversal.Buttons[num].AddInteractionPunch();

            // If lights are off or the module is solved, the buttons should do nothing.
            if (!init.Ready || init.Solved)
                return;

            int length = init.Conditions.GetLength(0) * init.Conditions.GetLength(1);

            switch (ButtonOrder[num])
            {
                // Subtract 1 from the current selected wire.
                case 0:
                    reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Button(4), reversal.Buttons[num].transform);
                    WireSelected = ((WireSelected + 7) % 9) + 1;
                    selectWire = true;
                    break;

                // Read previous instruction.
                case 1:
                    reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Button(2), reversal.Buttons[num].transform);
                    if (selectWire)
                        return;
                    Instruction = (--Instruction + length) % length;
                    break;

                // Read next instruction.
                case 2:
                    reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Button(1), reversal.Buttons[num].transform);
                    if (selectWire)
                        return;
                    Instruction = ++Instruction % length;
                    break;

                // Add 1 to the current selected wire.
                case 3:
                    reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Button(3), reversal.Buttons[num].transform);
                    WireSelected = (WireSelected % 9) + 1;
                    selectWire = true;
                    break;
            }

            coroutines.UpdateScreen(instructionX: Instruction / init.Conditions.GetLength(1), instructionY: Instruction % init.Conditions.GetLength(1), wireSelected: ref WireSelected, isSelectingWire: ref selectWire);
        }

        /// <summary>
        /// Stores how long the button was held for.
        /// </summary>
        protected internal void PressScreen()
        {
            // Plays button sound effect.
            reversal.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, reversal.Screen.transform);
            reversal.Screen.AddInteractionPunch(3);

            // If lights are off or the module is solved, the buttons should do nothing.
            if (!init.Ready || init.Solved)
                return;

            reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Button(5), reversal.Screen.transform);

            // Jump to next section in manual mode.
            if (!selectWire)
            {
                int lengthShort = init.Conditions.GetLength(1), length = init.Conditions.GetLength(0) * init.Conditions.GetLength(1);
                Instruction = ((Instruction / lengthShort) + 1) * lengthShort % length;

                coroutines.UpdateScreen(instructionX: Instruction / lengthShort, instructionY: Instruction % lengthShort, wireSelected: ref WireSelected, isSelectingWire: ref selectWire);
                return;
            }

            // The answer being correct is done here.
            if (CorrectAnswer == null || CorrectAnswer == WireSelected)
            {
                Debug.LogFormat("[Reformed Role Reversal #{0}]: The correct wire was cut. Module solved!", init.ModuleId);
                reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Solve, reversal.Screen.transform);
                init.Solved = true;

                reversal.Module.HandlePass();

                coroutines.UpdateScreen(instructionX: 0, instructionY: 0, wireSelected: ref WireSelected, isSelectingWire: ref selectWire);
                return;
            }

            // The answer being incorrect is done here.
            Debug.LogFormat("[Reformed Role Reversal #{0}]: Wire {1} was cut which was incorrect. Module strike!", init.ModuleId, WireSelected);
            reversal.Audio.PlaySoundAtTransform(Sounds.Rrr.Strike, reversal.Screen.transform);
            selectWire = false;
            Instruction = 0;

            coroutines.UpdateScreen(instructionX: Instruction / init.Conditions.GetLength(1), instructionY: Instruction % init.Conditions.GetLength(1), wireSelected: ref WireSelected, isSelectingWire: ref selectWire);

            reversal.Module.HandleStrike();
        }
    }
}
