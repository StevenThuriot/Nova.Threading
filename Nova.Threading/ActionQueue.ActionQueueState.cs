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

namespace Nova.Threading
{
    internal partial class ActionQueue
    {
        /// <summary>
        /// Base State Class for the ActionQueue.
        /// </summary>
        private abstract partial class ActionQueueState
        {
            private readonly ActionQueue _queue;

            /// <summary>
            /// Prevents a default instance of the <see cref="ActionQueueState" /> class from being created.
            /// </summary>
            /// <param name="queue">The queue.</param>
            private ActionQueueState(ActionQueue queue)
            {
                _queue = queue;
            }

            /// <summary>
            /// Initializes the ActionQueue's state.
            /// </summary>
            /// <param name="actionQueue">The action queue</param>
            public static void Initialize(ActionQueue actionQueue)
            {
                lock (actionQueue._lock)
                {
                    actionQueue._state = new InitialActionQueueState(actionQueue);
                }
            }

            /// <summary>
            /// Sets the ActionQueue's state as disposed.
            /// </summary>
            /// <param name="actionQueue">The action queue</param>
            public static void SetAsDisposed(ActionQueue actionQueue)
            {
                lock (actionQueue._lock)
                {
                    actionQueue._state = new DisposedActionQueueState(actionQueue);
                }
            }
            
            /// <summary>
            /// Sets the state depending on the passed action.
            /// </summary>
            /// <param name="action">The action.</param>
            internal virtual void SetStateDependingOn(IAction action)
            {
                if (action.Options.CheckFlags(ActionFlags.Terminating))
                {
                    _queue._state = new TerminatingActionQueueState(_queue);
                }
                else if (action.Options.CheckFlags(ActionFlags.Blocking))
                {
                    _queue._state = new BlockingActionQueueState(_queue);
                }
            }

            /// <summary>
            /// Checks if the action can be queued.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <returns></returns>
            internal abstract bool CanEnqueueAction(IAction action);
        }
    }
}
