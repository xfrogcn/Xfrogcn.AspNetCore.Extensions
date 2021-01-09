using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        internal static WebApiConfig config = new WebApiConfig();

        internal static Action<WebApiConfig> _configAction = null;

       
        public static IServiceCollection AddExtensions(this IServiceCollection serviceDescriptors, IConfiguration configuration = null, Action<WebApiConfig> configAction = null, Action<LoggerConfiguration> configureLogger = null)
        {
            _configAction = configAction;

            if (configuration != null)
            {
                serviceDescriptors.Configure<WebApiConfig>(configuration);
                configuration.Bind(config);
            }
            configAction?.Invoke(config);

            if (config.EnableSerilog)
            {
                serviceDescriptors.AddDefaultSerilog(config, (logConfig) =>
                {
                    configureLogger?.Invoke(logConfig);
                    if (configuration != null)
                    {
                        logConfig.ReadFrom.Configuration(configuration);
                    }
                });
            }
          
            // 注入消息日志消息处理器
            serviceDescriptors.AddHttpClient();
            serviceDescriptors.AddHttpMessageHandlerFilter();
            
            serviceDescriptors.AddClientTokenProvider();

            //实体转换
            serviceDescriptors.AddLightweightMapper();

            serviceDescriptors.TryAddTransient<JsonHelper>();

            serviceDescriptors.TryAddTransient<HttpRequestLogScopeMiddleware>();
            serviceDescriptors.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceDescriptors.AddTransient<IAutoRetry, AutoRetry>();

            if (configuration != null)
            {
                // 默认从配置的_Clients节点获取客户端列表（以客户端名称为key，下配置clientId,clientSecret)
                serviceDescriptors.AddClientTokenProvider(configuration);
            }
           
            serviceDescriptors.AddSingleton(config);
            serviceDescriptors.AddSingleton<IStartupFilter, WebApiStartupFilter>();

            return serviceDescriptors;
        }


    }
}
