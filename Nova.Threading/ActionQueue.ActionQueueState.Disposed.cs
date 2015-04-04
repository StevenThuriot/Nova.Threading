using System;

namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        private abstract partial class ActionQueueState
        {
            /// <summary>
            /// ActionQueue's state when disposed.
            /// </summary>
            private class DisposedActionQueueState : ActionQueueState
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.DisposedActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public DisposedActionQueueState(ActionQueue queue)
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
                    throw NewObjectDisposedException();
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueue(IAction action)
                {
                    throw NewObjectDisposedException();
                }

                /// <summary>
                /// Creates a new object disposed exception.
                /// </summary>
                /// <returns></returns>
                private static ObjectDisposedException NewObjectDisposedException()
                {
                    return new ObjectDisposedException("This queue has already been disposed.");
                }
            }
        }
    }
}
