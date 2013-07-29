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
using System.Threading.Tasks.Dataflow;

namespace Nova.Threading
{
    /// <summary>
    /// The instance that takes care of actually queueing the actions. This is unique on a session and step basis.
    /// </summary>
    internal partial class ActionQueue : IDisposable
    {
        private readonly TimeSpan _defaultWaitTimeout;
        private bool _disposed;
        private readonly CancellationTokenSource _tokenSource;
        private readonly FilterBlock<IAction> _filterBlock;
        private readonly object _lock;
        private ActionQueueState _state;

        /// <summary>
        /// Occurs when this queue needs to be cleaned up.
        /// </summary>
        public event EventHandler<ActionQueueEventArgs> CleanUpQueue;
        
        /// <summary>
        /// Gets the queue ID.
        /// </summary>
        /// <value>
        /// The queue ID.
        /// </value>
        public Guid ID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQueue" /> class.
        /// </summary>
        /// <param name="id">The queue ID.</param>
        /// <param name="maxDegreeOfParallelism">The max degree of parallelism.</param>
        /// <param name="defaultWaitTimeout">The default wait timeout.</param>
        public ActionQueue(Guid id, int maxDegreeOfParallelism, TimeSpan defaultWaitTimeout)
        {
            ID = id;

            _lock = new object();
            _defaultWaitTimeout = defaultWaitTimeout;
            
            ActionQueueState.Initialize(this);

            _tokenSource = new CancellationTokenSource();
            _filterBlock = new FilterBlock<IAction>(Filter, _tokenSource.Token);

            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _tokenSource.Token,
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };

            var actionBlock = new ActionBlock<IAction>(x => x.Execute(), options);

            _filterBlock.LinkTo(actionBlock);
            _filterBlock.Completion.ContinueWith(_ => actionBlock.Complete());
        }

        private bool Filter(IAction action)
        {
            lock (_lock)
            {
                return _state.Update(action).CanEnqueue(action);
            }
        }

        /// <summary>
        /// Enqueues the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        public bool Enqueue(IAction action)
        {
            return _filterBlock.Post(action);
        }

        /// <summary>
        /// Signals to the queue that it shouldn't accept or produce any more messages and shouldn't consume any more postponed messages.
        /// </summary>
        private void Complete(bool cancelIfNeeded = true)
        {
            _filterBlock.Complete();

            if (cancelIfNeeded && !_filterBlock.Completion.Wait(_defaultWaitTimeout))
            {
                //Hard cancel in case it's needed.
                _tokenSource.Cancel();
            }
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
            if (_disposed) return;

            if (disposing)
            {
                ActionQueueState.SetAsDisposed(this);

                if (!_filterBlock.Completion.IsCompleted)
                {
                    //Leaving current step/use case. No more actions should be executed.
                    Complete();
                }

                _tokenSource.Dispose();
                CleanUpQueue = null;
            }

            _disposed = true;
        }
    }
}
