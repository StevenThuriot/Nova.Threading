namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        private abstract partial class ActionQueueState
        {
            /// <summary>
            /// The Queue is terminated and waiting for disposal. It will not accept any new actions.
            /// </summary>
            private class TerminatedActionQueueState : ActionQueueState
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.TerminatedActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public TerminatedActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                }

                /// <summary>
                /// Sets the state depending on the passed action.
                /// </summary>
                /// <param name="action">The action.</param>
                internal override ActionQueueState Update(IAction action)
                {
                    //State transition is no longer allowed.
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
