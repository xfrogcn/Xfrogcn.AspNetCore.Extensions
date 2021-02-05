# 日志

Xfrogcn.AspNetCore.Extensions中日志是对Serilog日志库易用性的进一步封装，通过简单配置来实现常见的日志记录需求。

日志通过全局的WebApiConfig进行配置。

## 日志配置项

| 配置项 |      说明     | 默认值 |
| ----- | -------------- | ---- |
| EnableSerilog | 是否启用Serilog | true |
| SystemLogLevel | 系统日志级别, 系统日志是指日志源以Microsoft或System开始的日志 | Warning |
| EFCoreCommandLevel | EFCore日志级别，EFCore日志是指日志来源为Microsoft.EntityFrameworkCore.Database.Command的日志 | Information |
| AppLogLevel | 应用日志级别，应用日志指来源为Microsoft.AspNetCore、Microsoft.Hosting及其他来源的日志 | Information |
| ServerRequestLevel | 服务端请求记录日志级别，设置为Verbose可记录服务端请求及应答详情 | Information |
| ClientRequestLevel | 客户端请求记录日志级别 | Information |
| ConsoleLog | 是否开启控制台日志 | false |
| ConsoleJsonLog | 控制台日志是否使用Json日志格式 | false |
| FileLog | 是否开启本地文件日志 | true |
| FileJsonLog | 本地文件日志是否使用Json日志格式 | false |
| MaxLogLength | 单条日志的最大长度 | 8kb |
| LogPathTemplate | 日志路径模板 | LogPathTemplates.DayFolderAndLoggerNameFile | LogPath | 文件日志保存位置 | "Logs" |
| MaxLogFileSize | 最大单个日志文件大小，超过后将写到新的日志 | 100mb |
| RetainedFileCount | 旋转日志的文件数，超过此设置将被自动压缩 | 31 |
| LogTemplate | 日志模板 | "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{NewLine}{Exception}" |
| MaxLogDays | 本地文件日志最大保留天数, 如果设置为0，不做自动清理 | 0 |
| IgnoreLongLog | 是否忽略长日志（即超过MaxLogLength设置的日志） | false |

## 如何使用

```c#
    using IHost host = Host.CreateDefaultBuilder()
        .UseExtensions(config =>
        {
            // 在此处配置日志选项
            // config.FileLog = true;
            config.AppLogLevel = Serilog.Events.LogEventLevel.Verbose;
        })
        .Build();
```

## 关于长日志

对长日志的控制，仅针对于Json格式，通过MaxLogLength及IgnoreLongLog配置来设置对长日志的处理，正常情况，超过MaxLogLength设置的日志将被截取到MaxLogLength长度，但如果IgnoreLongLog设置为false，则超过MaxLogLength长度的日志将同时进行拆分记录为多条日志。

非Json格式日志不受长度设置影响。

## 关于本地文件日志记录路径

本地文件日志路径通过LogPathTemplate设置来配置，默认为LogPathTemplates.DayFolderAndLoggerNameFile，表示以每天作为子目录，以日志名称作为日志文件名。通过LogPathTemplates也内置了其他的路径模板：

|  路径模板名 | 说明 |
| ---------- | ---- |
| DayFolderAndLoggerNameFile | 以每天日期为目录，日志名称为文件名 |
| DayFile | 以每天日期为日志名称 |
| LoggerNameAndDayFile | 以[日志名称_每天日志]为日志文件名称 |
| LevelFile | 以日志级别缩写为日志文件名称 |
| DayFolderAndLevelFile | 以每天日期为目录，日志级别缩写为日志名称 |

由于LogPathTemplate未字符串配置，你也可以配置其他的路径模板。
