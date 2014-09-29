namespace UnityTools.Utils
{
    using System;
    using System.Linq;

    using UnityEngine;

    /// <summary>
    /// Helper methods to do with reflection
    /// </summary>
    public class ReflectionHelpers
    {
        #region Methods

        public static Type[] GetComponentsImplementing(Type type)
        {
            Type[] types = GetTypesImplementing(type);
            return types
                .Where(t => t.IsAssignableFrom(typeof(Component)))
                .ToArray();
        }

        public static Type[] GetTypesImplementing(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .ToArray();
        }

        #endregion Methods
    }
}