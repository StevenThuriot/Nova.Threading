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

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskParallelAction" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="startOnMainThread">Bool indicating if the first task runs on the main thread.</param>
        private TaskParallelAction(Guid id, bool startOnMainThread)
        {
            ID = id;

            _StartOnMainThread = startOnMainThread;

            var dispatcher = Application.Current.Dispatcher;

            _UISheduler = dispatcher.CheckAccess()
                              ? TaskScheduler.FromCurrentSynchronizationContext()
                              : dispatcher.Invoke(() => TaskScheduler.FromCurrentSynchronizationContext(), DispatcherPriority.Send);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskParallelAction" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="action">The function.</param>
        /// <param name="startOnMainThread">Bool indicating if the first task runs on the main thread.</param>
        public TaskParallelAction(Guid id, Action action, bool startOnMainThread)
            : this(id, startOnMainThread)
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
        public TaskParallelAction(Guid id, Func<bool> function, bool startOnMainThread)
            : this(id, startOnMainThread)
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
            if (_HandleException != null)
            {
                _LastContinuationTask = _LastContinuationTask.ContinueWith(x =>
                    {
                        if (TaskHasException(x))
                        {
                            _HandleException(x.Exception);
                            return true;
                        }

                        return false;
                    });
            }

            ExecuteTask();
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
                return true;
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

            _LastContinuationTask =
                _LastContinuationTask.ContinueWith(x => action(x.IsCompleted && !x.IsFaulted && !x.IsCanceled && x.Result),
                                                        Task.Factory.CancellationToken, TaskContinuationOptions.HideScheduler, scheduler);

            return this;
        }

        /// <summary>
        ///     Handles the exception of the previous task.
        /// </summary>
        /// <param name="action">The function.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public void HandleException(Action<Exception> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_HandleException != null)
                throw new Exception("HandleException can only be set once.");

            _HandleException = action;
        }

        /// <summary>
        ///     Executes the task.
        /// </summary>
        private void ExecuteTask()
        {
            var scheduler = _StartOnMainThread ? _UISheduler : TaskScheduler.Default;
            if (_CanExecute == null)
            {
                if (_Finish != null)
                {
                    ContinueWith(x => { _Finish(); return true; }, _FinishRunsOnMainThread);
                }

                _InitTask.Start(scheduler);
            }
            else
            {
                RunWithCanExecuteLogic(scheduler);
            }
        }

        /// <summary>
        ///     Executes the task while keeping the CanExecute logic in mind.
        /// </summary>
        /// <param name="scheduler">The scheduler.</param>
        private void RunWithCanExecuteLogic(TaskScheduler scheduler)
        {
            var canExecuteSheduler = _CanExecuteRunsOnMainThread ? _UISheduler : TaskScheduler.Default;

            var task = Task.Factory
                           .StartNew(_CanExecute, Task.Factory.CancellationToken, TaskCreationOptions.HideScheduler, canExecuteSheduler)
                           .ContinueWith(x =>
                                {
                                    if (!x.IsCompleted || x.IsFaulted || x.IsCanceled || !x.Result) return;

                                    _InitTask.Start(scheduler);
                                    _LastContinuationTask.Wait();
                                }, TaskContinuationOptions.HideScheduler);

            if (_Finish != null)
            {
                var finishSheduler = _FinishRunsOnMainThread ? _UISheduler : TaskScheduler.Default;
                task = task.ContinueWith(_ => _Finish(), Task.Factory.CancellationToken, TaskContinuationOptions.HideScheduler, finishSheduler);
            }

            if (_HandleException != null)
            {
                task.ContinueWith(x => { if (TaskHasException(x)) _HandleException(x.Exception); });
            }
        }

        /// <summary>
        /// Checks if an exception occurred in the passed task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>True if an exception occurred.</returns>
        private static bool TaskHasException(Task task)
        {
            return task.IsFaulted || task.Exception == null;
        }
    }
}