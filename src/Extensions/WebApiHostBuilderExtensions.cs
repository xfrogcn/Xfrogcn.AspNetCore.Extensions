using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Text;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebApiHostBuilderExtensions
    {
        public static Logger InnerLogger = null;


        public static IWebHostBuilder UseExtensions(this IWebHostBuilder builder, string[] args, Action<WebApiConfig> configAction = null, Action<LoggerConfiguration> configureLogger = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // 先初始化一个日志，以便在其他配置中可以先行使用
            var tempLogger = new LoggerConfiguration()
                    .MinimumLevel.Is(LogEventLevel.Verbose);
            tempLogger = tempLogger.WriteTo.Console();
            var _logger = tempLogger.CreateLogger();
            InnerLogger = _logger;

            

            // 注意 Host的初始化流程 先以此执行 ConfigureAppConfiguration， 最后执行 ConfigureServices
            builder = builder.ConfigureServices((context, collection) =>
            {
                collection.AddExtensions(context.Configuration, configAction, configureLogger);

                var config = ServiceCollectionExtensions.config;
                StringBuilder sb = new StringBuilder();
                foreach (string h in config.HttpHeaders)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(h);
                }

                InnerLogger.Information($"初始化完成：\n" +
                    $"\t系统日志级别：{config.SystemLogLevel}\n" +
                    $"\t应用日志级别：{config.AppLogLevel}\n" +
                    $"\tEF Core Command日志级别：{config.EFCoreCommandLevel}\n" +
                    $"\t是否开启控制台日志：{config.ConsoleLog}\n" +
                    $"\t服务端请求日志记录级别：{config.ServerRequestLevel}\n" +
                    $"\t客户端请求日志级别：{config.ClientRequestLevel}\n" +
                    $"\t是否开启客户端请求日志：{config.EnableClientRequestLog}\n" +
                    $"\t日志保留天数：{config.MaxLogDays}\n" +
                    $"\t记录以下HTTP请求头：{sb.ToString()} ");

            });
           


            return builder;
        }

            
            

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject != null && e.ExceptionObject is Exception)
            {
                Log.Logger.Fatal(e.ExceptionObject as Exception, "系统异常");
            }
            else
            {
                Log.Logger.Fatal($"系统异常");
            }
        }

    }
}
