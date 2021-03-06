﻿namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        private abstract partial class ActionQueueState
        {
            /// <summary>
            /// Default running state that just executes received actions.
            /// </summary>
            private class RunningActionQueueState : ActionQueueState
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.RunningActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public RunningActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                }

                /// <summary>
                /// Sets the state depending on the passed action.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override ActionQueueState Update(IAction action)
                {
                    if (action.Options.CheckFlags(ActionFlags.Terminating))
                    {
                        lock (_queue._lock)
                        {
                            var terminatingActionQueueState = new TerminatingActionQueueState(_queue);
                            _queue._state = terminatingActionQueueState;
                            
                            return terminatingActionQueueState;
                        }
                    }
                    
                    if (action.Options.CheckFlags(ActionFlags.Blocking))
                    {
                        lock (_queue._lock)
                        {
                            var blockingActionQueueState = new BlockingActionQueueState(_queue);
                            _queue._state = blockingActionQueueState;

                            return blockingActionQueueState;
                        }
                    }

                    return this;
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueue(IAction action)
                {
                    return true;
                }
            }
        }
    }
}
