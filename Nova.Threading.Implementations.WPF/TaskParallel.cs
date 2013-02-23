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

namespace Nova.Threading.Implementations.WPF
{
    /// <summary>
    /// Helper class to create an instance of IAction.
    /// </summary>
    public static class TaskParallel
    {
        /// <summary>
        /// Wraps the specified action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="id">The id.</param>
        /// <param name="execution">Gets the execution action.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        /// <param name="mainThread">Indicates whether this action starts executing on the main thread.</param>
        /// <returns></returns>
        public static IAction Wrap<T>(this T action, Func<T, Guid> id, Func<T, Action> execution, Func<bool> successful = null, bool mainThread = false)
        {
            var idResult = id(action);
            var executionResult = execution(action);

            var wrappedAction = Wrap(idResult, executionResult, successful, mainThread);
            wrappedAction.Options = action.GetActionFlags();

            return wrappedAction;
        }

        /// <summary>
        /// Wraps the specified action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="id">The id.</param>
        /// <param name="execution">The execution.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        /// <param name="mainThread">Indicates whether this action starts executing on the main thread.</param>
        /// <returns></returns>
        public static IAction Wrap<T>(this T action, Func<T, Guid> id, Func<T, Func<bool>> execution, Func<bool> successful = null, bool mainThread = false)
        {
            var idResult = id(action);
            var executionResult = execution(action);

            var wrappedAction = Wrap(idResult, executionResult, successful, mainThread);
            wrappedAction.Options = action.GetActionFlags();

            return wrappedAction;
        }

        /// <summary>
        /// Wraps the specified function into an IAction.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="action">The function.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        /// <param name="mainThread">Indicates whether this action starts executing on the main thread.</param>
        /// <returns></returns>
        public static IAction Wrap(Guid id, Action action, Func<bool> successful = null, bool mainThread = false)
        {
            return new TaskParallelAction(id, action, mainThread, successful);
        }

        /// <summary>
        /// Wraps the specified function into an IAction.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="function">The function.</param>
        /// <param name="successful">Returns <c>true</c> if this action ran succesfully.</param>
        /// <param name="mainThread">Indicates whether this action starts executing on the main thread.</param>
        /// <returns></returns>
        public static IAction Wrap(Guid id, Func<bool> function, Func<bool> successful = null, bool mainThread = false)
        {
            return new TaskParallelAction(id, function, mainThread, successful);
        }

        /// <summary>
        /// Gets the success state.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">This type of action is not supported</exception>
        public static async Task<bool> GetSuccessAsync(this IAction action)
        {
            var taskParallelAction = action as TaskParallelAction;
            if (taskParallelAction == null)
                throw new NotSupportedException("This type of action is not supported. Nova.Threading.Implementations.WPF only supports TPL actions.");

            return await taskParallelAction.GetSuccessAsync();
        }
    }
}