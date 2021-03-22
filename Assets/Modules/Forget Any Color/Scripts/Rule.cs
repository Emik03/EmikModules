namespace ForgetAnyColor
{
    /// <summary>
    /// Datatype for use in RuleSeed, containing only 1 integer.
    /// </summary>
    internal class Rule
    {
        /// <summary>
        /// This value is set from 0 through 9 or an index of edgework in Arrays.Edgework, for use in tables.
        /// </summary>
        internal int Number { get; set; }
    }
}
