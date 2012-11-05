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
using System.Collections.Generic;

namespace Nova.Threading
{
    /// <summary>
    /// A helper class to translate the Nova.Threading Attributes to ActionFlags.
    /// </summary>
    public static class FlagChecker
    {
        /// <summary>
        /// The cache
        /// </summary>
        private static readonly Dictionary<Type, ActionFlags> Cache = new Dictionary<Type, ActionFlags>();

        /// <summary>
        /// Determines whether the specified value is defined.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is defined; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDefined(this Type value, Type attributeType)
        {
            return Attribute.IsDefined(value, attributeType);
        }

        /// <summary>
        /// Gets the action flags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ActionFlags GetActionFlags<T>(this T value)
        {
            var type = typeof(T);

            ActionFlags flags;
            if (!Cache.TryGetValue(type, out flags))
            {
                if (type.IsDefined(typeof(EnterStepAttribute))) flags |= ActionFlags.EnterStep;
                if (type.IsDefined(typeof(LeaveStepAttribute))) flags |= ActionFlags.LeaveStep;
                if (type.IsDefined(typeof(BlockingAttribute))) flags |= ActionFlags.Blocking;

                Cache.Add(type, flags);
            }

            return flags;
        }
    }
}
