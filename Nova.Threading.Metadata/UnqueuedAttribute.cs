using System;

namespace Nova.Threading.Metadata
{
    /// <summary>
    /// This action will run unqueued. (Fire and forget)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UnqueuedAttribute : Attribute { }
}
