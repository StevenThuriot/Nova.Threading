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
    /// ActionFlags Extensions.
    /// </summary>
    internal static class ActionFlagsExtensions
    {
        /// <summary>
        /// Checks the flags.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool CheckFlags(this ActionFlags flags, ActionFlags value)
        {
            return (flags & value) == value;
        }
    }

    /// <summary>
    /// Flags to define special behaviour to an action.
    /// </summary>
    [Flags]
    public enum ActionFlags
    {
        /// <summary>
        /// No special action defined.
        /// </summary>
        None = 1 << 0,

        /// <summary>
        /// A queue will be created if necessairy.
        /// </summary>
        EnterStep = 1 << 1,

        /// <summary>
        /// The queue will be terminated after finishing this task. This is also blocking per definition.
        /// </summary>
        LeaveStep = 1 << 2,

        /// <summary>
        /// A blocking action. New actions won't be queued until this action has finished executing.
        /// </summary>
        Blocking = 1 << 3
    }
}