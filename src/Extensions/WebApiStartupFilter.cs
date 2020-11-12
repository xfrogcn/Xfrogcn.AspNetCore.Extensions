using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Xfrogcn.AspNetCore.Extensions
{
    class WebApiStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            Action<IApplicationBuilder> builder = (b) =>
            {
                b.UseMiddleware<HttpRequestLogScopeMiddleware>();
                // 此处实例化WebApiStartFilter
                var monitor = b.ApplicationServices.GetRequiredService<WebApiConfigMonitor>();
                monitor.Init();
                next(b);
            };

            return builder;
        }
    }
}
