# Http认证令牌处理

扩展库通过令牌提供器，并与HttpClient集合，实现了HttpClient请求认证的自动化处理，大大简化了认证的过程。

有关令牌提供器的详细信息，请参见[令牌提供器](./TokenProvider.md)

要使用Http认证令牌处理，可分为两个步骤：

- 通过令牌提供器配置认证信息
- 配置对应名称的HttpClient使用相应的认证配置

HttpClient与令牌提供器之间通过指定的ClientID关联

## 示例

以下配置ClientID为TestClient的客户端使用基本认证（用户名为test，密码为test），并设置默认HttpClient（名称为""）自动使用此认证信息管理认证过程。

```c#
    // 1. 首先，需要加入客户端信息，每个客户端必须有唯一的ID
    sc.AddClientTokenProvider(options =>
    {
        options.AddClient("", "TestClient", "")
            // 设置此客户端使用Basic认证
            .UseBasicAuth("test", "test");
    });
    // 2. 设置HttpClient关联认证管理器
    sc.AddHttpClient("", client =>
    {
        client.BaseAddress = new Uri("http://localhost");
    })
    // 设置此客户端关联TestClient的认证配置
    .AddTokenMessageHandler("TestClient");


    // 3. 使用
    IServiceProvider sp = sc.BuildServiceProvider();

    IHttpClientFactory httpFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = httpFactory.CreateClient("");
    string response = await client.GetAsync<string>("/limit");
```
