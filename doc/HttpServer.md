# Http服务端扩展

## 请求详细记录

通过将配置的ServerRequestLevel设置为Verbose，可记录每一个请求及应答的详细内容。

```c#
builder.UseExtensions(null, config =>
        {
            // 将服务端请求日志层级设置为Verbose可记录服务端请求详细日志
            config.ServerRequestLevel = Serilog.Events.LogEventLevel.Verbose;
        });
```

## 开启请求Buffer

默认情况下，AspNetCore中的请求内容只可读取一次，如果你需要多次读取，可通过`EnableBufferingAttribute`在Action或Controller上开启请求缓冲。
