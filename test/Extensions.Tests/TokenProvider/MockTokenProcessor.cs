using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace Extensions.Tests.TokenProvider
{
    public class MockTokenProcessor : CertificateProcessor
    {
        internal int exeCount = 0;

        public override async Task<ClientCertificateToken> GetToken(ClientCertificateInfo clientInfo, IHttpClientFactory clientFactory)
        {
            System.Threading.Interlocked.Increment(ref exeCount);
            await Task.Delay(100);
            return new ClientCertificateToken()
            {
                access_token = "1",
                expires_in = 60,
                token_type = "1"
            };
        }
    }
}
