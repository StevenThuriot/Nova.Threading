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
                private volatile bool _creationSucceeded;
                private volatile bool _creationalActionQueued;

                /// <summary>
                /// Initializes a new instance of the <see cref="ActionQueueState.InitialActionQueueState" /> class.
                /// </summary>
                /// <param name="queue">The queue.</param>
                public InitialActionQueueState(ActionQueue queue)
                    : base(queue)
                {
                    _creationSucceeded = false;
                    _creationalActionQueued = false;
                }

                /// <summary>
                /// Sets the state depending on the passed action.
                /// </summary>
                /// <param name="action">The action.</param>
                internal override void SetStateDependingOn(IAction action)
                {
                    if (!_creationSucceeded)
                        return; //Don't change state until the queue creation has finished and succeeded.

                    var actionQueue = _queue;
                    actionQueue._state = new RunningActionQueueState(actionQueue);
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueueAction(IAction action)
                {
                    if (_creationalActionQueued)
                        return false;

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
                    var actionQueue = _queue;
                    if (action.IsSuccesfull)
                    {
                        _creationSucceeded = true;
                        return true;
                    }

                    //Couldn't enter, complete and clean up queue :(

                    actionQueue.Complete();

                    var handler = actionQueue.CleanUpQueue;
                    if (handler != null) handler(actionQueue, new ActionQueueEventArgs(actionQueue.ID));

                    return false;
                }
            }
        }
    }
}
