using System;
using System.Collections.Generic;
using System.Text;

namespace authenticate
{
    public class AppSettings
    {
        public string ClientId { get; }
        public string DirectoryName { get; }

        public AppSettings(string clientId, string directoryName)
        {
            ClientId = clientId;
            DirectoryName = directoryName;
        }
    }
}
