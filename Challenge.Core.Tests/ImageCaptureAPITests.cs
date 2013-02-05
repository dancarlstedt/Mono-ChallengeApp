using System;
using System.IO;
using System.Linq;
using System.Net.Mime;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Extensions;

namespace Challenge.Core.Tests
{
    [TestClass]
    public class ImageCaptureAPITests
    {
        private const string CustId = "custId";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GET_With_Bogus_CustID_Returns_Resource_Not_Found()
        {
            var client = CreateClient();
            var request = CreateRequest(Method.GET);
            request.AddUrlSegment(CustId, "");

            var response = client.Execute(request);
            var content = response.Content;

            Assert.IsTrue(content.Contains("No HTTP resource was found"));
        }

        [TestMethod]
        public void DELETE_PUT_OPTIONS_PATCH_Arent_Supported()
        {
            var client = CreateClient();
            var unsupportedMethods = new[]
                                         {
                                             Method.DELETE,
                                             //Method.HEAD,
                                             Method.OPTIONS,
                                             Method.PATCH,
                                             //Method.GET,
                                             //Method.POST,
                                             Method.PUT
                                         };

            foreach (var unsupportedMethod in unsupportedMethods)
            {
                var request = CreateRequest(unsupportedMethod);
                request.AddUrlSegment(CustId, "22");

                var response = client.Execute(request);
                Assert.IsTrue(response.Content.Contains("does not support"), string.Format("{0} should not be supported!", unsupportedMethod.ToString()));
            }
        }

        [TestMethod]
        public void HEAD_Returns_Nothing()
        {
            var client = CreateClient();
            var request = CreateRequest(Method.HEAD);
            request.AddUrlSegment(CustId, "0000");

            var response = client.Execute(request);

            Assert.AreEqual(string.Empty, response.Content);
        }

        [TestMethod]
        public void POST_WITH_101010_Will_Give_Location()
        {
            var client = CreateClient();
            var request = CreateRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment(CustId, "101010");

            var image = File.ReadAllBytes(@"C:\images.jpg");
            request.AddBody(new { Date = DateTime.Now.Date, ImageDDD = image, CustId = "BIG PIMPIN" });

            var response = client.Execute<ImageCaptureAPIResult>(request);
            var location = response.Headers.FirstOrDefault(x => x.Name == "Location");
            Assert.IsNotNull(location);
        }

        [TestMethod]
        public void POST_Location_Given_Tells_Us_The_Answer_To_The_Ultimate_Question()
        {
            var client = CreateClient();
            var request = CreateRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment(CustId, "101010");

            var image = File.ReadAllBytes(@"C:\images.jpg");
            request.AddBody(new { Date = DateTime.Now.Date, ImageDDD = image, CustId = "BIG PIMPIN" });

            var response = client.Execute<ImageCaptureAPIResult>(request);
            var location = response.Headers.FirstOrDefault(x => x.Name == "Location");

            var locationClient = new RestClient(location.Value as string);
            var locationRequest = new RestRequest(Method.GET);
             
            //locationRequest.AddUrlSegment(CustId, "101010");

            var locationResponse = locationClient.Execute<System.Collections.Generic.List<string>>(locationRequest);
            Assert.IsNotNull(locationResponse);
        }


        [TestMethod]
        public void CustID_Must_Be_A_Number()
        {
            var client = CreateClient();
            var request = CreateRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment(CustId, "what what");

            var image = File.ReadAllBytes(@"C:\images.jpg");
            request.AddBody(new { Dates = DateTime.Now.Date, Image = image, CustId = "BIG PIMPIN" });

            var response = client.Execute<ImageCaptureAPIResult>(request);
            var location = response.Headers.FirstOrDefault(x => x.Name == "Location");
            Assert.IsNull(location);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("The request is invalid.", response.Data.Message);
        }

        private static RestClient CreateClient()
        {
            return new RestClient("http://inttesttwilio.fcsamerica.com/imagecapture/api/");
        }

        private static RestRequest CreateRequest(Method method)
        {
            return new RestRequest("images/?custId={custId}", method);
        }
    }
}
