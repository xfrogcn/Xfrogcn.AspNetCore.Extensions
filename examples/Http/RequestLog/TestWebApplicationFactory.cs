using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace RequestLog
{
    class TestWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint: class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder = builder.UseExtensions(null, config =>
            {
                // 将服务端请求日志层级设置为Verbose可记录服务端请求详细日志
                config.ServerRequestLevel = Serilog.Events.LogEventLevel.Verbose;
                config.AppLogLevel = Serilog.Events.LogEventLevel.Debug;
                config.FileLog = false;
                config.ConsoleLog = true;

            });
            base.ConfigureWebHost(builder);
               
        }

    }
}
