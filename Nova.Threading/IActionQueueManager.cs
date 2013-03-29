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
    public interface IActionQueueManager : IDisposable
    {
        /// <summary>
        /// Queues the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>True if the action was queued.</returns>
        /// <remarks>
        /// In case the queue does not exist yet, it will create it when queueing a <see cref="ActionFlags.Creational" /> action.
        /// </remarks>
        bool Enqueue(IAction action);
    }
}