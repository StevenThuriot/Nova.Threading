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
using System.Globalization;

namespace Nova.Threading
{
    /// <summary>
    /// A collection of ActionQueues, using a combination of SessionID and QueueID as a key.
    /// </summary>
    internal class ActionQueueCollection : KeyedCollection<string, ActionQueue>
    {
        /// <summary>
        /// Generates the ID.
        /// </summary>
        /// <param name="action">The queue.</param>
        /// <returns></returns>
        public static string GenerateID(IAction action)
        {
            return GenerateID(action.SessionID, action.QueueID);
        }

        /// <summary>
        /// Generates the ID.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns></returns>
        public static string GenerateID(ActionQueue queue)
        {
            return GenerateID(queue.SessionID, queue.QueueID);
        }

        /// <summary>
        /// Generates the ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="queueID">The queue ID.</param>
        /// <returns></returns>
        public static string GenerateID(Guid sessionID, Guid queueID)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", sessionID, queueID);
        }

        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(ActionQueue item)
        {
            return GenerateID(item);
        }

        /// <summary>
        /// Tries to get the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out ActionQueue item)
        {
            if (Dictionary == null)
            {
                item = null;
                return false;
            }

            return Dictionary.TryGetValue(key, out item);
        }

        /// <summary>
        /// Removes the specified queue.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="queueID">The queue ID.</param>
        /// <returns></returns>
        public bool Remove(Guid sessionID, Guid queueID)
        {
            var key = GenerateID(sessionID, queueID);
            return Remove(key);
        }

        /// <summary>
        /// Tries to get the value.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <param name="queueID">The queue ID.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool TryGetValue(Guid sessionID, Guid queueID, out ActionQueue item)
        {
            var key = GenerateID(sessionID, queueID);
            
            return TryGetValue(key, out item);
        }

    }
}