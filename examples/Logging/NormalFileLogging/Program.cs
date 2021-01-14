using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace NormalFileLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder()
                .UseExtensions(config =>
                {
                    // 默认开启
                    // config.FileLog = true;
                    config.AppLogLevel = Serilog.Events.LogEventLevel.Verbose;
                })
                .Build();

            _ = host.StartAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            // 放入 ./Logs/[YYYY-MM-DD]/Program.log中
            logger.LogInformation("测试日志：{time}", DateTimeOffset.Now);

            Console.ReadLine();
        }
    }
}
