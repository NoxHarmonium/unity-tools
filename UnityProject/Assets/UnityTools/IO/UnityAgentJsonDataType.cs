namespace UnityTools.IO
{
    using System;
    using System.IO;
    using System.Text;

    using LitJson;

    public class UnityAgentJsonDataType : UnityAgentDataType
    {
        #region Fields

        MemoryStream _memStream;
        JsonData _jsonData;

        #endregion Fields

        #region Constructors

        public UnityAgentJsonDataType()
            : base("application/json")
        {
        }

        #endregion Constructors

        #region Methods

		public void SetJsonData(JsonData data)
        {
            _jsonData = data;
        }

        public override void Dispose()
        {
            if (_memStream != null)
            {
                _memStream.Dispose();
                _memStream = null;
            }
        }

        public override Stream GetDataStream()
        {
            _memStream = new MemoryStream();
            var stringBytes = System.Text.Encoding.UTF8.GetBytes(_jsonData.ToString());
            _memStream.Write(stringBytes, 0, stringBytes.Length);
            _memStream.Seek(0, SeekOrigin.Begin);
            return _memStream;
        }

        #endregion Methods
    }
}