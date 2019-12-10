using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace authenticate
{
    public static class AuthenticateFunc
    {
        [FunctionName("authenticate")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var appSettings = new AppSettings(
                Environment.GetEnvironmentVariable("ClientId"),
                Environment.GetEnvironmentVariable("DirectoryName"));

            if (string.IsNullOrEmpty(appSettings.ClientId))
                return GetBadRequestMessage("Configuration missing ClientId parameter");
            if (string.IsNullOrEmpty(appSettings.DirectoryName))
                return GetBadRequestMessage("Configuration missing DirectoryName parameter");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var credentials = JsonConvert.DeserializeObject<UserCredentials>(requestBody);

            var result = await AuthService.AcquireToken(appSettings, credentials);
            switch (result.Status)
            {
                case AuthServiceStatus.OK:
                    return GetResponseMessage(HttpStatusCode.OK, new { token = result.Token });
                case AuthServiceStatus.InvalidCredentials:
                    return GetResponseMessage(HttpStatusCode.Unauthorized, new { message = "Invalid username and/or password" });
                default:
                    return GetResponseMessage(HttpStatusCode.Unauthorized, new { message = result.ErrorMessage });
            }
        }

        private static HttpResponseMessage GetBadRequestMessage(string message)
            => GetResponseMessage(HttpStatusCode.BadRequest, new { message });

        private static HttpResponseMessage GetResponseMessage(HttpStatusCode code, object response)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }
    }
}
