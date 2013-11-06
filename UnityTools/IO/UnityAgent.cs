namespace UnityTools.IO
{
    using System;

    /// <summary>
    /// A REST API Client designed with ease of use in mind.
    /// Based on SuperAgent <see href="http://visionmedia.github.io/superagent/"/>
    /// </summary>
    public class UnityAgent
    {
        #region Methods

        /// <summary>
        /// This specification reserves the method name CONNECT for use with a proxy that can dynamically switch to being a tunnel (e.g. SSL tunneling [44]).
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SingleConnect(string url)
        {
            return new UnityAgent().Connect(url);
        }

        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified by the Request-URI. 
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SingleDelete(string url)
        {
            return new UnityAgent().Delete(url);
        }

        /// <summary>
        /// The GET method means retrieve whatever information (in the form of an entity) is identified by the Request-URI.
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SingleGet(string url)
        {
            return new UnityAgent().Get(url);
        }

        /// <summary>
        /// The HEAD method is identical to GET except that the server MUST NOT return a message-body in the response. 
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SingleHead(string url)
        {
            return new UnityAgent().Head(url);
        }

        /// <summary>
        /// The OPTIONS method represents a request for information about the communication options available on the request/response chain identified by the Request-URI. 
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SingleOptions(string url)
        {
            return new UnityAgent().Options(url);
        }

        /// <summary>
        /// The actual function performed by the POST method is determined by the server and is usually dependent on the Request-URI. 
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SinglePost(string url)
        {
            return new UnityAgent().Post(url);
        }

        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied Request-URI. 
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SinglePut(string url)
        {
            return new UnityAgent().Put(url);
        }

        /// <summary>
        /// The TRACE method is used to invoke a remote, application-layer loop-back of the request message. The final recipient of the request SHOULD reflect the message received back to the client as the entity-body of a 200 (OK) response. 
        /// This is the static version of the method that will not retain state between different actions.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public static UnityAgentRequest SingleTrace(string url)
        {
            return new UnityAgent().Trace(url);
        }
        
         /// <summary>
        /// The PATCH method is used to apply partial modifications to a resource.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://tools.ietf.org/html/rfc5789"/>
        public static UnityAgentRequest SinglePatch(string url)
        {
            return new UnityAgent().Patch(url);
        }


        /// <summary>
        /// This specification reserves the method name CONNECT for use with a proxy that can dynamically switch to being a tunnel (e.g. SSL tunneling [44]).
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Connect(string url)
        {
            return CreateRequest("CONNECT", url);
        }

        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified by the Request-URI. 
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Delete(string url)
        {
            return CreateRequest("DELETE", url);
        }

        /// <summary>
        /// The GET method means retrieve whatever information (in the form of an entity) is identified by the Request-URI.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Get(string url)
        {
            return CreateRequest("GET", url);
        }

        /// <summary>
        /// The HEAD method is identical to GET except that the server MUST NOT return a message-body in the response. 
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Head(string url)
        {
            return CreateRequest("HEAD", url);
        }

        /// <summary>
        /// The OPTIONS method represents a request for information about the communication options available on the request/response chain identified by the Request-URI. 
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Options(string url)
        {
            return CreateRequest("OPTIONS", url);
        }

        /// <summary>
        /// The actual function performed by the POST method is determined by the server and is usually dependent on the Request-URI. 
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Post(string url)
        {
            return CreateRequest("POST", url);
        }

        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied Request-URI. 
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Put(string url)
        {
            return CreateRequest("PUT", url);
        }

        /// <summary>
        /// The TRACE method is used to invoke a remote, application-layer loop-back of the request message. The final recipient of the request SHOULD reflect the message received back to the client as the entity-body of a 200 (OK) response. 
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>
        public UnityAgentRequest Trace(string url)
        {
            return CreateRequest("TRACE", url);
        }
        
        /// <summary>
        /// The PATCH method is used to apply partial modifications to a resource.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <see href="http://tools.ietf.org/html/rfc5789"/>
        public UnityAgentRequest Patch(string url)
        {
            return CreateRequest("PATCH", url);
        }


        protected UnityAgentRequest CreateRequest(string verb, string url)
        {
            return new UnityAgentRequest(verb, url);
        }

        #endregion Methods
    }
}