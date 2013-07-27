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
                internal override void SetStateDependingOn(IAction action)
                {
                    //State transition is no longer allowed.
                    throw NewObjectDisposedException();
                }

                /// <summary>
                /// Checks if the action can be queued.
                /// </summary>
                /// <param name="action">The action.</param>
                /// <returns></returns>
                internal override bool CanEnqueueAction(IAction action)
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
