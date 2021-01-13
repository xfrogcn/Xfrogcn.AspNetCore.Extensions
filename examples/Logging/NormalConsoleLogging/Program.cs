using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            Console.ReadLine();

        }
    }
}
