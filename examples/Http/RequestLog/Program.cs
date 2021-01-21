using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RequestLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 集成测试服务端
            TestWebApplicationFactory<ApiServer.Startup> factory = new TestWebApplicationFactory<ApiServer.Startup>();


            IServiceCollection sc = new ServiceCollection()
                .AddExtensions(null, config =>
                {
                    // 同时设置EnableClientRequestLog为true及ClientRequestLevel为Verbose，可记录客户端请求详情
                    config.EnableClientRequestLog = true;
                    config.ClientRequestLevel = Serilog.Events.LogEventLevel.Verbose;
                    config.FileLog = false;
                    config.ConsoleLog = true;
                });

            // 此处配置HttpClient使用集成测试服务端的消息处理器
            // 注意，此示例不使用factory.CreateClient来创建客户端，因为此方法不是通过HttpFactory方式创建的
            // 故无法注入扩展功能
            sc.AddHttpClient("", client=>
            {
                client.BaseAddress = new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return factory.Server.CreateHandler();
            });
            IServiceProvider sp = sc.BuildServiceProvider();

            IHttpClientFactory httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("");
            var response =await client.GetAsync<List<ApiServer.WeatherForecast>>("/WeatherForecast");


            Console.ReadLine();
        }
    }
}
