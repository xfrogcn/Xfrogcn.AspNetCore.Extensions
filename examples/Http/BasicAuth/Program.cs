using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BasicAuth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 使用Basic认证方式，通过配置后，发送的请求自动处理请求认证信息，应用无需单独处理
            // 以下示例/limit 路径受限，需要通过Basic认证（用户名：test,密码: test)，接口返回用户名
            WebApplicationFactory<ApiServer.Startup> server = new WebApplicationFactory<ApiServer.Startup>();

            IServiceCollection sc = new ServiceCollection()
                .AddExtensions(null, config =>
                {
                    config.FileLog = false;
                    config.ConsoleLog = true;
                });

            // 1. 首先，需要加入客户端信息，每个客户端必须有唯一的ID
            sc.AddClientTokenProvider(options =>
            {
                options.AddClient("", "TestClient", "")
                    // 设置此客户端使用Basic认证
                    .UseBasicAuth("test", "test");
            });

            // 此处配置HttpClient使用集成测试服务端的消息处理器
            // 注意，此示例不使用factory.CreateClient来创建客户端，因为此方法不是通过HttpFactory方式创建的
            // 故无法注入扩展功能
            sc.AddHttpClient("", client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return server.Server.CreateHandler();
            })
            // 设置此客户端关联TestClient的认证配置
            .AddTokenMessageHandler("TestClient");
            IServiceProvider sp = sc.BuildServiceProvider();

            IHttpClientFactory httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("");
            string response = await client.GetAsync<string>("/limit");

            Console.WriteLine(response);

            Console.ReadLine();
            
        }
    }
}
