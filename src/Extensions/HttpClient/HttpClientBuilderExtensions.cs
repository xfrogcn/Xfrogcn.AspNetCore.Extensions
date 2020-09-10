using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpClientBuilderExtensions
    {
        public static string DisableEnsureSuccessStatusCode_Key = "DisableEnsureSuccessStatusCode";

        public static MockHttpMessageHandlerOptions AddMockHttpMessageHandler(this IHttpClientBuilder builder)
        {
            MockHttpMessageHandlerOptions options = new MockHttpMessageHandlerOptions();
            builder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new MockHttpMessageHandler(options);
            });
            return options;
        }

        /// <summary>
        /// 添加一个消息处理器，通过此消息处理器会在请求过程中，自动从认证服务获取令牌并添加到请求头中
        /// 该处理器通过<seealso cref="IClientCertificateProvider"/>获取对应客户端的认证令牌
        /// </summary>
        /// <param name="builder">HttpClient构建器</param>
        /// <param name="clientId">认证客户端Id</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddTokenMessageHandler(this IHttpClientBuilder builder, string clientId)
        {
            builder.AddHttpMessageHandler((sp) =>
            {
                IClientCertificateProvider provider = sp.GetRequiredService<IClientCertificateProvider>();
                ClientCertificateManager cm = provider.GetClientCertificateManager(clientId);
                return new GetClientTokenMessageHandler(cm);
            });
            return builder;
        }

        /// <summary>
        /// 禁止HttpClient Extensions 方法自动调用EnsureSuccessStatusCode
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder DisableEnsureSuccessStatusCode(this IHttpClientBuilder builder)
        {
            builder.AddHttpMessageHandler((sp) =>
            {
                return new DisableEnsureSuccessStatusCodeMessageHandler();
            });
            return builder;
        }

        /// <summary>
        /// 注入HTTP请求日志记录
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpMessageHandlerFilter(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, MessageHandlerFilter>());
            return serviceDescriptors;
        }


        public static void DisableEnsureSuccessStatusCode(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(DisableEnsureSuccessStatusCode_Key))
            {
                request.Properties[DisableEnsureSuccessStatusCode_Key] = true;
            }
            else
            {
                request.Properties.TryAdd(DisableEnsureSuccessStatusCode_Key, true);
            }
        }

        public static bool IsDisableEnsureSuccessStatusCode(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(DisableEnsureSuccessStatusCode_Key))
            {
                return (bool)request.Properties[DisableEnsureSuccessStatusCode_Key];
            }
            return false;
        }

        public static void IgnoreRequestToken(this HttpRequestMessage request)
        {
            if (!request.Properties.ContainsKey(GetClientTokenMessageHandler.IGNORE_TOKEN_PROPERTY))
            {
                request.Properties.Add(GetClientTokenMessageHandler.IGNORE_TOKEN_PROPERTY, true);
            }
        }
    }
}
