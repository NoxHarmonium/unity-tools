namespace UnityTools.Tests
{
    using System;
    using System.Net;

    using NUnit.Framework;
    using LitJson;

    using UnityTools.IO;

    [TestFixture]
    public class IOTests
    {
        #region Fields

        private const string BASE_TEST_URL = "http://httpbin.org/";

        #endregion Fields

        #region Methods

        [Test]
        public void TestApiGetSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("get");

            var response = agent
                .Get(url)
                .Begin()
                .Result;


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			StringAssert.AreEqualIgnoringCase((string)response.Json["url"], url);
        }
        
        [Test]
        public void TestApiPostSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("post");
            string testString = Guid.NewGuid().ToString();

            var response = agent
                .Post(url)
                .Send(testString)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.Body.Contains(testString));
			StringAssert.AreEqualIgnoringCase((string)response.Json["url"], url);
        }
        
        [Test]
        public void TestApiPutSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("put");
            string testString = Guid.NewGuid().ToString();

            var response = agent
                .Put(url)
                .Send(testString)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.Body.Contains(testString));
			StringAssert.AreEqualIgnoringCase((string)response.Json["url"], url);
        }
        
        [Test]
        public void TestApiPatchSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("patch");
            string testString = Guid.NewGuid().ToString();

            var response = agent
                .Patch(url)
                .Send(testString)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.Body.Contains(testString));
			StringAssert.AreEqualIgnoringCase((string)response.Json["url"], url);
        }
        
        [Test]
        public void TestApiDeleteSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("delete");

            var response = agent
                .Delete(url)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Test]
        public void TestApiTraceSyncronous()
        {
            //TODO: Find a way to test this
            Assert.Inconclusive();
        }
        
         [Test]
        public void TestApiConnectSyncronous()
        {
            //TODO: Find a way to test this
            Assert.Inconclusive();
        }
        
        [Test]
        public void TestApiHeadSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("get");

            var response = agent
                .Head(url)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(response.Body.Length == 0);
        }

        [Test]
        public void TestApiGzipSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("gzip");

            var response = agent
                .Get(url)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.That((bool)response.Json["gzipped"] == true);
        }

		[Test]
        public void TestApiPostJsonSyncronous()
        {
            UnityAgent agent = new UnityAgent();
            string url = GetTestUrl("post");
			var testData = new { 
				Guid = Guid.NewGuid().ToString(), 
				Message = "Hello",
				Number = new Random().Next(),
				EmptyString = String.Empty,
				NestedObj = new {
					NestedKey = "nested string"
				}
			};
			
			JsonData sentJsonData = JsonMapper.ToJson(testData);

            var response = agent
                .Post(url)
				.Send(sentJsonData)
                .Begin()
                .Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
           
			JsonData recvJsonData = ExtractJsonFromForm(response);

			Assert.AreEqual((string)	recvJsonData["Guid"], 					testData.Guid);
			Assert.AreEqual((string)	recvJsonData["Message"], 				testData.Message);
			Assert.AreEqual((int)		recvJsonData["Number"], 				testData.Number);
			Assert.AreEqual((string)	recvJsonData["EmptyString"], 			String.Empty);
			Assert.AreEqual((string)	recvJsonData["NestedObj"]["NestedKey"], testData.NestedObj.NestedKey);
        }

		private JsonData ExtractJsonFromForm(UnityAgentResponse response) 
        {
			// Unfortunately when we send json data to httpbin it just
			// thinks it is a form variable. When it echos the request
			// back at us it comes back a bit mangled. 
			// We need to extract the JSON string manually.

			// Get the form node that contains the data
			JsonData formNode = response.Json["form"];

			// The JSON data is stored as the first key
			// of the form object
			// I would use LINQ but it is flakey on iOS
			// due to AOT compilation
			string jsonDataString = "";
			int count = 0;
			foreach (var key in formNode.Keys) 
			{
				jsonDataString = key;
				count++;
			}

			// There should not be more than one key
			// something is wrong otherwise
			Assert.AreEqual(count, 1);

			return JsonMapper.ToObject(jsonDataString);
		}


        private string GetTestUrl(string verb)
        {
            /*
            Supported verbs. Thanks httpbin.org!

            /get Returns GET data.
            /post Returns POST data.
            /put Returns PUT data.
            /patch Returns PATCH data.
            /delete Returns DELETE data
               */

               return (BASE_TEST_URL + verb);
        }


        private void TestVerbWithData(string verb)
        {
            
        }

        #endregion Methods
    }
}