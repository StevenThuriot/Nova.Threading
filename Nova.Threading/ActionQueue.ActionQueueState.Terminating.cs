namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        private abstract partial class ActionQueueState
        {
            /// <summary>
            /// State the ActionQueue is in when receiving a terminating action.
            /// </summary>
            /// <remarks>
            /// This state will finalize the queue after terminating the action and revert into a TerminatedActionQueueState automatically if a new action gets queued, rejecting that action.
            /// </remarks>
            private class TerminatingActionQueueState : ActionQueueState
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.TerminatingActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public TerminatingActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                }

                internal override ActionQueueState Update(IAction action)
                {
                    //A terminating ActionQueue State can only become terminated.
                    var terminatedActionQueueState = new TerminatedActionQueueState(_queue);
                    _queue._state = terminatedActionQueueState;

                    return terminatedActionQueueState;
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueue(IAction action)
                {
                    action.FinishWith(() => FinalizeQueue(action), Priority.Highest);
                    return true;
                }

                /// <summary>
                /// Signals to the dataflow block that it shouldn't accept or produce any more messages and shouldn't consume any more postponed messages.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                private bool FinalizeQueue(IAction action)
                {
                    var actionQueue = _queue;

                    if (action.IsSuccesfull)
                    {
                        actionQueue.Complete();

                        var handler = actionQueue.CleanUpQueue;
                        if (handler != null) handler(actionQueue, new ActionQueueEventArgs(actionQueue.ID));

                        return true;
                    }

                    //Execution failed, reset queue to running.
                    lock (actionQueue._lock)
                    {
                        actionQueue._state = new RunningActionQueueState(actionQueue);
                    }

                    return false;
                }
            }
        }
    }
}
