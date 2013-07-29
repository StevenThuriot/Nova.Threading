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
                internal override ActionQueueState Update(IAction action)
                {
                    //If a new action gets queued, set the state to blocked. 
                    //If not, we don't need to change the state 
                    //  as it will polute memory without a reason.
                    var blockedActionQueueState = new BlockedActionQueueState(_queue);
                    _queue._state = blockedActionQueueState;

                    return blockedActionQueueState;
                }

                internal override bool CanEnqueue(IAction action)
                {
                    //Reset state to running after completion. Blocked will be skipped if no action is queued before this one finishes.
                    action.FinishWith(ResetQueue, Priority.Highest);
                    return true;
                }

                /// <summary>
                /// Resets the queue.
                /// </summary>
                private void ResetQueue()
                {
                    lock (_queue._lock)
                    {
                        _queue._state = new RunningActionQueueState(_queue);
                    }
                }
            }
        }
    }
}
