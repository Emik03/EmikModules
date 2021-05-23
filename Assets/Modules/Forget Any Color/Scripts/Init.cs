using System;
using System.Linq;
using UnityEngine;

namespace ForgetAnyColor
{
    /// <summary>
    /// Fires up everything else needed to start the module when this class is instantiated.
    /// </summary>
    public class Init
    {
        public Init(ForgetAnyColorCoroutineScript coroutine, FACScript FAC, ForgetAnyColorTPScript TP)
        {
            moduleId = ++moduleIdCounter;

            SFX.LogVersionNumber(FAC.Module, moduleId);

            this.coroutine = coroutine;
            this.FAC = FAC;
            this.TP = TP;

            calculate = new Calculate(FAC, this);
            render = new Render(calculate, FAC, this);
            selectable = new Selectable(calculate, coroutine, FAC, this, render);
        }

        internal Calculate calculate;
        internal ForgetAnyColorCoroutineScript coroutine;
        internal FACScript FAC;
        internal Render render;
        internal static Rule[][] rules;
        internal Selectable selectable;
        internal ForgetAnyColorTPScript TP;

        internal bool solved;
        internal static int moduleIdCounter, modulesPerStage = 1;
        internal int fakeStage, moduleId, stage, maxStage, finalStage = Arrays.EditorMaxStage, currentStage;
        internal int[,] cylinders;

        internal void Start()
        {
            // Boss module handler assignment.
            if (FAC.Boss.GetIgnoredModules(FAC.Module, Arrays.Ignore) != null)
                Arrays.Ignore = FAC.Boss.GetIgnoredModules(FAC.Module, Arrays.Ignore);

            // Set the final stage to the amount of modules.
            if (!Application.isEditor)
                finalStage = Math.Min(FAC.Info.GetSolvableModuleNames().Where(m => !Arrays.Ignore.Contains(m)).Count(), ushort.MaxValue);

            // Reset the static variable in case it got changed.
            modulesPerStage = Math.Min((int)Math.Ceiling((double)finalStage / 4), 4);

            // In the event there are no other solvable modules, this prevents a division by zero exception.
            if (modulesPerStage == 0)
                modulesPerStage = 1;

            // Add an event for each interactable element.
            for (byte i = 0; i < FAC.Selectables.Length; i++)
                FAC.Selectables[i].OnInteract += selectable.Interact(i);

            // maxStage is used by Souvenir, this grabs the latest guaranteed stage.
            maxStage = finalStage / modulesPerStage;

            // Initalize RuleSeed.
            if (rules == null)
                rules = GenerateRules(FAC.Rule.GetRNG(), ref FAC);

            // Set gear to some value, which would conflict colorblind if not set.
            FAC.GearText.text = "0";

            // Initalize Colorblind.
            render.colorblind = FAC.Colorblind.ColorblindModeActive;
            render.Colorblind(render.colorblind);

            // Initalizes the arrays.
            cylinders = new int[maxStage + 1, 3];

            // Logs initalization.
            bool singleStage = finalStage / modulesPerStage == 1;
            Debug.LogFormat("[Forget Any Color #{0}]: {1} (max {2}) stage{3} using {4}.{5}",
                moduleId,
                singleStage ? "A single" : (finalStage / modulesPerStage).ToString(),
                finalStage.ToString(),
                singleStage ? "" : "s",
                Arrays.Version,
                rules.GetLength(0) != 0 ? " Rule Seed " + FAC.Rule.GetRNG().Seed + '.' : string.Empty);

            // Automatically start a new stage.
            coroutine.StartNewStage();
        }

        private static Rule[][] GenerateRules(MonoRandom rnd, ref FACScript FAC)
        {
            FAC.Audio.PlaySoundAtTransform(SFX.Ftc.Start, FAC.Module.transform);

            if (rnd.Seed == 1)
                return new Rule[0][];

            var rules = new Rule[2][] { new Rule[24], new Rule[8] };
            int[] ranges = { 10, 30 };

            for (int i = 0; i < rules.Length; i++)
            {
                for (int j = 0; j < rules[i].Length; j++)
                {
                    rules[i][j] = new Rule
                    {
                        Number = rnd.Next(ranges[i])
                    };
                }
            }

            return rules;
        }
    }
}
