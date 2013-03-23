#region License

// 
//  Copyright 2013 Steven Thuriot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

#endregion

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Nova.Threading.Implementations.WPF
{
    /// <summary>
    ///     An internal class for IAction that uses the TPL.
    /// </summary>
    internal class TaskParallelAction : IAction
    {
        private readonly bool _StartOnMainThread;

        private readonly TaskScheduler _UISheduler;

        private Func<bool> _CanExecute;
        private bool _CanExecuteRunsOnMainThread;

        private Action _Finish;
        private bool _FinishRunsOnMainThread;

        private Action<Exception> _HandleException;

        private readonly Task<bool> _InitTask; //Need this to start execution.
        private Task<bool> _LastContinuationTask; //Need this for continuations.
        private readonly Func<bool> _Successfully;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskParallelAction" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="startOnMainThread">Bool indicating if the first task runs on the main thread.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        private TaskParallelAction(Guid id, bool startOnMainThread, Func<bool> successful)
        {
            ID = id;

            _StartOnMainThread = startOnMainThread;

            var dispatcher = Application.Current.Dispatcher;

            _UISheduler = dispatcher.CheckAccess()
                              ? TaskScheduler.FromCurrentSynchronizationContext()
                              : dispatcher.Invoke(() => TaskScheduler.FromCurrentSynchronizationContext(), DispatcherPriority.Send);

            _Successfully = successful;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskParallelAction" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="action">The function.</param>
        /// <param name="startOnMainThread">Bool indicating if the first task runs on the main thread.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        public TaskParallelAction(Guid id, Action action, bool startOnMainThread, Func<bool> successful)
            : this(id, startOnMainThread, successful)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            var initialAction = new Func<bool>(() =>
            {
                action();
                return true;
            });

            _InitTask = new Task<bool>(initialAction);
            _LastContinuationTask = _InitTask;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskParallelAction" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="function">The function.</param>
        /// <param name="startOnMainThread">Bool indicating if the first task runs on the main thread.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        public TaskParallelAction(Guid id, Func<bool> function, bool startOnMainThread, Func<bool> successful)
            : this(id, startOnMainThread, successful)
        {
            if (function == null)
                throw new ArgumentNullException("function");

            _InitTask = new Task<bool>(function);
            _LastContinuationTask = _InitTask;
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
            var scheduler = _StartOnMainThread ? _UISheduler : TaskScheduler.Default;
            if (_CanExecute == null)
            {
                Task task = _LastContinuationTask;

                if (_Finish != null)
                {
                    var finishSheduler = _FinishRunsOnMainThread ? _UISheduler : TaskScheduler.Default;
                    task = task.ContinueWith(_ => _Finish(), Task.Factory.CancellationToken, TaskContinuationOptions.HideScheduler, finishSheduler);
                }

                if (_HandleException != null)
                {
                    task = task.ContinueWith(Handle, TaskContinuationOptions.NotOnRanToCompletion);
                }

                _InitTask.Start(scheduler);
            }
            else
            {
                RunWithCanExecuteLogic(scheduler);
            }
        }

        /// <summary>
        /// Executes the task while keeping the CanExecute logic in mind.
        /// </summary>
        /// <param name="scheduler">The scheduler.</param>
        private void RunWithCanExecuteLogic(TaskScheduler scheduler)
        {
            var canExecuteSheduler = _CanExecuteRunsOnMainThread ? _UISheduler : TaskScheduler.Default;

            var task = Task.Factory.StartNew(_CanExecute, Task.Factory.CancellationToken, TaskCreationOptions.HideScheduler, canExecuteSheduler)
                                   .ContinueWith(x =>
                                   {
                                       if (!x.Result) return;

                                       _InitTask.Start(scheduler);
                                       _LastContinuationTask.Wait();
                                   }, TaskContinuationOptions.HideScheduler | TaskContinuationOptions.OnlyOnRanToCompletion);

            if (_Finish != null)
            {
                var finishSheduler = _FinishRunsOnMainThread ? _UISheduler : TaskScheduler.Default;
                task = task.ContinueWith(_ => _Finish(), Task.Factory.CancellationToken, TaskContinuationOptions.HideScheduler, finishSheduler);
            }

            if (_HandleException != null)
            {
                task = task.ContinueWith(Handle, TaskContinuationOptions.NotOnRanToCompletion);
            }
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

            if (_CanExecute != null)
                throw new Exception("CanExecute can only be set once.");

            _CanExecute = action;
            _CanExecuteRunsOnMainThread = mainThread;

            return this;
        }

        /// <summary>
        ///     Specifies the Finishing logic.
        ///     This can only be set once.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        public IAction FinishWith(Action action, bool mainThread = false)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_Finish != null)
                throw new Exception("Finish can only be set once.");

            _Finish = action;
            _FinishRunsOnMainThread = mainThread;

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
            get { return _Successfully == null || _Successfully(); }
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

            var func = new Func<bool, bool>(x =>
            {
                action();
                return x;
            });

            return ContinueWith(func, mainThread);
        }

        /// <summary>
        ///     Creates a continuation that executes when the target completes.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <param name="mainThread">True if the continuation executes on the main thread.</param>
        /// <returns></returns>
        public IAction ContinueWith(Func<bool, bool> action, bool mainThread = false)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            var scheduler = mainThread ? _UISheduler : TaskScheduler.Default;

            var cancellationToken = Task.Factory.CancellationToken;
            Func<Task<bool>, bool> continuationFunction = x => action(x.IsCompleted && x.Result);

            _LastContinuationTask = _LastContinuationTask.ContinueWith(continuationFunction, cancellationToken, TaskContinuationOptions.HideScheduler, scheduler);

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

            if (_HandleException != null)
                throw new Exception("HandleException can only be set once.");

            _HandleException = action;

            return this;
        }

        /// <summary>
        /// Handles the specified task's exception.
        /// </summary>
        /// <param name="task">The task.</param>
        private void Handle(Task task)
        {
            if (task.Exception != null)
            {
                _HandleException(task.Exception);
            }
        }

        /// <summary>
        /// Gets the success state.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetSuccessAsync()
        {
            var task = _LastContinuationTask.ContinueWith(x => ReturnSuccessState(x));
            _LastContinuationTask = task;

            return await task;
        }

        private bool ReturnSuccessState(Task<bool> x)
        {
            return x != null && x.IsCompleted && x.Result && IsSuccesfull;
        }
    }
}