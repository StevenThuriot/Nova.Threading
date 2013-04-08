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

namespace Nova.Threading.Implementations.WPF
{
    /// <summary>
    /// A Finishing Action
    /// </summary>
    internal class FinishAction
    {
        /// <summary>
        /// Gets or sets a value indicating whether [runs on main thread].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [runs on main thread]; otherwise, <c>false</c>.
        /// </value>
        public bool RunsOnMainThread { get; private set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public Action Action { get; private set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public Priority Priority { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinishAction" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="runsOnMainThread">if set to <c>true</c> [runs on main thread].</param>
        public FinishAction(Action action, Priority priority, bool runsOnMainThread)
        {
            RunsOnMainThread = runsOnMainThread;
            Action = action;
            Priority = priority;
        }
    }
}
