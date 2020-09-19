using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.File.Archive;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebApiHostBuilderExtensions
    {
        internal static WebApiConfig config = new WebApiConfig();

        public static Logger InnerLogger = null;


        private static void parseConfig(IConfiguration configuration)
        {
            string consoleLog = configuration["CONSOLE_LOG"];
            string systemLoglevel = configuration["SYSTEM_LOG_LEVEL"];
            string appLoglevel = configuration["APP_LOG_LEVEL"];
            string efLogLevel = configuration["EFQUERY_LOG_LEVEL"];
            string port = configuration["APP_PORT"];
            string requestLogLevel = configuration["REQUEST_LOG_LEVEL"];
            string maxLogLenght = configuration["MAX_LOG_LENGTH"];
            string ignoreLongLog = configuration["IGNORE_LONG_LOG"];

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
            if (!String.IsNullOrWhiteSpace(ignoreLongLog))
            {
                config.IgnoreLongLog = convertConfigBoolValue(ignoreLongLog, config.IgnoreLongLog).Value;
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

                // 默认配置
                var loggerConfiguration = new LoggerConfiguration()
                        .MinimumLevel.Is(config.AppLogLevel)
                        .MinimumLevel.Override("Microsoft", config.SystemLogLevel)
                        .MinimumLevel.Override("Microsoft.AspNetCore", config.AppLogLevel)
                        .MinimumLevel.Override("Microsoft.Hosting", config.AppLogLevel)
                        .MinimumLevel.Override("System", config.SystemLogLevel)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", config.EFCoreCommandLevel)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", config.AppLogLevel);

                configureLogger?.Invoke(context, loggerConfiguration);

                if (config.Port > 0)
                {
                    builder.UseSetting("URLS", $"http://*:{config.Port}");
                }

                loggerConfiguration = loggerConfiguration.Enrich.FromLogContext();

                if (config.ConsoleLog)
                {
                    loggerConfiguration = loggerConfiguration.WriteTo.Console(config.SystemLogLevel);
                }
                if(config.FileLog)
                {
                    
                    string logPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Logs");
                    if (!System.IO.Directory.Exists(logPath))
                    {
                        System.IO.Directory.CreateDirectory(logPath);
                    }
                   // logPath = logPath.TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar +  "{Date}/logs.txt";
                    ArchiveHooks archiveHooks = new ArchiveHooks(CompressionLevel.Fastest);
                    string template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}Scopes:{NewLine}{Properties}{NewLine}{Exception}";
                    loggerConfiguration = loggerConfiguration
                        .WriteTo.Map("SourceContext", (sc, lc) => {
                            lc.File(
                                 Path.Combine(logPath, $"{DateTime.Now.ToString("yyyy-MM-dd")}/{sc}.txt"),
                                 rollingInterval: RollingInterval.Day,
                                 rollOnFileSizeLimit: true,
                                 fileSizeLimitBytes: 1024 * 1024 * 100,
                                 retainedFileCountLimit: 128,
                                 hooks: archiveHooks,
                                 outputTemplate: template);
                        });
                }

                var logger = loggerConfiguration.CreateLogger();

                Log.Logger = logger;

                // 注入消息日志消息处理器
                collection.AddHttpClient();
                collection.AddHttpMessageHandlerFilter();
                // 默认从配置的_Clients节点获取客户端列表（以客户端名称为key，下配置clientId,clientSecret)
                collection.AddClientTokenProvider(context.Configuration);

                //实体转换
                collection.AddLightweightMapper();

                collection.AddSingleton<WebApiConfig>(config);
                collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(null, true));

                collection.TryAddTransient<JsonHelper>();

                collection.TryAddTransient<HttpRequestLogScopeMiddleware>();
                collection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                collection.AddTransient<IAutoRetry, AutoRetry>();

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


                logger.Information($"初始化完成，系统日志级别：{config.SystemLogLevel}, 应用日志级别：{config.AppLogLevel}, EF Core Command日志级别：{config.EFCoreCommandLevel} 是否开启控制台日志：{config.ConsoleLog}, 监听端口：{config.Port}, 记录以下HTTP请求头：{sb.ToString()} ");


            })
            .ConfigureAppConfiguration((host, b) =>
            {
                
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
