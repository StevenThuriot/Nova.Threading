using System;

namespace Nova.Threading.Metadata
{
    /// <summary>
    /// A queue will be created if necessairy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CreationalAttribute : Attribute { }
}