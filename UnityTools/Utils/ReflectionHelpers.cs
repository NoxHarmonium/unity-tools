using System;
using System.Linq;
using UnityEngine;

namespace UnityTools
{
    /// <summary>
    /// Helper methods to do with reflection
    /// </summary>
    public class ReflectionHelpers
    {
        
        public static Type[] GetTypesImplementing(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .ToArray();
        }
        
        public static Type[] GetComponentsImplementing(Type type)
        {
            Type[] types = GetTypesImplementing(type);
            return types
                .Where(t => t.IsAssignableFrom(typeof(Component)))
                .ToArray();
        
        }
    }
}

