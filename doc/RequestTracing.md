# 请求跟踪

扩展库可以配置HttpClient自动传递指定的Http请求头来辅助完成请求跟踪功能。

要通过HttpClient自动向下传递请求头，只需通过WebApiConfig的TrackingHeaders属性配置需要传递的请求头，默认配置为x-*，表示传递所有以x-为前缀的请求头。

```c#
    IServiceCollection sc = new ServiceCollection()
        .AddExtensions(null, config =>
        {
            // 以下配置传递以my-为前缀的请求头
            config.TrackingHeaders.Add("my-*");
        });
```

有关请求头传递的示例，请参考`examples/Http/HttpHeaderTracking`