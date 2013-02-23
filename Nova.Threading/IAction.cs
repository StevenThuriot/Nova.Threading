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
        /// Gets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        Guid ID { get; }

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
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        IAction ContinueWith(Action action, bool mainThread = false);

        /// <summary>
        /// Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        IAction ContinueWith(Func<bool, bool> action, bool mainThread = false);

        /// <summary>
        /// Exception handler for this action.
        /// </summary>
        /// <param name="action">The action.</param>
        void HandleException(Action<Exception> action);

        /// <summary>
        /// Specifies the Can Excute logic.
        /// This can only be set once.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        IAction CanExecute(Func<bool> action, bool mainThread = true);

        /// <summary>
        /// Specifies the Finishing logic.
        /// This can only be set once.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
	    IAction FinishWith(Action action, bool mainThread = false);

        /// <summary>
        /// Gets a value indicating whether this instance ran succesfully.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance ran succesfully; otherwise, <c>false</c>.
        /// </value>
        bool IsSuccesfull { get; }
    }
}
