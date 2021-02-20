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

## 内置的令牌缓存管理器

## 内置的令牌设置器

## 自定义