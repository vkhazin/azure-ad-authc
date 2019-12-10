using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace helloWorld
{
    public class GraphService
    {
        private readonly Uri GraphUri = new Uri("https://graph.microsoft.com/");
        private readonly string _token;

        public GraphService(string token)
        {
            _token = token;
        }

        public async Task<(UserMetaData data, GraphServiceResult result)> GetUserMetaData()
        {
            (var response, var result) = await SendGraphRequest("v1.0/me");

            var userData = result == GraphServiceResult.OK
                ? JsonConvert.DeserializeObject<UserMetaData>(response)
                : null;

            if (result == GraphServiceResult.OK && userData == null)
                result = GraphServiceResult.UnknownResponse;

            return (userData, result);
        }

        private async Task<(string response, GraphServiceResult result)> SendGraphRequest(string serviceUrl)
        {
            try
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(GraphUri, serviceUrl)))
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", _token);

                    using (var client = new HttpClient())
                    {
                        var response = await client.SendAsync(requestMessage);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                return (responseContent, GraphServiceResult.OK);
                            case HttpStatusCode.Unauthorized:
                                return (responseContent, GraphServiceResult.Unauthorized);
                            default:
                                return (responseContent, GraphServiceResult.UnknownResponse);
                        }
                    }
                }
            }
            catch
            {
                return (null, GraphServiceResult.UnknownResponse);
            }
        }
    }

    public enum GraphServiceResult
    {
        OK,
        Unauthorized,
        UnknownResponse
    }
}
