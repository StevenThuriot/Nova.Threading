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
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Threading
{
    /// <summary>
    /// An internal class for IAction that uses the TPL.
    /// </summary>
    internal class WrappedTask : IAction
    {
        private readonly Task _InitTask; //Need this to start execution
        private Task _LastContinuationTask; //Need this for continuations

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        /// <value>
        /// The session ID.
        /// </value>
        public Guid SessionID { get; private set; }
        /// <summary>
        /// Gets the queue ID.
        /// </summary>
        /// <value>
        /// The queue ID.
        /// </value>
        public Guid QueueID { get; private set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public ActionFlags Options { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrappedTask" /> class.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="queueID">The queue ID.</param>
        /// <param name="action">The action.</param>
        public WrappedTask(Guid sessionID, Guid queueID, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            SessionID = sessionID;
            QueueID = queueID;
            _InitTask = new Task(action);
            _LastContinuationTask = _InitTask;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            _InitTask.Start();
        }

        /// <summary>
        /// Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public IAction ContinueWith(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _LastContinuationTask = _LastContinuationTask.ContinueWith(_ => action(), TaskContinuationOptions.HideScheduler);
            return this;
        }

        /// <summary>
        /// Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public IAction ContinueOnMainThreadWith(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _LastContinuationTask = _LastContinuationTask.ContinueWith(_ => action(), new CancellationToken(), TaskContinuationOptions.HideScheduler, TaskScheduler.FromCurrentSynchronizationContext());
            return this;
        }
    }
}
