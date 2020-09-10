using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    

    public class ClientCertificateOptions
    {
        public class ClientItem : ClientCertificateInfo
        {
            public CertificateProcessor Processor { get; set; }


            public Func<IServiceProvider, string, TokenCacheManager> TokenCacheManager { get; set; }
        }

        private List<ClientItem> _clientList = new List<ClientItem>();
        public IReadOnlyList<ClientItem> ClientList => _clientList;

        public string DefaultUrl { get; set; }

        public void AddClient(string url, string clientId, string clientSecret, string clientName="", CertificateProcessor processor=null, Func<IServiceProvider, string, TokenCacheManager> tokenManagerFactory=null)
        {
            var old = _clientList.FirstOrDefault(c => c.ClientID == clientId);
            if( old == null)
            {
                old = new ClientItem();
                _clientList.Add(old);
            }

            old.ClientID = clientId;
            old.ClientSecret = clientSecret;
            old.AuthUrl = url;
            old.ClientName = clientName;
            old.Processor = processor;
            old.TokenCacheManager = tokenManagerFactory;
        }


        public void SetTokenProcessor(string clientId, CertificateProcessor processor)
        {
            var old = _clientList.FirstOrDefault(c => c.ClientID == clientId);
            if (old != null)
            {
                old.Processor = processor;
            }
        }

        public void SetTokenProcessor(string clientId, Func<ClientCertificateInfo, HttpClient, Task<ClientCertificateToken>> processor)
        {
            var old = _clientList.FirstOrDefault(c => c.ClientID == clientId);
            if (old != null && processor != null)
            {
                old.Processor = CertificateProcessor.CreateDelegateProcessor(processor);
            }
        }

        public void SetTokenCacheManager(string clientId, Func<IServiceProvider, string, TokenCacheManager> factory)
        {
            var old = _clientList.FirstOrDefault(c => c.ClientID == clientId);
            if (old != null)
            {
                old.TokenCacheManager = factory;
            }
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
