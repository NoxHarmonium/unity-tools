namespace UnityTools.IO
{
    using System;
    using System.IO;
    using System.Text;

    public class UnityAgentFormDataType : UnityAgentDataType
    {
        #region Fields

        MemoryStream _memStream;
        StringBuilder _stringBuilder;

        #endregion Fields

        #region Constructors

        public UnityAgentFormDataType()
            : base("application/x-www-form-urlencoded")
        {
            _stringBuilder = new StringBuilder();
        }

        #endregion Constructors

        #region Methods

        public void AppendData(string data)
        {
            _stringBuilder.Append(data);
        }

        public override void Dispose()
        {
            if (_memStream != null)
            {
                _memStream.Dispose();
                _memStream = null;
                _stringBuilder = null;
            }
        }

        public override Stream GetDataStream()
        {
            _memStream = new MemoryStream();
            var stringBytes = System.Text.Encoding.UTF8.GetBytes(_stringBuilder.ToString());
            _memStream.Write(stringBytes, 0, stringBytes.Length);
            _memStream.Seek(0, SeekOrigin.Begin);
            return _memStream;
        }

        #endregion Methods
    }
}