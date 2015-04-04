using System;

namespace Nova.Threading
{
    /// <summary>
    /// Interface for the instance that delegates the queued action to the correct ActionQueue.
    /// </summary>
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