using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Extensions.Tests
{
    [Trait("", "Logger")]
    public class LoggerTest
    {
        [Fact(DisplayName = "动态修改日志级别")]
        public void Test1()
        {
            Dictionary<string, string> configDic = new Dictionary<string, string>()
            {
                { "AppLogLevel", "Error" }
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDic)
                .Build();

            IServiceCollection services = new ServiceCollection()
                .AddExtensions(config)
                .AddLogging(logBuilder =>
                {
                    logBuilder.AddTestLogger();
                });



            IServiceProvider provider = services.BuildServiceProvider();

            // 配置监听与HostService关联，需要实例化
            var svcList = provider.GetService<IEnumerable<IHostedService>>();


            var logger = provider.GetService<ILogger<LoggerTest>>();
            logger.LogError("ERROR 1");
            logger.LogInformation("INFO 1");

            var logContent = provider.GetTestLogContent();
            Assert.Single(logContent.LogContents);

            //修改级别
            config["AppLogLevel"] = "Debug";
            config.Reload();


            logger.LogTrace("Trace 1");
            logger.LogDebug("Debug 1");
            logger.LogError("ERROR 3");

            Assert.Equal(3, logContent.LogContents.Count);

        }

        [Fact(DisplayName = "自定义配置覆盖")]
        public void Test2()
        {
            Dictionary<string, string> configDic = new Dictionary<string, string>()
            {
                { "AppLogLevel", "Information" }
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDic)
                .Build();

            IServiceCollection services = new ServiceCollection()
                .AddExtensions(config, configAction=>
                {
                    configAction.AppLogLevel = Serilog.Events.LogEventLevel.Error;
                })
                .AddLogging(logBuilder =>
                {
                    logBuilder.AddTestLogger();
                });



            IServiceProvider provider = services.BuildServiceProvider();

            // 配置监听与HostService关联，需要实例化
            var svcList = provider.GetService<IEnumerable<IHostedService>>();


            var logger = provider.GetService<ILogger<LoggerTest>>();
            logger.LogError("ERROR 1");
            logger.LogInformation("INFO 1");

            var logContent = provider.GetTestLogContent();
            Assert.Single(logContent.LogContents);

            //修改级别, 由于有自定义配置覆盖，所以级别不生效
            config["AppLogLevel"] = "Debug";
            config.Reload();


            logger.LogTrace("Trace 1");
            logger.LogDebug("Debug 1");
            logger.LogError("ERROR 3");

            Assert.Equal(2, logContent.LogContents.Count);

        }


    }
}
