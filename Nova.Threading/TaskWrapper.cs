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
    /// Helper class to create an instance of IAction.
    /// </summary>
    public static class TaskWrapper
    {
        /// <summary>
        /// Wraps the specified function into an IAction.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <param name="id">The ID.</param>
        /// <param name="mainThread">Indicates whether this action starts executing on the main thread.</param>
        /// <returns></returns>
        public static IAction Wrap(this Action action, Guid id, bool mainThread = false)
        {
            return new WrappedTask(id, action, mainThread);
        }

        /// <summary>
        /// Wraps the specified function into an IAction.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="id">The ID.</param>
        /// <param name="mainThread">Indicates whether this action starts executing on the main thread.</param>
        /// <returns></returns>
        public static IAction Wrap(this Func<bool> function, Guid id, bool mainThread = false)
        {
            return new WrappedTask(id, function, mainThread);
        }
    }
}
