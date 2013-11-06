namespace UnityTools.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using UnityEngine;

    /// <summary>
    /// A helper class inspired by the .NET 4.0 Task object and javascript flow libraries such as Q 
    /// </summary>
    /// <see href="http://promises-aplus.github.io/promises-spec/"/>
    public class UnityTask
    {
        #region Fields

        protected IDispatcher _dispatcher;
        protected List<Action> _endCallbacks = new List<Action>();
        protected Exception _exception = null;
        protected List<Action<Exception>> _failureCallbacks = new List<Action<Exception>>();
        protected bool _finished;
        protected List<Action<float>> _progressCallbacks = new List<Action<float>>();
        protected object _result;
        protected bool _succeeded;
        protected List<Action<object>> _successCallbacks = new List<Action<object>>();
        protected Thread _thread;
        protected EventWaitHandle _waitHandle;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityTask`1"/> class.
        /// </summary>
        /// <param name="dispatchOnUnityThread">If set to <c>true</c> than dispatch callbacks on unity thread.</param>
        public UnityTask(IDispatcher dispatcher = null)
        {
            _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityTask`1"/> class which wraps
        /// an action, automatically running it on a new thread. All uncaught exceptions 
        /// thrown in the spawned thread will automatically trigger Task.Reject().
        /// </summary>
        /// <param name="taskAction">The delegate that will be executed on a seperate thread.</param>
        /// <param name="dispatchOnUnityThread">If set to <c>true</c> than dispatch callbacks on unity thread.</param>
        public UnityTask(UnityTaskAutoThreadDelegate taskAction, IDispatcher dispatcher = null)
        {
            _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            _dispatcher = dispatcher;

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

        public delegate void UnityTaskAutoThreadDelegate(UnityTask task);

        #endregion Delegates

        #region Properties

        /// <summary>
        /// Gets the result of the task. If the task has not completed, this call will block, making the task synchronous.
        /// If the task failed, the exeception will rethrown when this is called.
        /// </summary>
        /// <value>The result of the task</value>
        public virtual object Result
        {
            get
            {
                if (!_finished)
                {
                    _waitHandle.WaitOne(); // Result blocks until task is completed making the task synchronous
                }

                if (_succeeded)
                {
                    return _result;
                }
                else
                {
                    throw _exception;
                }
            }
        }

        protected bool CanDispatch
        {
            get
            {
                return _dispatcher != null;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Combines multiple tasks of the same type into one callback and runs them simultaneously. On failure the failure callback is called immediately
        /// and the result of the other tasks is discarded.
        /// </summary>
        /// <param name="tasks">A series of tasks to execute in parallel</param>
        public static UnityTask All(params UnityTask[] tasks)
        {
            return All(null, tasks);
        }

        /// <summary>
        /// Combines multiple tasks of the same type into one callback and runs them sequentially. On failure the failure callback is called immediately
        /// and the result of the other tasks is discarded.
        /// </summary>
        /// <param name="tasks">A series of tasks to execute in sequential orders</param>
        public static UnityTask AllSequential(params Func<UnityTask>[] tasks)
        {
            return AllSequential(null, tasks);
        }

        /// <summary>
        /// Fire all the progress callbacks with the current progress
        /// </summary>
        /// <param name="progress">A floating point value representing the current progress</param>
        public void Notify(float progress)
        {
            foreach (var callback in _progressCallbacks)
            {
                if (CanDispatch)
                {
                    _dispatcher.Dispatch( () => callback(progress) );
                }
                else
                {
                    callback(progress);
                }
            }
        }

        /// <summary>
        /// Finish the task with an error state
        /// </summary>
        /// <param name="error">Error details.</param>
        public void Reject(Exception error)
        {
            if (_finished)
            {
                throw new InvalidOperationException("Cannot reject a task that is already finished");
            }

            _finished = true;
            _result = null;
            _exception = error;
            _succeeded = false;

            _waitHandle.Set(); // Unblock synchronous results

            foreach (var callback in _failureCallbacks)
            {
                Action<Exception> cb = callback; // Copy reference so that lambda executes properly
                if (CanDispatch)
                {
                    _dispatcher.Dispatch( () => cb(error) );
                }
                else
                {
                    cb(error);
                }
            }

            FireEndCallbacks();
        }

        /// <summary>
        /// Finish the task with a result
        /// </summary>
        /// <param name="value">The result of the task.</param>
        public void Resolve(object value = null)
        {
            if (_finished)
            {
                throw new InvalidOperationException("Cannot accept a task that is already finished");
            }

            _finished = true;
            _result = value;
            _waitHandle.Set(); // Unblock synchronous result
            _succeeded = true;

            foreach (var callback in _successCallbacks)
            {
                Action<object> cb = callback; // Copy reference so that lambda executes properly
                if (CanDispatch)
                {
                    _dispatcher.Dispatch( () => cb(value) );
                }
                else
                {
                    cb(value);
                }
            }

            FireEndCallbacks();
        }

        /// <summary>
        /// Add callbacks that will fire on certain task states. If the task has already completed or failed, the related callbacks will fire immediately.
        /// Returns the this task so that calls can be chained together.
        /// </summary>
        /// <param name="onFulfilled">Callback that fires when the task succeeds</param>
        /// <param name="onFailure">Callback that fires when the task fails</param>
        /// <param name="onProgress">Callback that fires when the progress of the task changes</param>
        /// <param name="onEnd">Callback that fires when the task succeeds or fails</param>
        public UnityTask Then(Action<object> onFulfilled = null, Action<Exception> onFailure = null, Action<float> onProgress = null, Action onEnd = null)
        {
            if (onFulfilled != null)
            {
                if (_finished && _succeeded)
                {
                    if (CanDispatch)
                    {
                        _dispatcher.Dispatch( () => onFulfilled(_result) );
                    }
                    else
                    {
                        onFulfilled(_result);
                    }
                }
                else
                {
                    _successCallbacks.Add(onFulfilled);
                }
            }

            if (onFailure != null)
            {
                if (_finished && !_succeeded)
                {
                    if (CanDispatch)
                    {
                        _dispatcher.Dispatch( () => onFailure(_exception) );
                    }
                    else
                    {
                        onFailure(_exception);
                    }
                }
                else
                {
                    _failureCallbacks.Add(onFailure);
                }
            }

            if (onProgress != null)
            {
                _progressCallbacks.Add(onProgress);
            }

            if (onEnd != null)
            {
                _endCallbacks.Add(onEnd);
            }

            return this;
        }

        /// <summary>
        /// Combines multiple tasks of the same type into one callback and runs them simultaneously. On failure the failure callback is called immediately
        /// and the result of the other tasks is discarded.
        /// </summary>
        /// <param name="tasks">A series of tasks to execute in paralell</param>
        private static UnityTask All(IDispatcher dispatcher, params UnityTask[] tasks)
        {
            UnityTask combinedTask = new UnityTask(dispatcher);

            int activeTaskCount = tasks.Length;
            object[] taskResults = new object[activeTaskCount];
            bool tasksSucceeding = true;

            for (int i = 0; i < tasks.Length; i++)
            {
                UnityTask task = tasks[i];
                int taskCount = i;

                task.Then(
                    onFulfilled: (object result) =>
                    {
                        if (!tasksSucceeding)
                            return;

                        taskResults[taskCount] = result;

                        activeTaskCount--;
                        if (activeTaskCount == 0)
                        {
                            combinedTask.Resolve(taskResults);
                        }

                    },
                    onFailure: (Exception ex) =>
                    {
                        tasksSucceeding = false;
                        combinedTask.Reject(ex); // Bail if one of the task errors out
                    }
                );
            }

            return combinedTask;
        }

        /// <summary>
        /// Combines multiple tasks of the same type into one callback and runs them sequentially. On failure the failure callback is called immediately
        /// and the result of the other tasks is discarded.
        /// </summary>
        /// <param name="tasks">A series of tasks to execute in sequential orders</param>
        private static UnityTask AllSequential(IDispatcher dispatcher, params Func<UnityTask>[] tasks)
        {
            UnityTask combinedTask = new UnityTask(dispatcher);
            List<Action> sequentialActions = new List<Action>();

            int activeTaskCount = tasks.Length;
            object[] taskResults = new object[activeTaskCount];

            for (int i = 0; i < tasks.Length; i++)
            {
                int taskCount = i;

                sequentialActions.Add(() =>
                {
                    UnityTask task = tasks[taskCount]();

                    task.Then(
                        onFulfilled: (object result) =>
                        {
                            taskResults[taskCount] = result;

                            if (taskCount == sequentialActions.Count - 1)
                            {
                                // Last task
                                combinedTask.Resolve(taskResults);
                            }
                            else
                            {
                                sequentialActions[taskCount+1]();
                            }
                        },
                        onFailure: (Exception e) =>
                        {
                            combinedTask.Reject(e);
                        }
                    );
                });

            }

            sequentialActions[0]();

            return combinedTask;
        }

        /// <summary>
        /// Called internally to trigger all the onEnd callbacks
        /// </summary>
        private void FireEndCallbacks()
        {
            foreach (var callback in _endCallbacks)
            {
                Action cb = callback; // Copy reference so that lambda executes properly
                if (CanDispatch)
                {
                    _dispatcher.Dispatch( cb );
                }
                else
                {
                    cb();
                }
            }
        }

        #endregion Methods
    }
}