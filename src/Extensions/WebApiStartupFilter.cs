using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Xfrogcn.AspNetCore.Extensions
{
    class WebApiStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            Action<IApplicationBuilder> builder = (b) =>
            {
                b.UseMiddleware<HttpRequestLogScopeMiddleware>();
                next(b);
            };

            return builder;
        }
    }
}
