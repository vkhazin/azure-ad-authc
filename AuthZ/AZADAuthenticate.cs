using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace AzureADAuthenticationService
{
    public static class AZADAuthenticate
    {

        private static string clientId = "b887052a-0472-46ae-9248-121f43b3de34"; //"08ea6579-47fb-487b-bc42-69f036824ac2";

        [FunctionName("AZADAuthenticate")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // string username = req.Query["username"];
            // string password = req.Query["password"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            username = username ?? data?.username;
            password = password ?? data?.password;


            var authority = "https://login.microsoftonline.com/techsky.onmicrosoft.com";
            var scopes = new string[] { "user.read" };
            IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authority).Build();

            var accounts = await app.GetAccountsAsync();

            AuthenticationResult result;

            var listAccounts = accounts.ToList();

            if (listAccounts.Any())
            {
                result = await app.AcquireTokenSilent(scopes, listAccounts.FirstOrDefault()).ExecuteAsync();
            }
            else
            {
                try
                {
                    var securePassword = new SecureString();
                    foreach (var c in password)
                        securePassword.AppendChar(c);

                    result = await app.AcquireTokenByUsernamePassword(scopes, username, securePassword).ExecuteAsync();
                }
                catch (MsalException exception)
                {
                    return new OkObjectResult(exception.ToString());
                }
            }
            return new OkObjectResult(result.AccessToken);


            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }



    }
}
