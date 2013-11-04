using UnityTools.Shared;

namespace UnityTools.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using UnityEngine;

    /// <summary>
    /// The <see cref="UnityDispatcher"/> class allows you to execute code on the main Unity thread.
    /// This is useful for situations where you need code on a seperate thread (i.e. slow IO operations) 
    /// to access things that can only be accessed on the main thread. 
    /// </summary>
    /// <example>
    ///		UnityDispatcher.Dispatch( () => texture.SaveAsPNG("./tex.png") );
    /// </example> 
    public class UnityDispatcher : MonoBehaviour, IInitialiseOnStartup
    {
        #region Fields

        private static readonly List<Action> _actions = new List<Action>();
        private static readonly object _lockObj = new object();
        private static readonly List<EventWaitHandle> _waitHandles = new List<EventWaitHandle>();

        private static UnityDispatcher _instance = null;
        private static int _unityThreadId;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Dispatches the action asynchronously on the Unity main thread.
        /// The action will execute in the next frame update phase.
        /// </summary>
        /// <param name="action">Action.</param>
        public static void Dispatch(Action action)
        {
            lock(_lockObj) {
                _actions.Add(action);
            }
        }

        /// <summary>
        /// Dispatches the action asynchronously on the Unity main thread but blocks
        /// the current thread until the action is executed.
        /// The action will execute in the next frame update phase.
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

        private static int GetCurrentThreadId()
        {
            // Although AppDomain.GetCurrentThreadId is depricated, ManagedThreadId did not seem to work properly
            // on iOS, every ID seemed to be the same.
            #pragma warning disable 618
            return AppDomain.GetCurrentThreadId();
            #pragma warning restore 618
        }

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


        #region IInitialiseOnStartup implementation

        public void Initialise()
        {
            UnityToolsSceneObject.Instance.AddComponent<UnityDispatcher>();
        }

        #endregion
        #endregion Methods
    }
}