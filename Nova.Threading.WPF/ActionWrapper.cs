using System;
using System.Threading.Tasks;

namespace Nova.Threading.WPF
{
    /// <summary>
    /// Helper class to create an instance of IAction.
    /// </summary>
    public static class ActionWrapper
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
        public static IAction Wrap<T>(this T action, Func<T, Guid> id, Func<T, Action> execution, Func<T, Func<bool>> successful = null, bool mainThread = false)
        {
            var idResult = id(action);
            var executionResult = execution(action);

            Func<bool> successfulResult = null;
            if (successful != null)
            {
                successfulResult = successful(action);
            }

            var wrappedAction = Wrap(idResult, executionResult, successfulResult, mainThread);
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
            return new ActionImplementation(id, action, mainThread, successful);
        }

        /// <summary>
        /// Gets the success state.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">This type of action is not supported</exception>
        public static Task<bool> GetSuccessAsync(this IAction action)
        {
            var novaAction = action as ActionImplementation;
            if (novaAction == null)
                throw new NotSupportedException("This type of action is not supported. Nova.Threading.WPF only supports TPL actions.");

            return novaAction.GetSuccessAsync();
        }
    }
}