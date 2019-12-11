using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
  //Response contract
  struct Token
  {
    public string token_type;
    public string access_token;
    public string scope;
    public string ext_expires_in;
    public string refresh_token;
    public string id_token;
  }

  public static class Authenticate
  {
    //Environment Variables
    private static string ClientId = Environment.GetEnvironmentVariable("AppClientID");
    private static string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");

    /// <summary>
    /// Azure function entry point
    /// </summary>
    /// <param name="username">username</param>
    /// <param name="password">password</param>
    /// <returns>Token</returns>
    [FunctionName("Authenticate")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req, 
        ILogger log)
    {
      try
      {
        log.LogInformation("Processing Authentication Request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string username = data?.username;
        string password = data?.password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
          return new BadRequestObjectResult(new { Message = "Missing username and/or password" });
        }

        try
        {
          var token = await GenerateToken(username, password);
          
          //Empty token means authentication has failed, weird but that's how it is
          if (string.IsNullOrEmpty(token.access_token)) {
            var result = new ObjectResult(new {
              message = "Invalid username and/or password"
            });
            result.StatusCode = StatusCodes.Status401Unauthorized;
            return result;
          
          } else {
            return new OkObjectResult(token);
          }
        }
        catch (MsalException)
        {
          var result = new ObjectResult("Error occurred retrieving token");
          result.StatusCode = StatusCodes.Status429TooManyRequests;
          return result;
        }
      }
      catch (Exception exception)
      {
        return new BadRequestObjectResult(new { exception.Message });
      }
    }


    /// <summary>
    /// Generate Token based on the passed Azure AD credentials
    /// </summary>
    /// <param name="username">username</param>
    /// <param name="password">password</param>
    /// <returns>Token</returns>
    private static async Task<Token> GenerateToken(string username, string password)
    {
      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri("https://login.microsoftonline.com");
        var request = new HttpRequestMessage(HttpMethod.Post, "/organizations/oauth2/v2.0/token");

        var keyValues = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("client_id", ClientId ),
            new KeyValuePair<string, string>("scope", "user.read openid profile offline_access"),
            new KeyValuePair<string, string>("client_secret",ClientSecret),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("grant_type", "password")
        };

        request.Content = new FormUrlEncodedContent(keyValues);
        var response = await client.SendAsync(request);

        var content = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
        return content;
      }
    }
  }
}
