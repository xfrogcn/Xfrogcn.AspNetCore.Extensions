using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HttpHeaderTracking
{
    class ServiceAFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        readonly WebApplicationFactory<TEntryPoint> _factoryB;
        public ServiceAFactory(WebApplicationFactory<TEntryPoint> factoryB)
        {
            _factoryB = factoryB;
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
            })
            .ConfigureServices(services=>
            {
                // 配置ServiceA所使用的HttpClient
                services.AddHttpClient("", client =>
                {
                     client.BaseAddress = new Uri("http://localhost");
                 })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return _factoryB.Server.CreateHandler();
                });
            });
            
        }
    }
}
