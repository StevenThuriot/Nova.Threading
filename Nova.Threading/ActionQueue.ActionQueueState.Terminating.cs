#region License
//  
// Copyright 2013 Steven Thuriot
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
