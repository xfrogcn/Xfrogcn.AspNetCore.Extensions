using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;

namespace Xfrogcn.AspNetCore.Extensions
{
    public abstract class CertificateProcessor
    {
        public const string HTTP_CLIENT_NAME = nameof(ClientCertificateManager);

        public abstract Task<ClientCertificateToken> GetToken(ClientCertificateInfo clientInfo, IHttpClientFactory clientFactory);

        public static readonly CertificateProcessor OIDC = new OIDCCertificateProcessor();

        public static CertificateProcessor CreateDelegateProcessor(Func<ClientCertificateInfo, HttpClient, Task<ClientCertificateToken>> proc)
        {
            return new DelegateCertificateProcessor(proc);
        }


        public class OIDCCertificateProcessor : CertificateProcessor
        {
            
            public override async Task<ClientCertificateToken> GetToken(ClientCertificateInfo clientInfo, IHttpClientFactory clientFactory)
            {
                Dictionary<String, string> dic = new Dictionary<string, string>()
                    {
                        {"grant_type","client_credentials" },
                        {"client_id", clientInfo.ClientID },
                        {"client_secret", clientInfo.ClientSecret }
                    };
                var httpClient = clientFactory.CreateClient(HTTP_CLIENT_NAME);

                httpClient.BaseAddress = new Uri(clientInfo.AuthUrl);
                ClientCertificateToken bt = await httpClient.SubmitFormAsync<ClientCertificateToken>("/connect/token", dic);
                if (bt != null && !String.IsNullOrEmpty(bt.access_token))
                {
                    return bt;
                }

                return null;
            }
        }


        public class DelegateCertificateProcessor : CertificateProcessor
        {
            private readonly Func<ClientCertificateInfo, HttpClient, Task<ClientCertificateToken>> _proc;
            public DelegateCertificateProcessor(Func<ClientCertificateInfo, HttpClient, Task<ClientCertificateToken>> requestProc)
            {
                _proc = requestProc;
            }

            public override async Task<ClientCertificateToken> GetToken(ClientCertificateInfo clientInfo, IHttpClientFactory clientFactory)
            {
                var httpClient = clientFactory.CreateClient(HTTP_CLIENT_NAME);

                httpClient.BaseAddress = new Uri(clientInfo.AuthUrl);

                var token = await _proc(clientInfo, httpClient);

                return token;
            }
        }
    }
}
