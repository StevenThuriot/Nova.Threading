namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        /// <summary>
        /// Base State Class for the ActionQueue.
        /// </summary>
        private abstract partial class ActionQueueState
        {
            private readonly ActionQueue _queue;

            /// <summary>
            /// Prevents a default instance of the <see cref="ActionQueueState" /> class from being created.
            /// </summary>
            /// <param name="queue">The queue.</param>
            private ActionQueueState(ActionQueue queue)
            {
                _queue = queue;
            }

            /// <summary>
            /// Initializes the ActionQueue's state.
            /// </summary>
            /// <param name="actionQueue">The action queue</param>
            public static void Initialize(ActionQueue actionQueue)
            {
                lock (actionQueue._lock)
                {
                    actionQueue._state = new InitialActionQueueState(actionQueue);
                }
            }

            /// <summary>
            /// Sets the ActionQueue's state as disposed.
            /// </summary>
            /// <param name="actionQueue">The action queue</param>
            public static void SetAsDisposed(ActionQueue actionQueue)
            {
                lock (actionQueue._lock)
                {
                    actionQueue._state = new DisposedActionQueueState(actionQueue);
                }
            }

            /// <summary>
            /// Sets the state depending on the passed action.
            /// </summary>
            /// <param name="action">The action.</param>
            internal abstract ActionQueueState Update(IAction action);

            /// <summary>
            /// Checks if the action can be queued.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <returns></returns>
            internal abstract bool CanEnqueue(IAction action);
        }
    }
}
