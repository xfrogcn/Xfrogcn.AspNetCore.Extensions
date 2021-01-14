using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using Xfrogcn.AspNetCore.Extensions;

namespace JsonFileLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder()
                         .UseExtensions(config =>
                         {
                             config.FileJsonLog = true;
                             config.AppLogLevel = Serilog.Events.LogEventLevel.Verbose;

                             // 指定日志存放目录
                             // config.LogPath = "Logs";
                             //指定日志文件名称模板，默认为日期为目录，日志名称为文件名称
                             //config.LogPathTemplate = LogPathTemplates.DayFolderAndLevelFile;
                             // 通过EnableSerilog来设置是否使用Serilog日志框架
                             //config.EnableSerilog = false;
                             // 设置文件的最大尺寸，超过此尺寸后将产生新的日志序列文件
                             config.MaxLogFileSize = 1024;
                             // 单个日志文件拆分的最大数量，超过此数量后日志将被自动压缩
                             // config.RetainedFileCount = 1;
                             // 日志最长保留天数，超过此设置后的日志将被自动清理
                             //config.MaxLogDays = 7;
                             // 单条日志内容的最大长度，超过后将被拆分或忽略超长部分（取决于IgnoreLongLog）
                             // config.MaxLogLength = 1024 * 8;
                         })
                         .Build();

            _ = host.StartAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("测试日志：{time}", DateTimeOffset.Now);
            // 使用@前缀将对象合并大日志JSON对象中，如果不带@前缀，则以字符串方式进行处理
            logger.LogInformation("obj: {@obj}", new Point { X = 0, Y = 1 });
            for(int i = 0; i < 10; i++)
            {
                logger.LogInformation(new string('A', 1024));
            }

            Console.ReadLine();
        }
    }
}
