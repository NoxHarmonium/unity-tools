namespace UnityTools.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using UnityEngine;

    /// <summary>
    /// A helper class inspired by the .NET 4.0 Task object and javascript flow libraries such as Q. 
    /// Uses generics.
    /// </summary>
    /// <see href="http://promises-aplus.github.io/promises-spec/"/>
    public class UnityTask<T> : UnityTask
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityTask`1"/> class.
        /// </summary>
        /// <param name="dispatchOnUnityThread">If set to <c>true</c> than dispatch callbacks on unity thread.</param>
        public UnityTask(IDispatcher dispatcher = null)
            : base(dispatcher)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityTask`1"/> class which wraps
        /// an action, automatically running it on a new thread. All uncaught exceptions 
        /// thrown in the spawned thread will automatically trigger Task.Reject().
        /// </summary>
        /// <param name="taskAction">The delegate that will be executed on a seperate thread.</param>
        /// <param name="dispatchOnUnityThread">If set to <c>true</c> than dispatch callbacks on unity thread.</param>
        public UnityTask(UnityTaskAutoThreadGenericDelegate taskAction, IDispatcher dispatcher = null)
            : base(dispatcher)
        {
            _thread = new Thread( () =>
            {
                try
                {
                    taskAction(this);
                }
                catch (Exception e)
                {
                    this.Reject(e);
                }
            });
            _thread.Start();
        }

        #endregion Constructors

        #region Delegates

        public delegate void UnityTaskAutoThreadGenericDelegate(UnityTask<T> task);

        #endregion Delegates

        #region Properties

        /// <summary>
        /// Gets the result of the task. If the task has not completed, this call will block, making the task synchronous.
        /// If the task failed, the exeception will rethrown when this is called.
        /// </summary>
        /// <value>The result of the task</value>
        public new T Result
        {
            get
            {
                return (T) base.Result;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Combines multiple tasks of the same type into one callback and runs them simultaneously. On failure the failure callback is called immediately
        /// and the result of the other tasks is discarded.
        /// </summary>
        /// <param name="tasks">A series of tasks to execute in paralell</param>
        public static UnityTask<T[]> All(params UnityTask<T>[] tasks)
        {
            // Cast the tasks down to the non generic version for code reuse
            UnityTask[] nonGenericTasks = tasks.Cast<UnityTask>().ToArray();

            // Call non generic version of All
            UnityTask combinedTask = UnityTask.All(nonGenericTasks);
            UnityTask<T[]> outputTask = new UnityTask<T[]>();

            // Funnel the non generic task into a generic one
            combinedTask.Then(
                onFulfilled: o => outputTask.Resolve((T)o),
                onFailure: ex => outputTask.Resolve(ex)
            );

            return outputTask;
        }

        /// <summary>
        /// Combines multiple tasks of the same type into one callback and runs them sequentially. On failure the failure callback is called immediately
        /// and the result of the other tasks is discarded.
        /// </summary>
        /// <param name="tasks">A series of tasks to execute in paralell</param>
        public static UnityTask<T[]> AllSequential(params Func<UnityTask<T>>[] tasks)
        {
            // Cast the tasks down to the non generic version for code reuse
            Func<UnityTask>[] nonGenericTasks = tasks.Cast<Func<UnityTask>>().ToArray();

            // Call non generic version of All
            UnityTask combinedTask = UnityTask.AllSequential(nonGenericTasks);
            UnityTask<T[]> outputTask = new UnityTask<T[]>();

            // Funnel the non generic task into a generic one
            combinedTask.Then(
                onFulfilled: o => outputTask.Resolve((T)o),
                onFailure: ex => outputTask.Resolve(ex)
            );

            return outputTask;
        }

        /// <summary>
        /// Finish the task with a result
        /// </summary>
        /// <param name="value">The result of the task.</param>
        public void Resolve(T value)
        {
            base.Resolve((object)value);
        }

        /// <summary>
        /// Add callbacks that will fire on certain task states. If the task has already completed or failed, the related callbacks will fire immediately.
        /// Returns the this task so that calls can be chained together.
        /// </summary>
        /// <param name="successCallback">Callback that fires when the task succeeds</param>
        /// <param name="failureCallback">Callback that fires when the task fails</param>
        /// <param name="progressCallback">Callback that fires when the progress of the task changes</param>
        public UnityTask<T> Then(Action<T> onFulfilled = null, Action<Exception> onFailure = null, Action<float> onProgress = null)
        {
            return (UnityTask<T>) base.Then(
                onFulfilled: o =>
                {
                    if (onFulfilled != null)
                    {
                        onFulfilled((T)o);
                    }
                },
                onFailure: ex =>
                {
                    if (onFailure != null)
                    {
                        onFailure(ex);
                    }
                },
                onProgress: p =>
                {
                    if (onProgress != null)
                    {
                        onProgress(p);
                    }
                }
            );
        }

        #endregion Methods
    }
}