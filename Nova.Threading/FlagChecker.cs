using System;
using System.Collections.Generic;
using Nova.Threading.Metadata;

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
                if (type.IsDefined(typeof(CreationalAttribute))) flags |= ActionFlags.Creational;
                if (type.IsDefined(typeof(TerminatingAttribute))) flags |= ActionFlags.Terminating;
                if (type.IsDefined(typeof(BlockingAttribute))) flags |= ActionFlags.Blocking;
                if (type.IsDefined(typeof(UnqueuedAttribute))) flags |= ActionFlags.Unqueued;

                Cache.Add(type, flags);
            }

            return flags;
        }
    }
}
