/// <summary>
/// Stores a condition for display in the module, for keeping track whether the condition is true, and what to do if it is.
/// </summary>
namespace ReformedRoleReversalModule
{
    sealed class Condition
    {
        /// <summary>
        /// If the condition is true, the wires to append gets stored here.
        /// IMPORTANT: Each digit is treated seperately, and negative means left, positive right!
        /// </summary>
        public int[] Append { get; set; }

        /// <summary>
        /// If the condition is true, the amount of wires to discard gets stored here.
        /// IMPORTANT: This value being negative means leftmost wires, and positive rightmost!
        /// </summary>
        public int? Discard { get; set; }

        /// <summary>
        /// If the condition is true, the condition to skip to gets stored here.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// The text from the condition that will be displayed on the module's screen.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// If the condition is true, the wire to cut gets stored here.
        /// </summary>
        public int? Wire { get; set; }
    }
}