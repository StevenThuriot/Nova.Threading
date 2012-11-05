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
using System.Linq;

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

        /// <summary>
        /// Checks the flags using an OR operator.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static bool CheckFlags(this ActionFlags flags, params ActionFlags[] values)
        {
            return values.Any(x => CheckFlags(flags, x));
        }
    }
}