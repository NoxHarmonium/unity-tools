using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Used to dispatch actions onto the main unity thread. Any threads using
/// callbacks should use the ActionDispatcher.Dispatch method.
/// </summary>
public class ActionDispatcher : MonoBehaviour
{
	#region Public static methods
	
	/// <summary>
	/// Dispatches the action asynchronously on the Unity main thread
	/// </summary>
	/// <param name="action">Action.</param>
	public static void Dispatch(Action action)
	{
		lock(_lockObj) {
			_actions.Add(action);
		}
	}
	
	/// <summary>
	/// Blocks the current thread until the action has been dispatched on the Unity main thread
	/// </summary>
	/// <param name="action">Action.</param>
	public static void DispatchWait(Action action)
	{
		if (GetCurrentThreadId() == _unityThreadId)
		{
			// Make sure you don't block the main thread if calling from the main thread causing a deadlock
			action();
		}
		else
		{
			EventWaitHandle handle;
			lock(_lockObj) {
				_actions.Add(action);
				handle = new EventWaitHandle(false, EventResetMode.ManualReset);
				_waitHandles.Add(handle);
			}
			handle.WaitOne();
		}
	}

	public static void Instantiate()
	{
		if (_instance == null) {
			_instance = new GameObject("ActionDispatcher").AddComponent<ActionDispatcher>();
		}
	}

	#endregion

	#region MonoBehaviour events
	private void Awake()
	{
		_unityThreadId = GetCurrentThreadId();
		
		if (_instance == null) {
			_instance = this;
		}
	}

	private void Update()
	{
		List<Action> actionsCopy = null;
		List<EventWaitHandle> waitHandlesCopy = null;
		lock (_lockObj) {
			actionsCopy = new List<Action>(_actions);
			waitHandlesCopy = new List<EventWaitHandle>(_waitHandles);
			_actions.Clear();
			_waitHandles.Clear();
		}
			
		foreach (var action in actionsCopy) {
			try {
				action();
			} catch (System.Exception e) {
				Debug.LogException(e);
			}
		}
		
		foreach (var waitHandle in waitHandlesCopy) {
			try {
				waitHandle.Set();
			} catch (System.Exception e) {
				Debug.LogException(e);
			}
		}
	}

	#endregion
	
	#region Private Methods
	
	private static int GetCurrentThreadId()
	{
		// Although AppDomain.GetCurrentThreadId is depricated, ManagedThreadId did not seem to work properly
		// on iOS, every ID seemed to be the same. 
		#pragma warning disable 618
		return AppDomain.GetCurrentThreadId();
		#pragma warning restore 618
	}
	
	#endregion

	#region Private static fields

	private static readonly object _lockObj = new object();
	private static readonly List<Action> _actions = new List<Action>();
	private static readonly List<EventWaitHandle> _waitHandles = new List<EventWaitHandle>();
	private static ActionDispatcher _instance = null;
	private static int _unityThreadId;

	#endregion

}

