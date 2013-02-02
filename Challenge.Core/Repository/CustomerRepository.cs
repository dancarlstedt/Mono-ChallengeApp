using System.Linq;

using RestSharp;

using System;

namespace Challenge.Core.Repository
{
    public class CustomerRepository
    {
        private readonly string _serviceUri;

        public CustomerRepository(string serviceUri)
        {
            _serviceUri = serviceUri;
        }

        public string Submit(ImageCaptureEntity entity)
        {
            var client = new RestClient(_serviceUri);

            var request = new RestRequest("images/?custId={custId}", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment("custId", entity.Id);
            request.AddBody(entity);

            var response = client.Execute<ImageCaptureAPIResult>(request);

            if (response.Data != null)
            {
                throw new Exception(response.Data.Message);
            }

            var header = response.Headers.FirstOrDefault(x => x.Name == "Location");
            return header != null ? header.Name : string.Empty;
        }

        public string[] Get(string location)
        {
            var client = new RestClient(location);
            var request = new RestRequest(Method.GET);
            var response = client.Execute<LocationResult>(request);

            if (response.Data == null)
            {
                throw new Exception(response.Content);
            }

            return response.Data.Answers;
        }
    }
}
