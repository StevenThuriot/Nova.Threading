using System;

namespace Nova.Threading
{
    /// <summary>
    /// Flags to define special behaviour to an action.
    /// </summary>
    [Flags]
    public enum ActionFlags
    {
        /// <summary>
        /// No special action defined.
        /// </summary>
        None = 1 << 0,

        /// <summary>
        /// A queue will be created if necessairy.
        /// </summary>
        Creational = 1 << 1,

        /// <summary>
        /// The queue will be terminated after finishing this task. This is also blocking per definition.
        /// </summary>
        Terminating = 1 << 2 | Blocking,

        /// <summary>
        /// A blocking action. New actions won't be queued until this action has finished executing.
        /// </summary>
        Blocking = 1 << 3,

        /// <summary>
        /// This action will run unqueued. (Fire and forget)
        /// </summary>
        Unqueued = 1 << 4
    }
}