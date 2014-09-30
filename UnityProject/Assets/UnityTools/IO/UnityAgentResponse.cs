namespace UnityTools.IO
{
    using System;
    using System.IO;
    using System.Net;
	using LitJson;

    public class UnityAgentResponse
    {
        #region Fields

        protected string _body;
        protected HttpWebResponse _response;
		protected JsonData _cachedJSON;

        #endregion Fields

        #region Constructors

        private UnityAgentResponse(HttpWebResponse response)
        {
            this._response = response;
        }

        #endregion Constructors

        #region Properties

        public string Body
        {
            get
            {
                return _body;
            }
        }

        public WebResponse InternalWebResponse
        {
            get
            {
                return _response;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return _response.StatusCode;
            }
        }

        public string StatusDescription
        {
            get
            {
				return _response.StatusDescription;
            }
        }

		public JsonData JSON 
		{
			get 
			{
				if (_cachedJSON == null) 
				{
					_cachedJSON = JsonMapper.ToObject(this.Body);
				}
				return _cachedJSON;
			}
		}

        #endregion Properties

        #region Methods

        public static UnityAgentResponse ReadFromWebResponse(HttpWebResponse response)
        {
            using (Stream respStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(respStream);
                UnityAgentResponse agentResp = new UnityAgentResponse(response);
                agentResp._body = reader.ReadToEnd();
                agentResp._response = response;
                return agentResp;
            }
        }

        #endregion Methods
    }
}