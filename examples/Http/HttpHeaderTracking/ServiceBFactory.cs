using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpHeaderTracking
{
    class ServiceBFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
       
        public ServiceBFactory()
        {
           
        }


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseExtensions(null, config =>
            {
                // 将服务端请求日志层级设置为Verbose可记录服务端请求详细日志
                config.FileLog = false;
                config.ConsoleLog = true;
                config.AppLogLevel = Serilog.Events.LogEventLevel.Warning;
            });

        }
    }
}
