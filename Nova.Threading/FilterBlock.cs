using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Nova.Threading
{
    /// <summary>
    /// A Dataflow block which acts as a <see cref="BufferBlock{T}"/> with the posibility to filter messages.
    /// </summary>
    /// <typeparam name="T">The type of message.</typeparam>
    internal sealed class FilterBlock<T> : IPropagatorBlock<T, T>
    {
        private readonly BufferBlock<T> _buffer;
        private readonly Predicate<T> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterBlock{T}"/> class.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public FilterBlock(Predicate<T> predicate)
            :this (predicate, CancellationToken.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterBlock{T}" /> class.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="token">The token.</param>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public FilterBlock(Predicate<T> predicate, CancellationToken token)
            : this(predicate, new DataflowBlockOptions { CancellationToken = token })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterBlock{T}" /> class.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public FilterBlock(Predicate<T> predicate, DataflowBlockOptions options)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");
            if (options == null) throw new ArgumentNullException("options");

            _predicate = predicate;
            
            _buffer = new BufferBlock<T>(options);
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation and completion of the dataflow block.
        /// </summary>
        /// <returns>The task.</returns>
        public Task Completion
        {
            get { return _buffer.Completion; }
        }

        /// <summary>
        /// Signals to the <see cref="T:System.Threading.Tasks.Dataflow.IDataflowBlock" /> that it should not accept nor produce any more messages nor consume any more postponed messages.
        /// </summary>
        public void Complete()
        {
            _buffer.Complete();
        }

        /// <summary>
        /// Causes the <see cref="T:System.Threading.Tasks.Dataflow.IDataflowBlock" /> to complete in a <see cref="F:System.Threading.Tasks.TaskStatus.Faulted" /> state.
        /// </summary>
        /// <param name="exception">The <see cref="T:System.Exception" /> that caused the faulting.</param>
        void IDataflowBlock.Fault(Exception exception)
        {
            ((IDataflowBlock)_buffer).Fault(exception);
        }

        /// <summary>
        /// Offers the message.
        /// </summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="messageValue">The message value.</param>
        /// <param name="source">The source.</param>
        /// <param name="consumeToAccept">if set to <c>true</c> [consume to accept].</param>
        /// <returns></returns>
        DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
        {
            return _predicate(messageValue)
                ? ((ITargetBlock<T>) _buffer).OfferMessage(messageHeader, messageValue, source, consumeToAccept)
                : DataflowMessageStatus.Declined;
        }

        /// <summary>
        /// Links to.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="linkOptions">The link options.</param>
        /// <returns></returns>
        public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        {
            return _buffer.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Consumes the message.
        /// </summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="target">The target.</param>
        /// <param name="messageConsumed">if set to <c>true</c> [message consumed].</param>
        /// <returns></returns>
        public T ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
        {
            return ((ISourceBlock<T>)_buffer).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Reserves the message.
        /// </summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            return ((ISourceBlock<T>)_buffer).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Releases the reservation.
        /// </summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="target">The target.</param>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        {
            ((ISourceBlock<T>)_buffer).ReleaseReservation(messageHeader, target);
        }
    }
}
