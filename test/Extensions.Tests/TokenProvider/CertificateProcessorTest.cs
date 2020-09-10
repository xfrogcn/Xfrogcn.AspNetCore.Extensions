using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xfrogcn.AspNetCore.Extensions;
using Xunit;

namespace Extensions.Tests.TokenProvider
{
    [Trait("", "TokenProvider")]
    public class CertificateProcessorTest
    {
        [Fact(DisplayName = "OIDC Token获取")]
        public async Task OIDCProcessor_Test()
        {
            IServiceCollection sc = new ServiceCollection();
            HttpRequestMessage request = null;
            sc.AddHttpClient(CertificateProcessor.OIDCCertificateProcessor.HTTP_CLIENT_NAME)
            .AddMockHttpMessageHandler()
            .AddMock("*", HttpMethod.Post, async (req, res) =>
            {
                request = req;
                await res.WriteObjectAsync(new ClientCertificateToken()
                {
                    access_token = "1",
                    token_type = "1",
                    expires_in = 60
                });
            });

            var sp = sc.BuildServiceProvider();

            IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();

            var processor = CertificateProcessor.OIDC;
            var token = await  processor.GetToken(new ClientCertificateInfo()
            {
                ClientID = "1",
                ClientName = "1",
                ClientSecret = "1",
                AuthUrl = "https://auth.com"
            }, factory);

            var reqStr = await request.GetObjectAsync<string>();

            Assert.Equal("grant_type=client_credentials&client_id=1&client_secret=1", reqStr);

        }


        [Fact(DisplayName = "委托Token获取")]
        public async Task DelegateProcessor_Test()
        {
            IServiceCollection sc = new ServiceCollection();
            HttpRequestMessage request = null;
            sc.AddHttpClient(CertificateProcessor.HTTP_CLIENT_NAME)
            .AddMockHttpMessageHandler()
            .AddMock("*", HttpMethod.Get, async (req, res) =>
            {
                request = req;
                await res.WriteObjectAsync(new ClientCertificateToken()
                {
                    access_token = "1",
                    token_type = "1",
                    expires_in = 60
                });
            });

            var sp = sc.BuildServiceProvider();

            IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();

            var processor = CertificateProcessor.CreateDelegateProcessor(async (ci, client)=>
            {
                return await client.GetAsync<ClientCertificateToken>("/cgi-bin/token", new NameValueCollection()
                {
                    { "grant_type", "client_credential" },
                    { "appid", ci.ClientID },
                    { "secret", ci.ClientSecret }
                });
            });
            var token = await processor.GetToken(new ClientCertificateInfo()
            {
                ClientID = "1",
                ClientName = "1",
                ClientSecret = "1",
                AuthUrl = "https://auth.com"
            }, factory);

            string reqStr = request.RequestUri.AbsoluteUri;

            Assert.Equal("https://auth.com/cgi-bin/token?grant_type=client_credential&appid=1&secret=1", reqStr);

        }



    }
}
