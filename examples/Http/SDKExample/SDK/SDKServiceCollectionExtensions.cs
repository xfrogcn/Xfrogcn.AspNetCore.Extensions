using SDKExample.SDK;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SDKServiceCollectionExtensions
    {
        /// <summary>
        /// 添加TestSDK
        /// 此SDK的服务端使用Basic验证
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <param name="url"></param>
        /// <param name="clientId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddTestSDK(this IServiceCollection serviceDescriptors, string url, string clientId, string userName, string password)
        {
            // 加入SDK所使用的HttpClient客户端
            serviceDescriptors.AddTokenClient(url, clientId, "", options=>
            {
                options.UseBasicAuth(userName, password);
            });

            return serviceDescriptors.AddHttpClient(clientId, client =>
                {
                    client.BaseAddress = new System.Uri(url);
                })
                .AddTypedClient<TestApiClient>()
                .AddTokenMessageHandler(clientId);
        }
    }
}
