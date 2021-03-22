using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ForgetTheColors
{
    /// <summary>
    /// Initalizes Colorblind, RuleSeed, and all of the temporary lists to be the correct size.
    /// </summary>
    sealed class Init
    {
        public void Start(ref KMBossModule Boss, ref bool colorblind, ref KMColorblindMode Colorblind, ref KMRuleSeedable Rule, ref int moduleId, ref Rule[][] rules, ref int maxStage, ref KMBombInfo BombInfo, ref List<byte> cylinder, ref List<byte> nixies, ref List<byte> gear, ref List<short> largeDisplay, ref List<int> calculatedValues, ref List<int> sineNumber, ref List<string> gearColor, ref List<string> ruleColor, ref TextMesh[] NixieTexts, ref KMAudio Audio, ref KMBombModule Module)
        {
            //boss module handler
            if (Boss.GetIgnoredModules(Module, Strings.Ignore) != null)
                Strings.Ignore = Boss.GetIgnoredModules(Module, Strings.Ignore);

            //enables colorblind mode if needed
            colorblind = Colorblind.ColorblindModeActive;

            //gets seed
            MonoRandom rnd = Rule.GetRNG();
            Debug.LogFormat("[Forget The Colors #{0}]: Using version {1} with rule seed: {2}.", moduleId, Strings.Version, rnd.Seed);

            if (rnd.Seed == 1)
                rules = null;

            else
            {
                //establishes new variable
                rules = new Rule[2][];
                rules[0] = new Rule[21];
                rules[1] = new Rule[10];

                //applies rule seeding for cylinders
                for (byte i = 0; i < rules[0].Length; i++)
                    rules[0][i] = new Rule { Cylinder = (byte)rnd.Next(rules[0].Length), Operator = (byte)rnd.Next(5) };

                //applies rule seeding for edgework
                for (byte i = 0; i < rules[1].Length; i++)
                    rules[1][i] = new Rule { Edgework = (byte)rnd.Next(rules[1].Length), Operator = (byte)rnd.Next(5) };
            }

            //if on unity, max stage should equal the initial value assigned, otherwise set it to the proper value
            if (!Application.isEditor)
                maxStage = BombInfo.GetSolvableModuleNames().Where(m => !Strings.Ignore.Contains(m)).Count();

            //proper grammar!!
            if (maxStage != 1)
                Debug.LogFormat("[Forget The Colors #{0}]: Welcome to FTC - This bomb will have {1} stages.", moduleId, maxStage);

            else
                Debug.LogFormat("[Forget The Colors #{0}]: Welcome to FTC - This bomb will have a single stage.", moduleId);

            //initialization of previous stage variables
            for (ushort i = 0; i < maxStage; i++)
            {
                for (byte j = 0; j < 4; j++)
                    cylinder.Add(0);

                nixies.Add(0);
                nixies.Add(0);
                gear.Add(0);
                largeDisplay.Add(0);
                calculatedValues.Add(0);
                sineNumber.Add(0);
                gearColor.Add("Red");
                ruleColor.Add("Red");
            }

            //begin module
            NixieTexts[0].text = "0";
            NixieTexts[1].text = "0";
            Audio.PlaySoundAtTransform(Sounds.Ftc.Start, Module.transform);
        }
    }
}