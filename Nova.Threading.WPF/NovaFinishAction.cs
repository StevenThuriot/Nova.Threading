using System;

namespace Nova.Threading.WPF
{
    /// <summary>
    /// A Finishing Action
    /// </summary>
    internal class NovaFinishAction : NovaAction
    {
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public Priority Priority { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NovaFinishAction" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="runsOnMainThread">if set to <c>true</c> [runs on main thread].</param>
        public NovaFinishAction(Action action, Priority priority, bool runsOnMainThread)
            : base(action, runsOnMainThread)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            Priority = priority;
        }
    }
}
