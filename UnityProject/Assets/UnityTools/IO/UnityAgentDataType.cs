namespace UnityTools.IO
{
    using System;
    using System.IO;

    public abstract class UnityAgentDataType : IDisposable
    {
        #region Fields

        protected string _httpContentType;

        #endregion Fields

        #region Constructors

        public UnityAgentDataType(string httpContentType)
        {
            _httpContentType = httpContentType;
        }

        #endregion Constructors

        #region Properties

        public string HttpContentType
        {
            get
            {
                return _httpContentType;
            }
        }

        #endregion Properties

        #region Methods

        public abstract void Dispose();

        public abstract Stream GetDataStream();

        #endregion Methods
    }
}