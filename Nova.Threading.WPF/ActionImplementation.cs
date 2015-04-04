using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Nova.Threading.WPF
{
    /// <summary>
    ///     An internal class for IAction that uses the TPL.
    /// </summary>
    internal class ActionImplementation : IAction
    {
        private Func<bool> _canExecute;
        private bool _canExecuteRunsOnMainThread;

        private readonly Queue<NovaAction> _actions;
        private readonly List<NovaFinishAction> _finishingActions;

        private Action<Exception> _handleException;

        private bool _crashed;
        private readonly Func<bool> _successfully;

        private TaskCompletionSource<bool> _taskCompletionSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActionImplementation" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        private ActionImplementation(Guid id, Func<bool> successful)
        {
            ID = id;

            _successfully = successful;

            _actions = new Queue<NovaAction>();
            _finishingActions = new List<NovaFinishAction>();

            FinishWith(CommandManager.InvalidateRequerySuggested, Priority.Lowest, true);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActionImplementation" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="action">The function.</param>
        /// <param name="startOnMainThread">Bool indicating if the first task runs on the main thread.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        public ActionImplementation(Guid id, Action action, bool startOnMainThread, Func<bool> successful)
            : this(id, successful)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            
            var novaAction = new NovaAction(action, startOnMainThread);
            _actions.Enqueue(novaAction);
        }
        
        /// <summary>
        ///     Gets the ID.
        /// </summary>
        /// <value>
        ///     The ID.
        /// </value>
        public Guid ID { get; private set; }

        /// <summary>
        ///     Gets or sets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        public ActionFlags Options { get; set; }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        public void Execute()
        {
            try
            {
                ExecuteLogic();
            }
            catch (Exception ex)
            {
                _crashed = true;

                if (_handleException != null)
                    _handleException(ex);
                else
                    throw;
            }
            finally
            {
                if (_taskCompletionSource != null)
                    _taskCompletionSource.SetResult(IsSuccesfull);
            }
        }
        
        private void ExecuteLogic()
        {
            var application = Application.Current;

            if (application == null)
                return;

            var dispatcher = application.Dispatcher;

            var canExecute = _canExecute != null;
            if (canExecute)
            {
                canExecute = _canExecuteRunsOnMainThread
                                 ? dispatcher.Invoke(_canExecute, DispatcherPriority.Send)
                                 : _canExecute();
            }

            var finishingActions = _finishingActions.OrderByDescending(x => x.Priority);

            var novaActions = canExecute
                                ? _actions.Union(finishingActions)
                                : finishingActions;

            //Create clean queue rather than add to the nova actions queue in case we want to execute this instance several times.
            var executionQueue = new Queue<NovaAction>(novaActions);
            
            while (executionQueue.Count > 0)
            {
                var novaAction = executionQueue.Dequeue();
                
                //Group following actions that run on the same thread as the next execution so we don't have to dispatch several items in a row.
                var listOfSimilarActions = BuildListOfSimilarActions(novaAction, executionQueue);

                if (novaAction.RunsOnMainThread)
                {
                    Action dispatchedAction = () =>
                    {
                        foreach (var action in listOfSimilarActions)
                        {
                            action.Execute();
                        }
                    };

                    dispatcher.Invoke(dispatchedAction, DispatcherPriority.Send);
                }
                else
                {
                    foreach (var action in listOfSimilarActions)
                    {
                        action.Execute();
                    }
                }
            }
        }

        private static IEnumerable<NovaAction> BuildListOfSimilarActions(NovaAction novaAction, Queue<NovaAction> executionQueue)
        {
            var listOfSimilarActions = new Queue<NovaAction>();
            listOfSimilarActions.Enqueue(novaAction);

            var runsOnMainThread = novaAction.RunsOnMainThread;

            while (executionQueue.Count > 0)
            {
                var nextAction = executionQueue.Peek();
                if (nextAction.RunsOnMainThread == runsOnMainThread)
                {
                    var action = executionQueue.Dequeue();
                    listOfSimilarActions.Enqueue(action);
                }
                else
                {
                    break;
                }
            }
            return listOfSimilarActions;
        }

        /// <summary>
        ///     Specifies the Can Excute logic.
        ///     This can only be set once.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        public IAction CanExecute(Func<bool> action, bool mainThread = true)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_canExecute != null)
                throw new Exception("CanExecute can only be set once.");

            _canExecute = action;
            _canExecuteRunsOnMainThread = mainThread;

            return this;
        }

        /// <summary>
        ///     Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        public IAction ContinueWith(Action action, bool mainThread = false)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            var novaAction = new NovaAction(action, mainThread);
            _actions.Enqueue(novaAction);

            return this;
        }

        /// <summary>
        /// Specifies the Finishing logic.
        /// This can set multiple times.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="priority">The priority. A lower</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public IAction FinishWith(Action action, Priority priority, bool mainThread)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            var finishAction = new NovaFinishAction(action, priority, mainThread);
            _finishingActions.Add(finishAction);

            return this;
        }

        /// <summary>
        /// Handles the exception of the previous task.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        /// <exception cref="System.Exception">HandleException can only be set once.</exception>
        public IAction HandleException(Action<Exception> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_handleException != null)
                throw new Exception("HandleException can only be set once.");

            _handleException = action;

            return this;
        }

        /// <summary>
        /// Gets a value indicating whether this instance ran succesfully.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance ran succesfully; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccesfull
        {
            get
            {
                return !_crashed && (_successfully == null || _successfully());
            }
        }

        /// <summary>
        /// Gets the success state.
        /// </summary>
        /// <returns></returns>
        public Task<bool> GetSuccessAsync()
        {
            if (_taskCompletionSource == null)
                _taskCompletionSource = new TaskCompletionSource<bool>();
            
            return _taskCompletionSource.Task;
        }
    }
}