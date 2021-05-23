using KeepCoding;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// Supplies an enumerator ButtonType, and ModSettings.
/// </summary>
namespace TheOctadecayotton
{
    /// <summary>
    /// The mod settings that can be adjusted by a user, usually from the ModSelector.
    /// </summary>
    public class ModSettingsJSON
    {
        /// <summary>
        /// The amount of dimensions, by default, 9.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> Dimensions")]
        public int Dimension { get; set; }

        /// <summary>
        /// The amount of rotations, by default, 3.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> Rotations")]
        public int Rotation { get; set; }

        /// <summary>
        /// Whether each sphere should preserve their color.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> ColorAssist")]
        public bool ColorAssist { get; set; }

        /// <summary>
        /// The amount of steps needed for update, by default, 1.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> InOutBounce")]
        public bool IsUsingBounce { get; set; }

        /// <summary>
        /// The amount of steps needed for update, by default, 1.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> InOutElastic")]
        public bool IsUsingElastic { get; set; }

        /// <summary>
        /// Scales the spheres individually on the X, Y, and Z axis rather than taking the max out of those 3.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> StretchToFit")]
        public bool StretchToFit { get; private set; }

        /// <summary>
        /// Contains the min/max amount of dimensions. Do keep in mind that the Axis enum needs sufficient amount of elements, as well as the array in Position.cs if you ever plan on changing this.
        /// WARNING: Changing the minimum value below 3 or maximum value above 20 requires large restructuring of the module's rules. Exactly 20 is also untested and may cause problems.
        /// </summary>
        public const int Min = 3, Max = 12;

        /// <summary>
        /// Gets the value from ModSettings.
        /// </summary>
        /// <param name="octadecayotton">The instance of the module.</param>
        /// <param name="dimension">The amount of dimensions.</param>
        /// <param name="rotation">The amount of rotations.</param>
        public static void Get(TheOctadecayottonScript octadecayotton, out int dimension, out int rotation, out bool colorAssist, out bool isUsingBounce, out bool isUsingElastic, out bool stretchToFit)
        {
            // Default values.
            dimension = 9;
            rotation = 3;
            colorAssist = false;
            isUsingBounce = false;
            isUsingElastic = false;
            stretchToFit = false;

            try
            {
                // Try loading settings.
                var settings = JsonConvert.DeserializeObject<ModSettingsJSON>(octadecayotton.ModSettings.Settings);

                // Do settings exist?
                if (settings != null)
                {
                    dimension = Mathf.Clamp(settings.Dimension, Min, Max);
                    rotation = Mathf.Clamp(settings.Rotation, 0, 255);
                    colorAssist = settings.ColorAssist;
                    isUsingBounce = settings.IsUsingBounce;
                    isUsingElastic = settings.IsUsingElastic;
                    stretchToFit = settings.StretchToFit;

                    Debug.LogFormat("[The Octadecayotton #{0}]: JSON loaded successfully, values are: (Dimensions = {1}), (Rotations = {2}), (ColorAssist: {3}), (InOutBounce: {4}), (InOutElastic: {5}), and (StretchToFit: {6}).",
                        octadecayotton.moduleId,
                        dimension,
                        rotation,
                        colorAssist,
                        isUsingBounce,
                        isUsingElastic,
                        stretchToFit);
                }

                else
                    Debug.LogFormat("[The Octadecayotton #{0}]: JSON is null, resorting to default values.", octadecayotton.moduleId);
            }
            catch (JsonReaderException e)
            {
                // In the case of catastrophic failure and devastation.
                Debug.LogFormat("[The Octadecayotton #{0}]: JSON error: \"{1}\", resorting to default values.", octadecayotton.moduleId, e.Message);
            }
        }

        public static bool LoadMission(TheOctadecayottonScript octadecayotton, ref int dimension, ref int rotation, ref bool colorAssist, ref bool isUsingBounce, ref bool isUsingElastic, ref bool stretchToFit)
        {
            string description = Application.isEditor ? "" : Game.Mission.Description;

            Regex regex = new Regex(@"\[The Octadecayotton\] (\d+,){5}\d+");

            var match = regex.Match(description);

            if (!match.Success)
                return true;

            int[] values = match.Value.Replace("[The Octadecayotton] ", "").Split(',').ToNumbers();

            if (values == null || values.Length != 6)
                return true;

            if (!values[0].IsBetween(Min, Max) || !values[1].IsBetween(0, 255))
                return true;

            if (values.Skip(2).Any(i => !i.IsBetween(0, 1)))
                return true;

            dimension = values[0];
            rotation = values[1];
            colorAssist = values[2] == 1;
            isUsingBounce = values[3] == 1;
            isUsingElastic = values[4] == 1;
            stretchToFit = values[5] == 1;

            Debug.LogFormat("[The Octadecayotton #{0}]: Mission contains data specific to this module, values are: (Dimensions = {1}), (Rotations = {2}), (ColorAssist: {3}), (InOutBounce: {4}), (InOutElastic: {5}), and (StretchToFit: {6}).",
                        octadecayotton.moduleId,
                        dimension,
                        rotation,
                        colorAssist,
                        isUsingBounce,
                        isUsingElastic,
                        stretchToFit);

            return false;
        }
    }
}
