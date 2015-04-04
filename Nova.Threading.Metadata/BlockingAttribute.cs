using System;

namespace Nova.Threading.Metadata
{
    /// <summary>
    /// A blocking action. New actions won't be queued until this action has finished executing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BlockingAttribute : Attribute { }
}