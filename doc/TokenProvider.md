# 认证令牌处理

在真实项目中，服务之间的交互式常态，特别是在微服务体系下，而服务之间的交互常常需要提供认证信息（如基础认证、Token认证等），认证过程通常包括获取令牌、令牌缓存、令牌的失效判断、令牌的重新获取等，如果这些处理都由每个业务系统自己处理，是非常繁琐的。

## 认证提供器

认证令牌通过认证提供器进行统一管理, 认证提供器包含以下组件，他们之间相互协同来实现令牌的自动管理：

- 客户端信息，用于记录认证客户端信息（如clientId，client密钥等）
- 认证管理器，用于管理特定客户端的认证过程
- 认证处理器，用于获取令牌
- 令牌设置器，用于将令牌设置到Http请求中
- 令牌缓存管理器，用于管理令牌缓存
- 应答检查器，用于检查Http应答是否为未授权
- 认证提供器，用于获取指定客户端的认证管理器
- 认证设置，用于配置指定客户端的认证信息

## 配置

1. 启用认证提供器

如果通过了UseExtensions或者AddExtensions方式启用了扩展库功能，默认已经开启认证提供器，如果想单独使用认证提供器，可使用IServiceCollection的AddClientTokenProvider扩展方法：

```c#
    IServiceCollection sc = new ServiceCollection()
        .AddClientTokenProvider();
```

2. 配置客户端

有两种方式配置认证客户端信息，一是与AddClientTokenProvider集合，传入认证配置委托：

```c#
 sc.AddClientTokenProvider(options =>
        {
            options.AddClient("", "TestClient", "")
                // 设置此客户端使用Basic认证
                .UseBasicAuth("test", "test");
        });
```

二是，直接通过AddTokenClient方法：

```c#
    serviceDescriptors.AddTokenClient(url, clientId, "", options=>
    {
        options.UseBasicAuth(userName, password);
    });
```

## 内置的认证方式

令牌提供器内置了两种认证方式：

- 基本认证：UseBasicAuth
- OIDC认证：UseOIDCAuth

## 内置的认证处理器

扩展库提供了OIDC认证处理器，通过SetProcessor方法可进行配置：

```c#
 sc.AddClientTokenProvider(options =>
        {
            options.AddClient("", "TestClient", "")
                .SetProcessor(CertificateProcessor.OIDC);
        });
```

## 内置的令牌缓存管理器

内部提供两种缓存管理器：

- 本地缓存: 令牌信息缓存到本机
- 分布式缓存：通过注入的IDistributedCache来缓存令牌

可以通过SetTokenCacheManager方法来指定缓存管理器工厂：

```c#
 sc.AddClientTokenProvider(options =>
        {
            options.AddClient("", "TestClient", "")
                // 本地缓存
                // .SetTokenCacheManager( TokenCacheManager.MemoryCacheFactory )
                // 分布式缓存
                .SetTokenCacheManager(TokenCacheManager.DistributedCacheFactory);
        });
```

## 内置的令牌设置器

扩展库提供两种令牌设置器：

- Bearer：将令牌设置到认证请求头
- QueryString：将令牌附加到请求的查询字符串中

可通过SetTokenSetter进行配置：

```c#
 sc.AddClientTokenProvider(options =>
        {
            options.AddClient("", "TestClient", "")
                // .SetTokenSetter(SetTokenProcessor.Bearer)
                .SetTokenSetter(SetTokenProcessor.QueryString);
        });
```

## 自定义

如果内置的认证方式或组件无法满足需求，可替换实现相应的自定义组件

1. 自定义认证处理器

    自定义认证处理器支持两种方式：

   - 实现一个CertificateProcessor的子类
   - 支持配置一个Func&lt;ClientCertificateInfo, HttpClient, Task&lt;ClientCertificateToken&gt;&gt;处理委托, 该委托传入认证客户端信息、请求Token所使用的HttpClient，需要委托返回令牌信息

2. 自定义令牌缓存管理器

    要自定义令牌缓存管理器，需要实现自己的TokenCacheManager，然后通过SetTokenCacheManager来设置缓存管理器的创建委托。

3. 自定义令牌设置器

    自定义令牌设置器支持两种方式：

   - 实现一个SetTokenProcessor的子类
   - 支持配置一个Func&lt;HttpRequestMessage, string, Task&gt;处理委托, 该委托传入当前请求消息以及令牌字符串

4. 自定义应答检查器

    应答检查器用于从Http应答中判断令牌是否失效，要实现自己的应答检查器，只需实现CheckResponseProcessor的子类，然后通过SetResponseChecker来配置
