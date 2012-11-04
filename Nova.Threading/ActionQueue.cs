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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Threading
{
    /// <summary>
    /// The instance that takes care of actually queueing the actions. This is unique on a session and step basis.
    /// </summary>
    internal class ActionQueue : IDisposable
    {
        private bool _Disposed;
        private readonly object _Lock = new object();
        private readonly BlockingCollection<IAction> _BlockingCollection;
        private readonly CancellationTokenSource _TokenSource;

        private readonly List<string> _BlockingActions;

        private bool IsBlocked
        {
            get { return _BlockingActions.Count > 0; }
        }

        /// <summary>
        /// Occurs when this queue needs to be cleaned up.
        /// </summary>
        public event EventHandler<ActionQueueEventArgs> CleanUpQueue;

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
        /// Initializes a new instance of the <see cref="ActionQueue" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public ActionQueue(IAction action)
            : this(action.SessionID, action.QueueID)
        {
            Enqueue(action);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQueue" /> class.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="queueID">The queue ID.</param>
        public ActionQueue(Guid sessionID, Guid queueID)
        {
            _TokenSource = new CancellationTokenSource();
            _BlockingCollection = new BlockingCollection<IAction>();
            _BlockingActions = new List<string>();

            SessionID = sessionID;
            QueueID = queueID;

            var currentThread = Thread.CurrentThread;

            Task.Factory.StartNew(() => Listen(currentThread.CurrentCulture, currentThread.CurrentUICulture), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Enqueues the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Enqueue(IAction action)
        {
            lock (_Lock)
            {
                //Don't enqueue new actions when we have a blocking action still in the list.
                if (IsBlocked) return;

                if (action.Options.CheckFlags(ActionFlags.Blocking) || 
                    action.Options.CheckFlags(ActionFlags.LeaveStep)) //LeaveStep is blocking by its nature.
                {
                    var key = ActionQueueCollection.GenerateID(action);
                    _BlockingActions.Add(key);
                }

                _BlockingCollection.Add(action);
            }
        }

        /// <summary>
        /// Finishes the queue.
        /// </summary>
        private void FinishQueue()
        {
            lock (_Lock)
            {
                //Leaving current step/use case. No more actions should be executed.
                _BlockingCollection.CompleteAdding();
                _BlockingActions.Clear();

                //Hard cancel in case it's needed.
                if (!_BlockingCollection.IsCompleted)
                    _TokenSource.Cancel();

                var handler = CleanUpQueue;
                if (handler != null) handler(this, new ActionQueueEventArgs(SessionID, QueueID));
            }
        }

        /// <summary>
        /// Removes the blocking action.
        /// </summary>
        /// <param name="key">The key.</param>
        private void RemoveBlockingAction(string key)
        {
            lock (_Lock)
            {
                _BlockingActions.Remove(key);
            }
        }

        /// <summary>
        /// Listens for newly queued actions and executes them.
        /// </summary>
        /// <param name="currentCulture">The current culture.</param>
        /// <param name="currentUICulture">The current UI culture.</param>
        private void Listen(CultureInfo currentCulture, CultureInfo currentUICulture)
        {
            var currentThread = Thread.CurrentThread;
            
            currentThread.CurrentCulture = currentCulture;
            currentThread.CurrentUICulture = currentUICulture;

            try
            {
                foreach (var action in _BlockingCollection.GetConsumingEnumerable(_TokenSource.Token))
                {
                    Execute(action);
                }
            }
            catch (OperationCanceledException)
            {
                //Cleaning up or Disposing Action Queue.
            }
        }

        /// <summary>
        /// Executes the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        private void Execute(IAction action)
        {
            if (action.Options.CheckFlags(ActionFlags.LeaveStep))
            {
                action.ContinueWith(FinishQueue);
            }
            else if (action.Options.CheckFlags(ActionFlags.Blocking))
            {
                var key = ActionQueueCollection.GenerateID(action);
                action.ContinueWith(() => RemoveBlockingAction(key));
            }

            action.Execute();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ActionQueue"/> is reclaimed by garbage collection.
        /// </summary>
        ~ActionQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (_Disposed) return;

            if (disposing)
            {
                CleanUpQueue = null;

                if (!_BlockingCollection.IsAddingCompleted)
                {
                    //Leaving current step/use case. No more actions should be executed.
                    _BlockingCollection.CompleteAdding();

                    //Hard cancel in case it's needed.
                    if (!_BlockingCollection.IsCompleted)
                        _TokenSource.Cancel();
                }

                _BlockingActions.Clear();
                _BlockingCollection.Dispose();
                _TokenSource.Dispose();
            }

            _Disposed = true;
        }
    }
}
