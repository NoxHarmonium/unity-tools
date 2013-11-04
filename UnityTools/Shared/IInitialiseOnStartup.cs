namespace UnityTools.Shared
{
    using System;

    /// <summary>
    /// Classed implementing this interface will have their Initialise method
    /// called as soon as the <see cref="UnityToolsSceneObject"/> is initialised.
    /// This should be in the first frame of the application.
    /// </summary>
    public interface IInitialiseOnStartup
    {
        #region Methods

        void Initialise();

        #endregion Methods
    }
}