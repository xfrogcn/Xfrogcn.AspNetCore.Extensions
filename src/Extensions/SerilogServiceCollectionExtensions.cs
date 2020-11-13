using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.File.Archive;

namespace Xfrogcn.AspNetCore.Extensions
{
    public static class SerilogServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSerilog(this IServiceCollection serviceDescriptors, WebApiConfig apiConfig=null, Action<LoggerConfiguration> configureLogger = null, bool preserveStaticLogger = false, bool writeToProviders = true)
        {
            var loggerConfiguration = new LoggerConfiguration();

            LoggerProviderCollection loggerProviders = null;
            if (writeToProviders)
            {
                loggerProviders = new LoggerProviderCollection();

                loggerConfiguration.WriteTo.Providers(loggerProviders);
            }

            apiConfig = apiConfig ?? new WebApiConfig();
            configFromWebApiConfig(loggerConfiguration, apiConfig); 

            configureLogger?.Invoke(loggerConfiguration);
            var logger = loggerConfiguration.CreateLogger();

            Serilog.ILogger registeredLogger = null;
            if (preserveStaticLogger)
            {
                registeredLogger = logger;
            }
            else
            {
                Log.Logger = logger;
            }

            // 去除内置的LoggerProvider
            var removeProviders = serviceDescriptors.Where(x => x.ServiceType == typeof( ILoggerProvider)).ToList();
            removeProviders.ForEach(p =>
            {
                serviceDescriptors.Remove(p);
            });


            serviceDescriptors.AddSingleton<ILoggerFactory>(services =>
            {
                var factory = new SerilogLoggerFactory(registeredLogger, true, loggerProviders);

                if (writeToProviders)
                {
                    foreach (var provider in services.GetServices<ILoggerProvider>())
                        factory.AddProvider(provider);
                }

                return factory;
            });

            // 日志定时清理托管服务
            serviceDescriptors.AddHostedService<ClearLogsHostService>();

            return serviceDescriptors;
        }

        internal static string GetLogPath(this WebApiConfig apiConfig)
        {
            string path = apiConfig.LogPath;
            if (string.IsNullOrEmpty(path))
            {
                path = "Logs";
            }

            if (!Path.IsPathRooted(path))
            {
                path = System.IO.Path.Combine(AppContext.BaseDirectory, path);
            }
            return path;
        }


        private static LoggerConfiguration configFromWebApiConfig(LoggerConfiguration loggerConfiguration, WebApiConfig apiConfig)
        {
            apiConfig = apiConfig ?? new WebApiConfig();

            loggerConfiguration.MinimumLevel.ControlledBy(apiConfig.InnerSerilogLevels.AppLogLevel)
                .MinimumLevel.Override("Microsoft", apiConfig.InnerSerilogLevels.SystemLogLevel)
                .MinimumLevel.Override("System", apiConfig.InnerSerilogLevels.SystemLogLevel)
                .MinimumLevel.Override("Microsoft.AspNetCore", apiConfig.InnerSerilogLevels.AppLogLevel)
                .MinimumLevel.Override("Microsoft.Hosting", apiConfig.InnerSerilogLevels.AppLogLevel)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", apiConfig.InnerSerilogLevels.EFCoreCommandLevel)
                .MinimumLevel.Override("System.Net.Http.HttpClient", apiConfig.InnerSerilogLevels.AppLogLevel)
                .MinimumLevel.Override("ServerRequest.Logger", apiConfig.InnerSerilogLevels.ServerRequestLevel)
                .MinimumLevel.Override("ClientRequest.Logger", apiConfig.InnerSerilogLevels.ServerRequestLevel)
                .Destructure.ToMaximumStringLength(apiConfig.MaxLogLength);

            loggerConfiguration = loggerConfiguration.Enrich.FromLogContext();
            if (apiConfig.ConsoleLog)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console();
            }
            else
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console(Serilog.Events.LogEventLevel.Warning);
            }
            if (apiConfig.FileLog)
            {

                string path = apiConfig.GetLogPath();

                string pathTemplate = apiConfig.LogPathTemplate;
                if (string.IsNullOrEmpty(pathTemplate))
                {
                    pathTemplate = LogPathTemplates.DayFolderAndLoggerNameFile;
                }

                Serilog.Formatting.Display.MessageTemplateTextFormatter pathFormatter = new Serilog.Formatting.Display.MessageTemplateTextFormatter(pathTemplate);

                ArchiveHooks archiveHooks = new ArchiveHooks(CompressionLevel.Fastest);
                string template = apiConfig.LogTemplate;
                if (string.IsNullOrEmpty(template))
                {
                    template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{NewLine}{Exception}";
                }
                bool keySelector(Serilog.Events.LogEvent logEvent, out string key)
                {
                    StringWriter sw = new StringWriter();
                    StringBuilder sb = new StringBuilder();
                    pathFormatter.Format(logEvent, sw);
                    sw.Flush();
                    string dp = sw.GetStringBuilder().ToString();
                    key = Path.Combine(path, dp);
                    return true;
                };
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Map<string>(keySelector, (path, lc) => {
                        lc.Async(lc => lc.File(
                             path,
                             rollingInterval: RollingInterval.Infinite,
                             rollOnFileSizeLimit: true,
                             fileSizeLimitBytes: apiConfig.MaxLogFileSize,
                             retainedFileCountLimit: 128,
                             hooks: archiveHooks,
                             outputTemplate: template));
                    });
            }

            return loggerConfiguration;
        }
    }
}
