using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SDKExample.SDK;
using System;
using System.Threading.Tasks;

namespace SDKExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WebApplicationFactory<ApiServer.Startup> server = new WebApplicationFactory<ApiServer.Startup>();

            IServiceCollection sc = new ServiceCollection()
               .AddExtensions(null, config =>
               {
                   config.FileLog = false;
                   config.ConsoleLog = true;
               });


            sc.AddTestSDK("http://localhost", "TEST_SDK", "test", "test");

            // 此处是演示代码，设置HttpClient请求模拟的服务端，正式使用无需此设置
            sc.AddHttpClient("TEST_SDK")
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return server.Server.CreateHandler();
                });

            var sp = sc.BuildServiceProvider();

            var client =  sp.GetRequiredService<TestApiClient>();

            string response = await client.Test();

            Console.WriteLine($"应答：{response}");

            Console.ReadLine();
        }
    }
}
