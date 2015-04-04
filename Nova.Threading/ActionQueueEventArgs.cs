using System;

namespace Nova.Threading
{
    /// <summary>
    /// Action Queue EventArgs, supplying the Session and Queue ID.
    /// </summary>
    internal class ActionQueueEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the queue ID.
        /// </summary>
        /// <value>
        /// The queue ID.
        /// </value>
        public Guid ID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionQueueEventArgs" /> class.
        /// </summary>
        /// <param name="id">The queue ID.</param>
        public ActionQueueEventArgs(Guid id)
        {
            ID = id;
        }
    }
}