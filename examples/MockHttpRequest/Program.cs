using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockHttpRequest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection sc = new ServiceCollection()
                .AddExtensions();

                sc.AddHttpClient("", client =>
                {
                    client.BaseAddress = new Uri("http://localhost");
                })
                .AddMockHttpMessageHandler()
                // 请求/api/test1 返回Hello, url可以使用*,便是所有url
                .AddMock<string>("/api/test1", HttpMethod.Get, "Hello")
                // 通过请求判断
                .AddMock(request =>
                {
                    // 包含x-test头时，Mock
                    if (request.Headers.GetValues("x-test") != null)
                    {
                        return true;
                    }
                    return false;
                }, async (request, response) =>
                {
                    await response.WriteObjectAsync("TEST");
                });

            var sp = sc.BuildServiceProvider();

            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            // 注意名称需要与配置时匹配，此处为""
            var client = httpFactory.CreateClient("");

            string response = await client.GetAsync<string>("/api/test1");
            Console.WriteLine(response);

            response = await client.GetAsync<string>("api/test2", null, new System.Collections.Specialized.NameValueCollection()
            {
                {"x-test", "" }
            });
            Console.WriteLine(response);

            Console.ReadLine();
        }
    }
}
