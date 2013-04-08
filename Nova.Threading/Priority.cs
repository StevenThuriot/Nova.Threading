using System;

namespace Nova.Threading
{
    /// <summary>
    /// Priority in which the actions will execute.
    /// </summary>
    [Serializable]
    public enum Priority
    {
        /// <summary>
        /// The lowest
        /// </summary>
        Lowest,

        /// <summary>
        /// Below normal
        /// </summary>
        BelowNormal,

        /// <summary>
        /// Normal
        /// </summary>
        Normal,

        /// <summary>
        /// Above normal
        /// </summary>
        AboveNormal,

        /// <summary>
        /// The highest
        /// </summary>
        Highest,
    }
}