namespace UnityTools.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using UnityTools.Threading;
    using UnityTools.Utils;

    using LitJson;

    public class UnityAgentRequest
    {
        #region Fields

        public const int BUFFER_SIZE = 4096;
        public const string DEFAULT_CONTENT_TYPE = "application/x-www-form-urlencoded";
        public const string USER_AGENT = "UnityAgent HTTP client";

        protected UnityAgentDataType _data;
        protected Dictionary<string, string> _queryVars;
        protected HttpWebRequest _webRequest;

        #endregion Fields

        #region Constructors

        public UnityAgentRequest(string verb, string url)
        {
            _webRequest = WebRequest.Create(url) as HttpWebRequest;
            _webRequest.Method = verb;
            _webRequest.ContentType = DEFAULT_CONTENT_TYPE;
            _webRequest.AllowWriteStreamBuffering = true;
            _webRequest.AllowAutoRedirect = true;
            _webRequest.UserAgent = USER_AGENT;
            _webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        #endregion Constructors

        #region Properties

        public bool AllowAutoRedirect
        {
            get
            {
                return _webRequest.AllowAutoRedirect;
            }
            set
            {
                _webRequest.AllowAutoRedirect = value;
            }
        }

        public string ContentType
        {
            get
            {
                if (_data != null)
                {
                    return _data.HttpContentType;
                }
                else
                {
                    return DEFAULT_CONTENT_TYPE;
                }

            }
        }

        #endregion Properties

        #region Methods

        public UnityTask<UnityAgentResponse> Begin()
        {
            return new UnityTask<UnityAgentResponse>(
                task =>
                {
                   if (_data != null)
                   {
                       using (Stream inStream = _data.GetDataStream())
                       {
                            _webRequest.ContentLength = inStream.Length;
                            using (Stream outStream = _webRequest.GetRequestStream())
                            {
                               inStream.CopyToSync(outStream, BUFFER_SIZE);
                            }
                       }
                   }

                   HttpWebResponse resp = _webRequest.GetResponse() as HttpWebResponse;
                   UnityAgentResponse resp_ = UnityAgentResponse.ReadFromWebResponse(resp);
                   task.Resolve(resp_);
                });
        }

        /// <summary>
        /// Sets the value of a query string in this request.
        /// </summary>
        /// <param name="key">The key of the query string pair..</param>
        /// <param name="value">The value of the query string pair.</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Query(string key, string value)
        {
            _queryVars[key] = value;
            return this;
        }

        /// <summary>
        /// Sets all the query string pairs of this request.
        /// </summary>
        /// <param name="key">The name of the header (the key).</param>
        /// <param name="value">The value of the header</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Query(Dictionary<string, string> queryVars)
        {
            _queryVars = queryVars;
            return this;
        }

        /// <summary>
        /// Sets the body data of the request as a string. This method assumes the use of
        /// the application/x-www-form-urlencoded data type (<see cref="UnityAgentFormDataType"/>) and
        /// will append the data each time this method is called seperated by an ampersand.
        /// To set a custom data type use the other overloads.
        /// </summary>
        /// <param name="data">The body data of the request.</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Send(string data)
        {
            if (_data == null)
            {
                var formData = new UnityAgentFormDataType();
                formData.AppendData(data);
                _data = formData;
            }
            else
            {
                if (_data is UnityAgentFormDataType)
                {
                    (_data as UnityAgentFormDataType).AppendData(data);
                }
                else
                {
                    throw new Exception("Data type is already set to something other than form. Cannot use this method.");
                }
            }
            return this;
        }

		/// <summary>
        /// Sets the body data of the request as a JSON object. This method assumes the use of
        /// the application/x-www-form-urlencoded data type (<see cref="UnityAgentFormDataType"/>) and
        /// will append the data each time this method is called seperated by an ampersand.
        /// To set a custom data type use the other overloads.
        /// </summary>
        /// <param name="data">The body data of the request.</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Send(JsonData data)
        {
            if (_data == null)
            {
                var jsonDataPayload = new UnityAgentJsonDataType();
				jsonDataPayload.SetJsonData(data);
				_data = jsonDataPayload;
            }
            else
            {
            	throw new Exception("Can only send one JSON payload per request.");
            }
            return this;
        }

        /// <summary>
        /// Sets the data of a request. This is a low level method. You would usually
        /// one of the other overloads of this method to send text or JSON data.
        /// </summary>
        /// <param name="body">The body data of the request.</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Send(UnityAgentDataType data)
        {
            if (_data != null)
            {
                throw new Exception("Cannot send more than data type objects");
            }
            else
            {
                _data = data;
            }
            return this;
        }

        /// <summary>
        /// Sets the value of a header in this request.
        /// Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.
        /// </summary>
        /// <param name="key">The name of the header (the key).</param>
        /// <param name="value">The value of the header</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Set(string key, string value)
        {
            _webRequest.Headers[key] = value;
            return this;
        }

        /// <summary>
        /// Sets all the headers of this request.
        /// </summary>
        /// <param name="key">The name of the header (the key).</param>
        /// <param name="value">The value of the header</param>
        /// <returns>Returns this <see cref="UnityAgentRequest"/> object so that calls can be chained.</returns>
        public UnityAgentRequest Set(Dictionary<string, string> headers)
        {
            _webRequest.Headers = new WebHeaderCollection();
               foreach (var kvp in headers)
               {
                _webRequest.Headers[kvp.Key] = kvp.Value;
               }
               return this;
        }

        #endregion Methods
    }
}