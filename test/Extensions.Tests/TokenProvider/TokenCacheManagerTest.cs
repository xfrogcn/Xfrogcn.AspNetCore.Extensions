using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xfrogcn.AspNetCore.Extensions;
using Xunit;
namespace Extensions.Tests.TokenProvider
{
    [Trait("", "TokenProvider")]
    public class TokenCacheManagerTest
    {
        [Fact(DisplayName = "内存缓存-设置及获取Token")]
        public async Task MemoryCache_SetAndGetToken()
        {
            MemoryTokenCacheManager tokenManger = new MemoryTokenCacheManager("1");
            TokenCache tc = new TokenCache();
            await tokenManger.SetToken(tc);

            var tc2 = await tokenManger.GetToken();

            Assert.Equal(tc, tc2);

            await tokenManger.RemoveToken();

            tc2 = await tokenManger.GetToken();
            Assert.Null(tc2);
        }

        [Fact(DisplayName = "内存缓存-并发")]
        public void MemoryCache_Test()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLogging()
                .AddHttpClient();
            var sp = sc.BuildServiceProvider();

            ClientCertificateInfo ci = new ClientCertificateInfo()
            {
                ClientID = "1",
                AuthUrl = "1",
                ClientSecret = "1"
            };
            MockTokenProcessor processor = new MockTokenProcessor();
            SetTokenProcessor setter = SetTokenProcessor.Bearer;
            MemoryTokenCacheManager cacheManager = new MemoryTokenCacheManager(ci.ClientID);
            ClientCertificateManager ccm = new ClientCertificateManager(
                ci, processor,
                setter,
                cacheManager,
                sp.GetRequiredService<ILogger<ClientCertificateManager>>(),
                sp.GetRequiredService<IHttpClientFactory>());


            List<Task> taskList = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run( async ()=>
                {
                    await ccm.GetAccessToken();
                }));
            }

            Task.WaitAll(taskList.ToArray());

            Assert.Equal(1, processor.exeCount);
        }

        [Fact(DisplayName = "分布式缓存-设置及获取Token")]
        public async Task DistributedCache_SetAndGetToken()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddDistributedMemoryCache();
            var sp = sc.BuildServiceProvider();

            DistributedTokenCacheManager tokenManger = new DistributedTokenCacheManager(sp.GetRequiredService<IDistributedCache>(), "1");
            TokenCache tc = new TokenCache() { access_token ="1"};
            await tokenManger.SetToken(tc);

            var tc2 = await tokenManger.GetToken();

            Assert.Equal(tc.access_token, tc2.access_token);

            await tokenManger.RemoveToken();

            tc2 = await tokenManger.GetToken();
            Assert.Null(tc2);
        }

        [Fact(DisplayName = "分布式缓存-并发")]
        public void DistributedCache_Test()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLogging()
                .AddHttpClient()
                .AddDistributedMemoryCache();
            var sp = sc.BuildServiceProvider();

            ClientCertificateInfo ci = new ClientCertificateInfo()
            {
                ClientID = "1",
                AuthUrl = "1",
                ClientSecret = "1"
            };
            MockTokenProcessor processor = new MockTokenProcessor();
            DistributedTokenCacheManager cacheManager = new DistributedTokenCacheManager(
                sp.GetRequiredService<IDistributedCache>(),
                ci.ClientID);
            ClientCertificateManager ccm = new ClientCertificateManager(
                ci, processor,
                SetTokenProcessor.Bearer,
                cacheManager,
                sp.GetRequiredService<ILogger<ClientCertificateManager>>(),
                sp.GetRequiredService<IHttpClientFactory>());


            List<Task> taskList = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                taskList.Add(Task.Run(async () =>
                {
                    await ccm.GetAccessToken();
                }));
            }

            Task.WaitAll(taskList.ToArray());

            Assert.Equal(1, processor.exeCount);
        }

    }
}
