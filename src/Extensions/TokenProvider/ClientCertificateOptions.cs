using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class ClientCertificateInfo
    {
        public string ClientID { get; set; }

        public string ClientName { get; set; }

        public string AuthUrl { get; set; }

        public string ClientSecret { get; set; }
    }
    public class ClientCertificateOptions
    {
        private List<ClientCertificateInfo> _clientList = new List<ClientCertificateInfo>();
        public IReadOnlyList<ClientCertificateInfo> ClientList => _clientList;

        public string DefaultUrl { get; set; }

        public void AddClient(string url, string clientId, string clientSecret, string clientName="")
        {
            var old = _clientList.FirstOrDefault(c => c.ClientID == clientId);
            if( old == null)
            {
                old = new ClientCertificateInfo();
                _clientList.Add(old);
            }

            old.ClientID = clientId;
            old.ClientSecret = clientSecret;
            old.AuthUrl = url;
            old.ClientName = clientName;
            
        }

        public void FromConfiguration(IConfiguration configuration)
        {
            string url = configuration["AuthUrl"];
            if (!String.IsNullOrEmpty(url))
            {
                DefaultUrl = url;
            }

            var children = configuration.GetChildren();
            foreach(var c in children)
            {
                string clientId = c["ClientID"];
                string clientSecret = c["ClientSecret"];
                string curl = c["AuthUrl"];
                string clientName = c.Key ?? "";
                if(!String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(clientSecret))
                {
                    AddClient(curl, clientId, clientSecret, clientName);
                }

            }
        }
    }
}
