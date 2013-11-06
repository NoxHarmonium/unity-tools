namespace UnityTools.Tests
{
    using System;
    using System.Net;

    using NUnit.Framework;

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
            Assert.That(response.Body.Contains("\"gzipped\": true"));
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