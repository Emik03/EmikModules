using KeepCoding;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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
        /// The speed of the spheres, by default, 8.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> Slowness")]
        public int Slowness { get; set; }

        /// <summary>
        /// Preserves the color of the spheres.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> ColorAssist")]
        public bool ColorAssist { get; set; }

        /// <summary>
        /// Moves the spheres using InOutBounce ease.
        /// </summary>
        [JsonProperty("TheOctadecayotton -> InOutBounce")]
        public bool IsUsingBounce { get; set; }

        /// <summary>
        /// Moves the spheres using InOutElastic ease.
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
        public static void Get(TheOctadecayottonScript octadecayotton, out int dimension, out int rotation, out int slowness, out bool colorAssist, out bool isUsingBounce, out bool isUsingElastic, out bool stretchToFit)
        {
            // Default values.
            dimension = 9;
            rotation = 3;
            slowness = 8;
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
                    slowness = settings.Slowness == 0 ? 8 : Mathf.Clamp(settings.Slowness, 1, 12);
                    colorAssist = settings.ColorAssist;
                    isUsingBounce = settings.IsUsingBounce;
                    isUsingElastic = settings.IsUsingElastic;
                    stretchToFit = settings.StretchToFit;

                    Debug.LogFormat("[The Octadecayotton #{0}]: JSON loaded successfully, values are: (Dimensions = {1}), (Rotations = {2}), (Slowness: {3}), (ColorAssist: {4}), (InOutBounce: {5}), (InOutElastic: {6}), and (StretchToFit: {7}).",
                        octadecayotton.moduleId,
                        dimension,
                        rotation,
                        slowness,
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

        public static bool LoadMission(TheOctadecayottonScript octadecayotton, ref int dimension, ref int rotation, ref int slowness, ref bool colorAssist, ref bool isUsingBounce, ref bool isUsingElastic, ref bool stretchToFit)
        {
            string description = Game.Mission.Description;

            if (description == null)
                return true;

            Regex regex = new Regex(@"\[The Octadecayotton\] (\d+,){6}\d+");

            var match = regex.Match(description);

            if (!match.Success)
                return true;

            int[] values = match.Value.Replace("[The Octadecayotton] ", "").Split(',').ToNumbers(min: 0, max: 255, minLength: 7, maxLength: 7);

            if (values == null)
                return true;

            if (!values[0].IsBetween(Min, Max) || !values[1].IsBetween(0, 255) || !values[2].IsBetween(1, 12))
                return true;

            if (values.Skip(3).Any(i => !i.IsBetween(0, 1)))
                return true;

            dimension = values[0];
            rotation = values[1];
            slowness = values[2];
            colorAssist = values[3] == 1;
            isUsingBounce = values[4] == 1;
            isUsingElastic = values[5] == 1;
            stretchToFit = values[6] == 1;

            Debug.LogFormat("[The Octadecayotton #{0}]: Mission contains data specific to this module, values are: (Dimensions = {1}), (Rotations = {2}), (Slowness = {3}) (ColorAssist: {4}), (InOutBounce: {5}), (InOutElastic: {6}), and (StretchToFit: {7}).",
                        octadecayotton.moduleId,
                        dimension,
                        rotation,
                        slowness,
                        colorAssist,
                        isUsingBounce,
                        isUsingElastic,
                        stretchToFit);

            return false;
        }
    }
}
