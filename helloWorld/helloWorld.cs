using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace helloWorld
{
    public static class HelloWorldFunc
    {
        [FunctionName("helloWorld")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var token = req.Headers["x-token"];

            if (string.IsNullOrEmpty(token))
                return UnauthorizedMessage();

            (var userData, var result) = await new GraphService(token).GetUserMetaData();

            return GetMessageFromResult(userData, result);
        }

        private static HttpResponseMessage GetMessageFromResult(UserMetaData userData, GraphServiceResult result)
        {
            switch (result)
            {
                case GraphServiceResult.OK:
                    return GetResponseMessage(HttpStatusCode.OK, GetHelloBody(userData));
                case GraphServiceResult.Unauthorized:
                    return UnauthorizedMessage();
                default:
                    return GetResponseMessage(HttpStatusCode.InternalServerError, "Unknown service response");
            }
        }

        private static string GetHelloBody(UserMetaData userData)
            => $"Hello {userData.DisplayName ?? userData.UserPrincipalName} retrived from the authentication token";

        private static HttpResponseMessage UnauthorizedMessage()
            => GetResponseMessage(HttpStatusCode.Unauthorized, "Invalid or expired token");

        private static HttpResponseMessage GetResponseMessage(HttpStatusCode code, string message)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { message }))
            };
        }
    }
}
