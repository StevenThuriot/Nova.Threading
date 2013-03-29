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
using System.Collections.ObjectModel;

namespace Nova.Threading
{
    /// <summary>
    /// A collection of ActionQueues, using a combination of SessionID and ID as a key.
    /// </summary>
    internal class ActionQueueCollection : KeyedCollection<Guid, ActionQueue>, IDisposable
    {
        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override Guid GetKeyForItem(ActionQueue item)
        {
            return item.ID;
        }

        /// <summary>
        /// Tries to get the value.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool TryGetValue(Guid id, out ActionQueue item)
        {
            if (Dictionary == null)
            {
                item = null;
                return false;
            }

            return Dictionary.TryGetValue(id, out item);
        }
        
        private bool _Disposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="ActionQueueCollection" /> class.
        /// </summary>
        ~ActionQueueCollection()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_Disposed) return;

            if (disposing)
            {
                foreach (var actionQueue in this)
                {
                    actionQueue.Dispose();
                }
                
                Clear();
            }

            _Disposed = true;
        }
    }
}