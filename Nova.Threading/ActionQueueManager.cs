﻿using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Nova.Threading
{
    /// <summary>
    /// The instance that delegates the queued action to the correct ActionQueue.
    /// </summary>
    public class ActionQueueManager : IActionQueueManager
    {

        private bool _disposed;
        private readonly object _lock;
        private readonly ActionQueueCollection _queues;


        /// <summary>
        /// Gets or sets the max degree of parallelism.
        /// </summary>
        /// <value>
        /// The max degree of parallelism.
        /// </value>
        /// <remarks>This only gets used when creating a new queue.</remarks>
        public int MaxDegreeOfParallelism { get; set; }


        /// <summary>
        /// Gets or sets the default wait timeout when completing a queue.
        /// </summary>
        /// <value>
        /// TimeSpan.FromMilliseconds(1500)
        /// </value>
        public TimeSpan DefaultWaitTimeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQueueManager" /> class.
        /// </summary>
        public ActionQueueManager()
        {
            _lock = new object();
            _queues = new ActionQueueCollection();
            MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded;
            DefaultWaitTimeout = TimeSpan.FromMilliseconds(1500);
        }

        /// <summary>
        /// Queues the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>True if the action was queued.</returns>
        /// <remarks>
        /// In case the queue does not exist yet, it will create it when queueing a <see cref="ActionFlags.Creational" /> action.
        /// </remarks>
        public bool Enqueue(IAction action)
        {
            if (_disposed) return false;

            if (action == null)
                throw new ArgumentNullException("action");

            if (action.Options.CheckFlags(ActionFlags.Unqueued))
            {
                Task.Run(() => action.Execute());
                return true;
            }

            lock (_lock)
            {
                ActionQueue queue;
                if (_queues.TryGetValue(action.ID, out queue))
                {
                    var result = queue.Enqueue(action);
                    return result;
                }
                
                //Create new Queue on Creational Actions when it doesn't exist yet.
                if (action.Options.CheckFlags(ActionFlags.Creational))
                {
                    queue = new ActionQueue(action.ID, MaxDegreeOfParallelism, DefaultWaitTimeout);
                    queue.CleanUpQueue += CleanUpQueue;

                    _queues.Add(queue);

                    var result = queue.Enqueue(action);
                    return result;
                }
            }
            
            //No queue present, discarding action.
            return false;
        }

        /// <summary>
        /// Cleans up finished queues.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ActionQueueEventArgs" /> instance containing the event data.</param>
        private void CleanUpQueue(object sender, ActionQueueEventArgs e)
        {
            lock (_lock)
            {
                var queue = sender as ActionQueue;

                if (queue == null && !_queues.TryGetValue(e.ID, out queue))
                    return; //Sender was null or not an ActionQueue and the Queue id was not found in our storage.

                queue.Dispose();
                _queues.Remove(queue);
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ActionQueueManager"/> is reclaimed by garbage collection.
        /// </summary>
        ~ActionQueueManager()
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
                _queues.Dispose();
            }

            _disposed = true;
        }
    }
}
