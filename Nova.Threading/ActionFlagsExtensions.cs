namespace Nova.Threading
{
    /// <summary>
    /// ActionFlags Extensions.
    /// </summary>
    internal static class ActionFlagsExtensions
    {
        /// <summary>
        /// Checks the flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool CheckFlags(this ActionFlags flags, ActionFlags value)
        {
            return (flags & value) == value;
        }
    }
}