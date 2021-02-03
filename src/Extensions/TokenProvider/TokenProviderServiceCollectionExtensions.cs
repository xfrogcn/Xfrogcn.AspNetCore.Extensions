using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TokenProviderServiceCollectionExtensions
    {
        public static IServiceCollection AddClientTokenProvider(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.Configure<ClientCertificateOptions>((options) =>{});
            serviceDescriptors.TryAddSingleton<IClientCertificateProvider, ClientCertificateProvider>();

            return serviceDescriptors;
        }

        public static IServiceCollection AddClientTokenProvider(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.AddClientTokenProvider();

            CurrentClientInfo cci = new CurrentClientInfo();
            configuration.Bind(cci);
            if (!String.IsNullOrEmpty(cci.ClientID) && !String.IsNullOrEmpty(cci.ClientSecret))
            {
                serviceDescriptors.AddSingleton(cci);
            }

            serviceDescriptors.Configure<ClientCertificateOptions>((options) =>
            {
                // 顶层默认设置（当前应用信息）
                if (!String.IsNullOrEmpty(cci.ClientID) && !String.IsNullOrEmpty(cci.ClientSecret))
                {
                    options.AddClient(cci.AuthUrl, cci.ClientID, cci.ClientSecret, cci.ClientName);
                }
                if (!String.IsNullOrEmpty(cci.AuthUrl))
                {
                    options.DefaultUrl = cci.AuthUrl;
                }
                IConfiguration section = configuration?.GetSection("_Clients");
                if (section != null)
                {
                    options.FromConfiguration(section);
                }
                
            });

            return serviceDescriptors;
        }

        public static IServiceCollection AddClientTokenProvider(this IServiceCollection serviceDescriptors, Action<ClientCertificateOptions> optionsAction)
        {
            serviceDescriptors.AddClientTokenProvider();

            serviceDescriptors.Configure<ClientCertificateOptions>(optionsAction);

            return serviceDescriptors;
        }

        public static IServiceCollection AddTokenClient(this IServiceCollection serviceDescriptors, ClientCertificateInfo ci)
        {
            if(ci!=null && !string.IsNullOrEmpty(ci.ClientID) && !string.IsNullOrEmpty(ci.ClientSecret))
            {
                return AddClientTokenProvider(serviceDescriptors, (options) =>
                {
                    options.AddClient(ci.AuthUrl, ci.ClientID, ci.ClientSecret, ci.ClientName);
                });
            }
            return serviceDescriptors;
        }

        public static IServiceCollection AddTokenClient(this IServiceCollection serviceDescriptors, string url, string clientId, string clientSecret, Action<ClientCertificateOptions.ClientItem> clientOptions=null)
        {
            return AddClientTokenProvider(serviceDescriptors, options =>
            {
                var client = options.AddClient(url, clientId, clientSecret);
                if (clientOptions != null)
                {
                    clientOptions(client);
                }
            });
        }

        /// <summary>
        /// 配置获取Token的HttpClient
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IHttpClientBuilder ConfigureTokenHttpClient(this IServiceCollection serviceDescriptors)
        {
            return serviceDescriptors.AddHttpClient(ClientCertificateManager.HTTP_CLIENT_NAME);
        }
    }
}
