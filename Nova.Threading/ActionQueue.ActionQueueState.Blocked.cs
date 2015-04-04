namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        private abstract partial class ActionQueueState
        {
            /// <summary>
            /// The ActionQueue is blocked while in this state.
            /// </summary>
            /// <remarks>
            /// This state does not allow transitions or new actions on the queue.
            /// The only way to get back to a running state, is by having the BlockingAction finish.
            /// </remarks>
            private class BlockedActionQueueState : ActionQueueState
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.BlockedActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public BlockedActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                }

                /// <summary>
                /// Sets the state depending on the passed action.
                /// </summary>
                /// <param name="action">The action.</param>
                internal override ActionQueueState Update(IAction action)
                {
                    //Don't allow state changes. Wait for the Blocking Action to reset the queue to running.
                    return this;
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueue(IAction action)
                {
                    return false;
                }
            }
        }
    }
}
