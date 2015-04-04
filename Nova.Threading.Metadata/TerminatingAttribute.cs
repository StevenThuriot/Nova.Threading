using System;

namespace Nova.Threading.Metadata
{
    /// <summary>
    /// The queue will be terminated after finishing this task. This is also blocking per definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TerminatingAttribute : Attribute { }
}