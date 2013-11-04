using System;
using System.Threading;
using System.Collections.Generic;

namespace UnityTools.Threading
{
	/// <summary>
	/// A helper class inspired by the .NET 4.0 Task object and javascript flow libraries such as Q and jQuery deferred
	/// TODO: Make a UnityTask that works without a type param so that methods that return nothing dont need to use a type like bool
	/// </summary>
	public class UnityTask<T>
	{
		protected T _result;
		protected bool _finished;
		protected bool _succeeded;
		protected EventWaitHandle _waitHandle;
		protected List<Action<T>> _successCallbacks = new List<Action<T>>();
		protected List<Action<Exception>> _failureCallbacks = new List<Action<Exception>>();
		protected bool _dispatch;
		protected Exception _exception = null;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="UnityTask`1"/> class.
		/// </summary>
		/// <param name="dispatchOnUnityThread">If set to <c>true</c> than dispatch callbacks on unity thread.</param>
		public UnityTask(bool dispatchOnUnityThread = true)
		{
			_waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
			_dispatch = dispatchOnUnityThread;
		}
		
		/// <summary>
		/// Combines multiple tasks of the same type into one callback. On failure the failure callback is called immediately
		/// and the result of the other tasks is discarded.
		/// </summary>
		/// <param name="tasks">A series of tasks to execute in paralell</param>
		public static UnityTask<T[]> When(params UnityTask<T>[] tasks)
		{
			UnityTask<T[]> combinedTask = new UnityTask<T[]>();
			
			int activeTaskCount = tasks.Length;
			T[] taskResults = new T[activeTaskCount];
			bool tasksSucceeding = true;
		
			for (int i = 0; i < tasks.Length; i++)
			{
				UnityTask<T> task = tasks[i];
				int taskCount = i;
				
				task.OnSuccess((T result) => 
				{
					if (!tasksSucceeding)
						return;
					
					taskResults[taskCount] = result;
					
					activeTaskCount--;
					if (activeTaskCount == 0)
					{
						combinedTask.Accept(taskResults);
					}
				
				}).OnFailure((Exception ex) =>
				{
					tasksSucceeding = false;
					combinedTask.Reject(ex); // Bail if one of the task errors out
				});
			
			}
			
			return combinedTask;
		}	
		
		/// <summary>
		/// Finish the task with a result
		/// </summary>
		/// <param name="value">The result of the task.</param>
		public void Accept(T value)
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
				if (_dispatch)
				{
					ActionDispatcher.Dispatch( () => callback(value) );
				}
				else
				{
					callback(value);
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
			_result = default(T);
			_exception = error;
			_waitHandle.Set(); // Unblock synchronous results
			_succeeded = false;
			
			foreach (var callback in _failureCallbacks)
			{
				if (_dispatch)
				{
					ActionDispatcher.Dispatch( () => callback(error) );
				}
				else
				{
					callback(error);
				}
			}
		}
		
		/// <summary>
		/// Add a callback that will fire if this task succeeds. If the task has already completed than the callback will fire immediately.
		/// Returns the same task so that calls can be chained together
		/// </summary>
		/// <param name="callback">The callback that will fire when the task succeeds</param>
		public UnityTask<T> OnSuccess(Action<T> callback)
		{
			if (_finished && _succeeded)
			{
				if (_dispatch)
				{
					ActionDispatcher.Dispatch( () => callback(_result) );
				}
				else
				{
					callback(_result);
				}
			}
			else
			{
				_successCallbacks.Add(callback);
			}
			return this;
		}
		
		/// <summary>
		/// Add a callback that will fire if this task fails. If the task has already failed than the callback will fire immediately.
		/// Returns the same task so that calls can be chained together
		/// </summary>
		/// <param name="callback">The callback that will fire when the task fails</param>
		public UnityTask<T> OnFailure(Action<Exception> callback)
		{
			if (_finished && !_succeeded)
			{
				callback(_exception);
			}
			else
			{
				_failureCallbacks.Add(callback);
			}
			return this;
		}
		
		/// <summary>
		/// Gets the result of the task. If the task has not completed, this call will block, making the task synchronous.
		/// If the task failed, the exeception will rethrown when this is called.
		/// </summary>
		/// <value>The result of the task</value>
		public T Result
		{
			get
			{
				if (_finished)
				{
					if (_succeeded)
					{
						return _result;
					}
					else
					{
						throw _exception;
					}
				}
				else
				{
					_waitHandle.WaitOne(); // Result blocks until task is completed making the task synchronous
					return _result;
				}
			}	
		
		}
		
	}
}

