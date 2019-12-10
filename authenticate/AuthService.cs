using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace authenticate
{
    public class AuthService
    {
        public static async Task<AuthServiceResult> AcquireToken(
            AppSettings appSettings, UserCredentials credentials)
        {
            appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

            if (credentials == null)
                return new AuthServiceResult(AuthServiceStatus.InvalidCredentials);

            var scopes = new string[] { "user.read" };
            var app = PublicClientApplicationBuilder.Create(appSettings.ClientId)
                .WithAuthority($"https://login.microsoftonline.com/{appSettings.DirectoryName}").Build();

            try
            {
                var result = await app.AcquireTokenByUsernamePassword(
                    scopes,
                    credentials.Username,
                    new NetworkCredential("", credentials.Password).SecurePassword
                    ).ExecuteAsync();

                return new AuthServiceResult(result.AccessToken);
            }
            catch (Exception exception)
            {
                exception = exception is MsalException || exception.InnerException == null || !(exception.InnerException is MsalException)
                    ? exception
                    : exception.InnerException;

                return exception is MsalException && exception.Message.Contains("AADSTS50126")
                    ? new AuthServiceResult(AuthServiceStatus.InvalidCredentials)
                    : new AuthServiceResult(AuthServiceStatus.Error, $"{exception.GetType().Name}: {exception.Message}");
            }
        }
    }

    public class AuthServiceResult
    {
        public bool IsAuthenticated { get => !string.IsNullOrEmpty(Token); }
        public string Token { get; }
        public string ErrorMessage { get; }
        public AuthServiceStatus Status { get; }

        public AuthServiceResult(string token)
        {
            Token = token;
            Status = IsAuthenticated ? AuthServiceStatus.OK : AuthServiceStatus.InvalidCredentials;
        }

        public AuthServiceResult(AuthServiceStatus status)
        {
            Status = status;
        }

        public AuthServiceResult(AuthServiceStatus status, string errorMessage)
            : this(status)
        {
            ErrorMessage = errorMessage;
        }
    }

    public enum AuthServiceStatus
    {
        OK,
        InvalidCredentials,
        Error
    }
}
