#region License

// 
//  Copyright 2012 Steven Thuriot
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
    /// <summary>
    /// The Action Interface used for queueing actions.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets the session ID.
        /// </summary>
        /// <value>
        /// The session ID.
        /// </value>
        Guid SessionID { get; }

        /// <summary>
        /// Gets the queue ID.
        /// </summary>
        /// <value>
        /// The queue ID.
        /// </value>
        Guid QueueID { get; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        ActionFlags Options { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        void Execute();

        /// <summary>
        /// Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        IAction ContinueWith(Action action);

        /// <summary>
        /// Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        IAction ContinueOnMainThreadWith(Action action);


        /// <summary>
        /// Exception handler for this action.
        /// </summary>
        /// <param name="action">The action.</param>
        void HandleException(Action<Exception> action);
    }
}
