using Microsoft.VisualStudio.TestTools.UnitTesting;
using authenticate;
using System;
using System.Collections.Generic;
using System.Text;

namespace authenticate.Tests
{
    [TestClass()]
    public class AuthServiceTests
    {
        [TestMethod()]
        public void AcquireTokenTest()
        {
            var resultSuccess = AuthService.AcquireToken(AppSettings, new UserCredentials() {
                Username = "chris@alexeybusyginhotmail.onmicrosoft.com",
                Password = "Tupa2441Tupa2441"
            }).Result;

            Assert.AreEqual(true, resultSuccess.IsAuthenticated);
            Assert.AreEqual(AuthServiceStatus.OK, resultSuccess.Status);

            var resultFail = AuthService.AcquireToken(AppSettings, new UserCredentials()
            {
                Username = "chris@alexeybusyginhotmail.onmicrosoft.com",
                Password = "xxxx"
            }).Result;

            Assert.AreEqual(false, resultFail.IsAuthenticated);
            Assert.AreEqual(AuthServiceStatus.InvalidCredentials, resultFail.Status);
        }

        public AppSettings AppSettings => new AppSettings(
            "5b1e59a8-ced1-41d5-9a72-01e38e02614f",
            "alexeybusyginhotmail.onmicrosoft.com");
    }
}