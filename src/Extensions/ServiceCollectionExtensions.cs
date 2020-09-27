using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExtensions(this IServiceCollection serviceDescriptors)
        {
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


            return serviceDescriptors;
        }
    }
}
