# 请求模拟

在微服务架构下，服务之间相互调用，这对测试和调试带来了挑战，请求模拟功能，可以让你专注于你自己的服务，屏蔽外部服务的影响。可灵活配置依赖服务的应答来对本地服务进行测试或调试。

请求模拟功能与`IHttpClientFactory`模式结合，可为指定名称的HttpClient提供模拟应答的配置。

## 示例

以下示例针对于默认名称("")的HttpClient配置了两条规则：

- Url为"/api/test1"的Get请求：返回字符串"Hello"
- 对于请求头中包含"x-test"头的请求：返回字符串"TEST"

```c#
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
```

详细示例请参考：examples/Http/MockHttpRequest项目