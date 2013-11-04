namespace UnityTools.Shared
{
    using System;

    using UnityEngine;

    public class UnityToolsSceneObject : MonoBehaviour
    {
        #region Fields

        public const string GAMEOBJECT_NAME = "UnityToolsSceneObject";

        protected static GameObject _instance;

        #endregion Fields

        #region Properties

        public static GameObject Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException(
                    "To use UnityTools you need to add a GameObject to your scene with a UnityToolsSceneObject component."+
                    "This cannot be done automatically as this property is not guaranteed to be called from a safe thread");
                }

                return _instance;
            }
        }

        #endregion Properties

        #region Methods

        private void Awake()
        {
            _instance = this.gameObject;
            foreach (Type t in ReflectionHelpers.GetComponentsImplementing(typeof(IInitialiseOnStartup)))
            {
               this.gameObject.AddComponent(t);
            }
        }

        #endregion Methods
    }
}