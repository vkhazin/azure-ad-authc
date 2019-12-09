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

namespace Authenticate
{
    public static class Authenticate
    {
        [FunctionName("authenticate")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var cliendId = Environment.GetEnvironmentVariable("ClientId");
            var directoryName = Environment.GetEnvironmentVariable("DirectoryName");

            if (string.IsNullOrEmpty(cliendId))
                return GetBadRequestMessage("Configuration missing ClientId parameter");
            if (string.IsNullOrEmpty(directoryName))
                return GetBadRequestMessage("Configuration missing DirectoryName parameter");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var credentials = JsonConvert.DeserializeObject<Credentials>(requestBody);
            var message = "Invalid username and/or password";

            if (credentials != null)
            {
                var scopes = new string[] { "user.read" };
                var app = PublicClientApplicationBuilder.Create(cliendId)
                    .WithAuthority($"https://login.microsoftonline.com/{directoryName}").Build();

                try
                {
                    var result = app.AcquireTokenByUsernamePassword(
                        scopes,
                        credentials.Username,
                        new NetworkCredential("", credentials.Password).SecurePassword
                        ).ExecuteAsync().Result;

                    return GetResponseMessage(HttpStatusCode.OK, new { token = result.AccessToken });
                }
                catch (Exception exception)
                {
                    exception = exception is MsalException || exception.InnerException == null || !(exception.InnerException is MsalException)
                        ? exception
                        : exception.InnerException;

                    if (exception is MsalException)
                    {

                    }
                    message = exception.Message;
                }
            }

            return GetResponseMessage(HttpStatusCode.Unauthorized, new { message });
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

        private class Credentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
