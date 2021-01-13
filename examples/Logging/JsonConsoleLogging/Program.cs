using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;

namespace JsonConsoleLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder()
                         .UseExtensions(config =>
                         {
                             config.ConsoleJsonLog = true;
                             config.FileLog = false;
                             config.AppLogLevel = Serilog.Events.LogEventLevel.Verbose;
                         })
                         .Build();

            _ = host.StartAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("测试日志：{time}", DateTimeOffset.Now);
            // 使用@前缀将对象合并大日志JSON对象中，如果不带@前缀，则以字符串方式进行处理
            logger.LogInformation("obj: {@obj}", new Point { X = 0, Y = 1 });

            Console.ReadLine();
        }
    }
}
