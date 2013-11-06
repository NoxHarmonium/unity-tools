namespace UnityTools.Threading
{
    using System;

    /// <summary>
    /// Represents a class that enables threads to execute code on a main thread loop.
    /// This is useful for things such as UI where certain things can only be executed
    /// on a specific main thread.
    /// </summary>
    public interface IDispatcher
    {
        #region Methods

        /// <summary>
        /// Dispatches the action asynchronously on the dispatch main thread.
        /// The action will execute in the next frame update phase.
        /// </summary>
        /// <param name="action">The action to execute on the dispatch thread.</param>
        void Dispatch(Action action);

        /// <summary>
        /// Dispatches the action asynchronously on the dispatch main thread but blocks
        /// the current thread until the action is executed.
        /// The action will execute in the next frame update phase.
        /// </summary>
        /// <param name="action">The action to execute on the dispatch thread.</param>
        void DispatchWait(Action action);

        /// <summary>
        /// This method should be executed on the main dispatch thread to
        /// execute the queued dispatch actions.
        /// </summary>
        void PumpActionQueue();

        #endregion Methods
    }
}