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

namespace Nova.Threading.WPF
{
    /// <summary>
    /// An action
    /// </summary>
    internal class NovaAction
    {
        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="NovaAction" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="runsOnMainThread">if set to <c>true</c> [runs on main thread].</param>
        public NovaAction(Action action, bool runsOnMainThread)
        {
            _action = action;
            if (action == null)
                throw new ArgumentNullException("action");

            RunsOnMainThread = runsOnMainThread;
        }
        
        /// <summary>
        /// Gets a value indicating whether this action runs on th main thread.
        /// </summary>
        public bool RunsOnMainThread { get; private set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            _action();
        }
    }
}
