using System;

namespace UnityTools.Shared
{
    /// <summary>
    /// Classed implementing this interface will have their Initialise method
    /// called as soon as the <see cref="UnityToolsSceneObject"/> is initialised.
    /// This should be in the first frame of the application.
    /// </summary>
    public interface IInitialiseOnStartup
    {
        void Initialise();
    }
}

