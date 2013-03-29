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
using System.Threading.Tasks;

namespace Nova.Threading
{/// <summary>
    /// The instance that delegates the queued action to the correct ActionQueue.
    /// </summary>
    public class ActionQueueManager : IActionQueueManager
    {
        private bool _Disposed;
        private readonly object _Lock;
        private readonly ActionQueueCollection _Queues;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQueueManager" /> class.
        /// </summary>
        public ActionQueueManager()
        {
            _Lock = new object();
            _Queues = new ActionQueueCollection();
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
            if (_Disposed) return false;

            if (action == null)
                throw new ArgumentNullException("action");

            if (action.Options.CheckFlags(ActionFlags.Unqueued))
            {
                Task.Run(() => action.Execute());
                return true;
            }

            lock (_Lock)
            {
                ActionQueue queue;
                if (_Queues.TryGetValue(action.ID, out queue))
                {
                    var result = queue.Enqueue(action);
                    return result;
                }
                
                //Create new Queue on Creational Actions when it doesn't exist yet.
                if (action.Options.CheckFlags(ActionFlags.Creational))
                {
                    queue = new ActionQueue(action.ID);
                    queue.CleanUpQueue += CleanUpQueue;

                    _Queues.Add(queue);

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
            lock (_Lock)
            {
                var queue = sender as ActionQueue;

                if (queue == null && !_Queues.TryGetValue(e.ID, out queue))
                    return; //Sender was null or not an ActionQueue and the Queue id was not found in our storage.

                queue.Dispose();
                _Queues.Remove(queue);
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
            if (_Disposed) return;

            if (disposing)
            {
                _Queues.Dispose();
            }

            _Disposed = true;
        }
    }
}
