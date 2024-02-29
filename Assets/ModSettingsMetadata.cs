using System.Collections.Generic;
using UnityEngine;

internal class ModSettingsMetadata : MonoBehaviour
{
    internal static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "EmikModules-settings.txt" },
            { "Name", "Emik's Modules" },
            { "Listings", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "Text", "The Octadecayotton Settings" }, { "Type", "Section" } },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> Dimensions" },
                        { "Text", "Dimensions (Set)" },
                        { "Description", "Dimension count from 3-27. May cause lag. (Default: 9)" }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> Rotations" },
                        { "Text", "Rotations (Spin)" },
                        { "Description", "Rotation count from 0-255. (Default: 3)" }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> Slowness" },
                        { "Text", "Slowness (Slowness)" },
                        { "Description", "Slowness rotations from 1-12. (Default: 8)" }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> ColorAssist" },
                        { "Text", "Color Assist (Stay)" },
                        { "Description", "Spheres stay the same color while moving, making reads much easier." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> InOutBounce" },
                        { "Text", "Bouncy Easing (Springiness)" },
                        { "Description", "Makes rotations use a silly easing not suitable for serious play." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> InOutElastic" },
                        { "Text", "Exaggerated Easing (Supple)" },
                        { "Description", "Makes rotations use a silly easing not suitable for serious play." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> StretchToFit" },
                        { "Text", "Stretch To Fit (Stretch)" },
                        { "Description", "Stretches the tesseract to take up the whole module. Proportions may be skewed." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "TheOctadecayotton -> ExperimentalRendering" },
                        { "Text", "Experimental Rendering" },
                        { "Description", "May improve lag on high-end devices. Requires a restart to take effect." }
                    },

                    new Dictionary<string, object> { { "Text", "hexOS Settings" }, { "Type", "Section" } },
                    new Dictionary<string, object>
                    {
                        { "Key", "hexOS -> DisableOctOS" },
                        { "Text", "Disable octOS" },
                        { "Description", "Disables the ability to go into hard mode by submitting a number without playing the sequence." },
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> ForceOctOS" },
                        { "Text", "Force octOS" },
                        { "Description", "Forces the module to start out in octOS." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> FastStrike" },
                        { "Text", "Faster octOS Strikes" },
                        { "Description", "Exclusive to OctOS, shortens the strike animation to be 15 seconds instead of 57.5 seconds." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> ExperimentalShake" },
                        { "Text", "Experimental Shake" },
                        { "Description", "Have the module shake upon every played beat. Don't use this on serious bombs. :)" }
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> ForceAltSolve" },
                        { "Text", "Use Alternate Solve Animation" },
                        { "Description", "Force the module to play the alternative solve sequence." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> FlashOtherColors" },
                        { "Text", "Extraneous Flashes" },
                        { "Description", "The amount decoy colors flash. Cannot exceed 6. (Default: 5)" }
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> DelayPerBeat" },
                        { "Text", "Beat Delay" },
                        { "Description", "Alter the delay between each beat played. Cannot exceed 1. (Default: 0.07)" }
                    },
                    new Dictionary<string, object> {
                        { "Key", "hexOS -> CustomSolveQuote" },
                        { "Text", "Solve Quote" },
                        { "Description", "The quote displayed on solve. (Blank = default)" }
                    },

                    new Dictionary<string, object> { { "Text", "Phosphorescence Settings" }, { "Type", "Section" } },
                    new Dictionary<string, object> {
                        { "Key", "Phosphorescence -> CruelMode" },
                        { "Text", "Cruel Mode" },
                        { "Description", "Forces the user to recognise the colors of the buttons without being told from the label." }
                    },
                    new Dictionary<string, object> {
                        { "Key", "Phosphorescence -> StreamDelay" },
                        { "Text", "Stream Delay" },
                        { "Description", "For TP: stream delay in seconds. (1 = +15 seconds on timer)" }
                    }
                }
            }
        }
    };
}