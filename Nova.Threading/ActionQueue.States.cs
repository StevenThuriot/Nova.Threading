#region License

// 
//  Copyright 2013 Steven Thuriot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

#endregion

using System;

namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        #region *** Action Queue State ***

        /// <summary>
        /// Base State Class for the ActionQueue.
        /// </summary>
        private abstract class ActionQueueState
        {
            private readonly ActionQueue _Queue;

            /// <summary>
            /// Prevents a default instance of the <see cref="ActionQueueState" /> class from being created.
            /// </summary>
            /// <param name="queue">The queue.</param>
            private ActionQueueState(ActionQueue queue)
            {
                _Queue = queue;
            }

            /// <summary>
            /// Initializes the ActionQueue's state.
            /// </summary>
            /// <param name="actionQueue">The action queue</param>
            public static void Initialize(ActionQueue actionQueue)
            {
                lock (actionQueue._Mutex)
                {
                    actionQueue._State = new RunningActionQueueState(actionQueue);
                }
            }

            /// <summary>
            /// Sets the ActionQueue's state as disposed.
            /// </summary>
            /// <param name="actionQueue">The action queue</param>
            public static void SetAsDisposed(ActionQueue actionQueue)
            {
                lock (actionQueue._Mutex)
                {
                    actionQueue._State = new DisposedActionQueueState(actionQueue);
                }
            }

            /// <summary>
            /// Enqueues the action and corrects the state, if needed.
            /// </summary>
            /// <param name="action">The action to be queued.</param>
            /// <returns>True if the action was queued.</returns>
            public bool Enqueue(IAction action)
            {
                lock (_Queue._Mutex)
                {
                    SetStateDependingOn(action);
                    return _Queue._State.EnqueueAction(action);
                }
            }

            /// <summary>
            /// Sets the state depending on the passed action.
            /// </summary>
            /// <param name="action">The action.</param>
            protected virtual void SetStateDependingOn(IAction action)
            {
                if (action.Options.CheckFlags(ActionFlags.Terminating))
                {
                    _Queue._State = new TerminatingActionQueueState(_Queue);
                }
                else if (action.Options.CheckFlags(ActionFlags.Blocking))
                {
                    _Queue._State = new BlockingActionQueueState(_Queue);
                }
            }

            /// <summary>
            /// Enqueues the action.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <returns></returns>
            protected abstract bool EnqueueAction(IAction action);

            #region *** Concrete States ***

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
                /// Enqueues the action.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                protected override bool EnqueueAction(IAction action)
                {
                    return _Queue._ActionBlock.Post(action);
                }
            }

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
                protected override void SetStateDependingOn(IAction action)
                {
                    //State transition is no longer allowed.
                    throw NewObjectDisposedException();
                }

                /// <summary>
                /// Enqueues the action.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                protected override bool EnqueueAction(IAction action)
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

            /// <summary>
            /// ActionQueue's state when receiving a Blocking Action.
            /// </summary>
            /// <remarks>
            /// This state will revert into a BlockedActionQueueState automatically if an action is queued before the blocking action finishes.
            /// If no action gets queued, the state will go from Blocking to Running directly, so the memory doesn't get poluted with an unused Blocked State.
            /// If an action does get queued, the Blocked State will not accept any new actions until the Blocked Action resets the queue to Running.
            /// </remarks>
            private class BlockingActionQueueState : ActionQueueState
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.BlockingActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public BlockingActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                }

                /// <summary>
                /// Sets the state depending on the passed action.
                /// </summary>
                /// <param name="action">The action.</param>
                protected override void SetStateDependingOn(IAction action)
                {
                    //If a new action gets queued, set the state to blocked. 
                    //If not, we don't need to change the state 
                    //  as it will polute memory without a reason.

                    _Queue._State = new BlockedActionQueueState(_Queue);
                }

                protected override bool EnqueueAction(IAction action)
                {
                    //Reset state to running after completion. Blocked will be skipped if no action is queued before this one finishes.
                    action.ContinueWith(ResetQueue);

                    return _Queue._ActionBlock.Post(action);
                }

                /// <summary>
                /// Resets the queue.
                /// </summary>
                private void ResetQueue()
                {
                    lock (_Queue._Mutex)
                    {
                        _Queue._State = new RunningActionQueueState(_Queue);
                    }
                }
            }

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
                protected override void SetStateDependingOn(IAction action)
                {
                    //Don't allow state changes. Wait for the Blocking Action to reset the queue to running.
                }

                /// <summary>
                /// Enqueues the action.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                protected override bool EnqueueAction(IAction action)
                {
                    return false;
                }
            }

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

                protected override void SetStateDependingOn(IAction action)
                {
                    //A terminating ActionQueue State can only become terminated.
                    _Queue._State = new TerminatedActionQueueState(_Queue);
                }

                /// <summary>
                /// Enqueues the action.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                protected override bool EnqueueAction(IAction action)
                {
                    action.ContinueWith(FinalizeQueue);
                    return _Queue._ActionBlock.Post(action);
                }

                /// <summary>
                /// Signals to the dataflow block that it shouldn't accept or produce any more messages and shouldn't consume any more postponed messages.
                /// </summary>
                private void FinalizeQueue()
                {
                    _Queue.Complete();

                    var handler = _Queue.CleanUpQueue;
                    if (handler != null) handler(_Queue, new ActionQueueEventArgs(_Queue.ID));
                }
            }

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
                protected override void SetStateDependingOn(IAction action)
                {
                    //State transition is no longer allowed.
                }

                /// <summary>
                /// Enqueues the action.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                protected override bool EnqueueAction(IAction action)
                {
                    return false;
                }
            }

            #endregion *** Inheriting States. ***
        }

        #endregion *** Action Queue State ***
    }
}
