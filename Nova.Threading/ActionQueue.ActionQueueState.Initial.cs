namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        private abstract partial class ActionQueueState
        {
            /// <summary>
            /// The initial state for an <see cref="ActionQueue" />.
            /// <remarks>
            /// The <see cref="ActionQueueState.InitialActionQueueState" /> is a special case of <see cref="ActionQueueState.BlockingActionQueueState" />. 
            /// It will allow to enqueue only one action and remain blocked until that action finishes. When it does, it will turn into the <see cref="ActionQueueState.RunningActionQueueState" />.
            /// 
            /// This also implies that a Creational action is always blocking!
            /// If that action is not succesful, the <see cref="ActionQueueState.InitialActionQueueState" /> will make sure the queue gets cleaned up.
            /// </remarks>
            /// </summary>
            private class InitialActionQueueState : ActionQueueState
            {
                private bool _creationSucceeded;
                private bool _creationalActionQueued;

                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.InitialActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public InitialActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                }

                /// <summary>
                /// Sets the state depending on the passed action.
                /// </summary>
                /// <param name="action">The action.</param>
                internal override ActionQueueState Update(IAction action)
                {
                    if (!_creationSucceeded)
                        return this; //Don't change state until the queue creation has finished and succeeded.

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

                    var runningActionQueueState = new RunningActionQueueState(_queue);
                    _queue._state = runningActionQueueState;

                    return runningActionQueueState;
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueue(IAction action)
                {
                    if (_creationalActionQueued) return false;
                    
                    _creationalActionQueued = true;

                    action.FinishWith(() => InitializeQueue(action), Priority.Highest);
                    return true;
                }

                /// <summary>
                /// Initializes the queue.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                private bool InitializeQueue(IAction action)
                {
                    if (action.IsSuccesfull)
                    {
                        _creationSucceeded = true;
                        return true;
                    }


                    //Couldn't enter, complete and clean up queue :(
                    _queue.Complete();

                    var handler = _queue.CleanUpQueue;
                    if (handler != null) handler(_queue, new ActionQueueEventArgs(_queue.ID));

                    return false;
                }
            }
        }
    }
}
