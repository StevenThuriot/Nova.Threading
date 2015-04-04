using System;

namespace Nova.Threading
{
	/// <summary>
	/// The Action Interface used for queueing actions.
	/// </summary>
	public interface IAction
    {
        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        Guid ID { get; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        ActionFlags Options { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        void Execute();

        /// <summary>
        /// Specifies the Can Excute logic.
        /// This can only be set once.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        IAction CanExecute(Func<bool> action, bool mainThread = true);

        /// <summary>
        /// Exception handler for this action.
        /// </summary>
        /// <param name="action">The action.</param>
        IAction HandleException(Action<Exception> action);

        /// <summary>
        /// Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        IAction ContinueWith(Action action, bool mainThread = false);

        /// <summary>
        /// Specifies the Finishing logic.
        /// This can set multiple times.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority. A lower</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
	    IAction FinishWith(Action action, Priority priority = Priority.Normal, bool mainThread = false);

        /// <summary>
        /// Gets a value indicating whether this instance ran succesfully.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance ran succesfully; otherwise, <c>false</c>.
        /// </value>
        bool IsSuccesfull { get; }
    }
}
