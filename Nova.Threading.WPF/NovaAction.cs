using System;

namespace Nova.Threading.WPF
{
    /// <summary>
    /// An action
    /// </summary>
    internal class NovaAction
    {
        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="NovaAction" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="runsOnMainThread">if set to <c>true</c> [runs on main thread].</param>
        public NovaAction(Action action, bool runsOnMainThread)
        {
            _action = action;
            if (action == null)
                throw new ArgumentNullException("action");

            RunsOnMainThread = runsOnMainThread;
        }
        
        /// <summary>
        /// Gets a value indicating whether this action runs on th main thread.
        /// </summary>
        public bool RunsOnMainThread { get; private set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            _action();
        }
    }
}
