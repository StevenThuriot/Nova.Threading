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
    /// Action Queue EventArgs, supplying the Session and Queue ID.
    /// </summary>
    internal class ActionQueueEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the queue ID.
        /// </summary>
        /// <value>
        /// The queue ID.
        /// </value>
        public Guid ID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQueueEventArgs" /> class.
        /// </summary>
        /// <param name="id">The queue ID.</param>
        public ActionQueueEventArgs(Guid id)
        {
            ID = id;
        }
    }
}