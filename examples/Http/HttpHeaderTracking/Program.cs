using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpHeaderTracking
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 以下模拟 客户端-->服务A-->服务B 的调用链路，客户端传入头x-request-id，将自动传递到服务B
            // 集成测试服务端
            ServiceBFactory<ApiServer.Startup> factoryB = new ServiceBFactory<ApiServer.Startup>();
            ServiceAFactory<ApiServer.Startup> factoryA = new ServiceAFactory<ApiServer.Startup>(factoryB);


            IServiceCollection sc = new ServiceCollection()
                .AddExtensions(null, config =>
                {
                    // 同时设置EnableClientRequestLog为true及ClientRequestLevel为Verbose，可记录客户端请求详情
                    config.FileLog = false;
                    config.ConsoleLog = true;
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
                return factoryA.Server.CreateHandler();
            });
            IServiceProvider sp = sc.BuildServiceProvider();

            IHttpClientFactory httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("");
            // 调用服务A
            var response = await client.GetAsync<string>("/tracking/serviceA", null, new System.Collections.Specialized.NameValueCollection()
            {
                { "x-request-id", "test-request-id" }
            });

            Console.WriteLine(response);

            Console.ReadLine();

        }
    }
}
