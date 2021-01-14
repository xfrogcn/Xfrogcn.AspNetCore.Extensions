using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xfrogcn.AspNetCore.Extensions;

namespace NormalConsoleLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder()
                .UseExtensions(config =>
                {
                    config.ConsoleLog = true;
                    config.FileLog = false;
                    config.AppLogLevel = Serilog.Events.LogEventLevel.Verbose;
                })
                .Build();

            _ = host.StartAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("测试日志：{time}", DateTimeOffset.Now);

            // 动态修改日志级别
            var apiConfig = host.Services.GetRequiredService<WebApiConfig>();
            apiConfig.AppLogLevel = Serilog.Events.LogEventLevel.Error;
            // 此日志被忽略
            logger.LogInformation("测试日志：{time}", DateTimeOffset.Now);
            apiConfig.AppLogLevel = Serilog.Events.LogEventLevel.Verbose;

            // 日志显示
            logger.LogInformation("测试日志：{time}", DateTimeOffset.Now);


            Console.ReadLine();

        }
    }
}
