using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
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
                .AddExtensions(config, configAction =>
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

        [Fact(DisplayName = "MapCondition-MapCountLimit")]
        public void Test3()
        {
            LoggerConfiguration config = new Serilog.LoggerConfiguration();

            List<string> _logContents = new List<string>();
            int disposeCount = 0;
            Action callback = () =>
            {
                disposeCount++;
            };

            config.WriteTo.MapCondition<string>((logEvent) =>
            {
                return (logEvent.Properties.GetValueOrDefault("Name") as ScalarValue).Value.ToString();
            }, (key, logConfig) =>
            {
                logConfig.Sink(new TestSink(_logContents, callback));
            }, null, TimeSpan.FromSeconds(0), 1);

            var logger = config.CreateLogger();

            var testLogger1 = logger.ForContext("Name", "Test1");
            testLogger1.Information("A");

            var testLogger2 = logger.ForContext("Name", "Test2");
            testLogger2.Information("A");

            Assert.Equal(1, disposeCount);

        }

        [Fact(DisplayName = "MapCondition-DisposeCondition")]
        public void Test4()
        {
            LoggerConfiguration config = new Serilog.LoggerConfiguration();

            List<string> _logContents = new List<string>();
            int disposeCount = 0;
            Action callback = () =>
            {
                disposeCount++;
            };

            config.WriteTo.MapCondition<string>((logEvent) =>
            {
                return (logEvent.Properties.GetValueOrDefault("Name") as ScalarValue).Value.ToString();
            }, (key, logConfig) =>
            {
                logConfig.Sink(new TestSink(_logContents, callback));
            }, key => true, TimeSpan.FromSeconds(0));

            var logger = config.CreateLogger();

            var testLogger1 = logger.ForContext("Name", "Test1");
            testLogger1.Information("A");

            var testLogger2 = logger.ForContext("Name", "Test2");
            testLogger2.Information("A");

            testLogger1.Information("B");

            Assert.Equal(3, disposeCount);

        }

        [Fact(DisplayName = "MapCondition-CustomerKey")]
        public void Test5()
        {
            LoggerConfiguration config = new Serilog.LoggerConfiguration();

            List<string> _logContents = new List<string>();
            int disposeCount = 0;
            Action callback = () =>
            {
                disposeCount++;
            };

            config.WriteTo.MapCondition<TestSinkKey>((logEvent) =>
            {
                TestSinkKey key = new TestSinkKey()
                {
                    Name = (logEvent.Properties.GetValueOrDefault("Name") as ScalarValue).Value.ToString(),
                    Time = logEvent.Timestamp
                };
                return key;
            }, (key, logConfig) =>
            {
                logConfig.Sink(new TestSink(_logContents, callback));
            }, key =>
            {
                return key.Name == "Test1";
            }, TimeSpan.FromSeconds(0));

            var logger = config.CreateLogger();

            var testLogger1 = logger.ForContext("Name", "Test1");
            testLogger1.Information("A");

            var testLogger2 = logger.ForContext("Name", "Test2");
            testLogger2.Information("A");

            testLogger1.Information("B");

            testLogger2.Information("A");

            Assert.Equal(2, disposeCount);

        }
    }





    class TestSink : ILogEventSink, IDisposable
    {
        readonly List<string> _contents;
        readonly Action _disposeCallback;

        public TestSink(List<string> contents, Action disposeCallback)
        {
            _contents = contents;
            _disposeCallback = disposeCallback;
        }

        public void Dispose()
        {
            _disposeCallback();
        }

        public void Emit(LogEvent logEvent)
        {
            _contents.Add(logEvent.RenderMessage());
        }
    }

    class TestSinkKey : IEqualityComparer<TestSinkKey>
    {
        public string Name { get; set; }

        public DateTimeOffset Time { get; set; }

        public bool Equals([AllowNull] TestSinkKey x, [AllowNull] TestSinkKey y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] TestSinkKey obj)
        {
            return (obj.Name ?? "").GetHashCode();
        }
    }
}
