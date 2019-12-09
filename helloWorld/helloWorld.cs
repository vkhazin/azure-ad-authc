using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace helloWorld
{
    public static class HelloWorld
    {
        [FunctionName("helloWorld")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var token = req.Headers["x-token"];

            if (string.IsNullOrEmpty(token))
                return UnauthorizedMessage();

            (var userData, var errorMessage) = await GetUserMetaData(token);

            return userData != null
                ? GetResponseMessage(HttpStatusCode.OK, new
                {
                    message = $"Hello {userData.DisplayName ?? userData.UserPrincipalName} retrived from the authentication token"
                })
                : UnauthorizedMessage();
        }

        private static async Task<(UserMetaData data, string errorMessage)> GetUserMetaData(string token)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(requestMessage);

                var userData = JsonConvert.DeserializeObject<UserMetaData>(await response.Content.ReadAsStringAsync());
                if (userData == null)
                    throw new Exception("Unknown response from server");

                return (userData, "");
            }
            catch (WebException ex)
            {
                return (null, ex.Message);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        private static HttpResponseMessage UnauthorizedMessage()
            => GetResponseMessage(HttpStatusCode.Unauthorized, new { message = "Invalid or expired token" });

        private static HttpResponseMessage GetResponseMessage(HttpStatusCode code, object response)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }
    }
}
