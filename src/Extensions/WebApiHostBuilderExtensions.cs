using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Text;
using Xfrogcn.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebApiHostBuilderExtensions
    {
        internal static WebApiConfig config = new WebApiConfig();

        public static Logger InnerLogger = null;


        private static void parseConfig(IConfiguration configuration)
        {
            configuration.Bind(config);

            string consoleLog = configuration["CONSOLE_LOG"];
            string systemLoglevel = configuration["SYSTEM_LOG_LEVEL"];
            string appLoglevel = configuration["APP_LOG_LEVEL"];
            string efLogLevel = configuration["EFQUERY_LOG_LEVEL"];
            string port = configuration["APP_PORT"];
            string requestLogLevel = configuration["REQUEST_LOG_LEVEL"];
            string maxLogLenght = configuration["MAX_LOG_LENGTH"];
          
            if (!String.IsNullOrWhiteSpace(appLoglevel))
            {
                LogEventLevel appLevel = LogEventLevel.Debug;
                if (Enum.TryParse<LogEventLevel>(appLoglevel, out appLevel))
                {
                    config.AppLogLevel = appLevel;
                }
            }
            if (!String.IsNullOrWhiteSpace(systemLoglevel))
            {
                LogEventLevel sysLevel = LogEventLevel.Warning;
                if (Enum.TryParse<LogEventLevel>(systemLoglevel, out sysLevel))
                {
                    config.SystemLogLevel = sysLevel;
                }
            }
            if (!String.IsNullOrWhiteSpace(efLogLevel))
            {
                LogEventLevel efLevel = LogEventLevel.Debug;
                if (Enum.TryParse<LogEventLevel>(efLogLevel, out efLevel))
                {
                    config.EFCoreCommandLevel = efLevel;
                }
            }
            if (!String.IsNullOrEmpty(requestLogLevel))
            {
                LogEventLevel rLevel = LogEventLevel.Verbose;
                if (Enum.TryParse<LogEventLevel>(efLogLevel, out rLevel))
                {
                    config.RequestLogLevel = rLevel;
                }
                else
                {
                    config.RequestLogLevel = null;
                }
            }
            if (!String.IsNullOrEmpty(maxLogLenght))
            {
                int ml = 0;
                if (int.TryParse(maxLogLenght, out ml) && ml > 0)
                {
                    config.MaxLogLength = ml;
                }
            }
           

            config.ConsoleLog = convertConfigBoolValue(consoleLog, config.ConsoleLog).Value;


            if (!String.IsNullOrEmpty(port))
            {
                int p = 0;
                if (int.TryParse(port, out p))
                {
                    config.Port = p;
                }
            }


        }

        public static IWebHostBuilder UseExtensions(this IWebHostBuilder builder, string[] args, Action<WebApiConfig> configAction = null, Action<WebHostBuilderContext, LoggerConfiguration> configureLogger = null)
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
                parseConfig(context.Configuration);
                configAction?.Invoke(config);

                if (config.Port > 0)
                {
                    builder.UseSetting("URLS", $"http://*:{config.Port}");
                }

                if (config.EnableSerilog)
                {
                    collection.AddDefaultSerilog(config, (logConfig) =>
                    {
                        configureLogger?.Invoke(context, logConfig);
                        logConfig.ReadFrom.Configuration(context.Configuration);
                    });
                }

                
                collection.AddExtensions();
                // 默认从配置的_Clients节点获取客户端列表（以客户端名称为key，下配置clientId,clientSecret)
                collection.AddClientTokenProvider(context.Configuration);
                collection.AddSingleton<WebApiConfig>(config);
                collection.AddSingleton<IStartupFilter, WebApiStartupFilter>();

                StringBuilder sb = new StringBuilder();
                foreach (string h in config.HttpHeaders)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(h);
                }

                InnerLogger.Information($"初始化完成，系统日志级别：{config.SystemLogLevel}, 应用日志级别：{config.AppLogLevel}, EF Core Command日志级别：{config.EFCoreCommandLevel} 是否开启控制台日志：{config.ConsoleLog}, 监听端口：{config.Port},日志保留天数：{config.MaxLogDays} 记录以下HTTP请求头：{sb.ToString()} ");

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

        private static bool? convertConfigBoolValue(string val, bool? defValue)
        {
            bool? boolVal = defValue;
            if (val == "on" || val == "1" || val == "yes" || val == "true")
            {
                boolVal = true;
            }
            else if (val == "no" || val == "0" || val == "off" || val == "false")
            {
                boolVal = false;
            }

            return boolVal;
        }
    }
}
