﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.File.Archive;
using Serilog.Sinks.Map;

namespace Xfrogcn.AspNetCore.Extensions
{
    public static class SerilogServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSerilog(this IServiceCollection serviceDescriptors, WebApiConfig apiConfig=null, Action<LoggerConfiguration> configureLogger = null, bool preserveStaticLogger = false, bool writeToProviders = false)
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

            return serviceDescriptors;
        }


        private static LoggerConfiguration configFromWebApiConfig(LoggerConfiguration loggerConfiguration, WebApiConfig apiConfig)
        {
            apiConfig = apiConfig ?? new WebApiConfig();

            loggerConfiguration.MinimumLevel.Is(apiConfig.AppLogLevel)
                .MinimumLevel.Override("Microsoft", apiConfig.SystemLogLevel)
                .MinimumLevel.Override("System", apiConfig.SystemLogLevel)
                .MinimumLevel.Override("Microsoft.AspNetCore", apiConfig.AppLogLevel)
                .MinimumLevel.Override("Microsoft.Hosting", apiConfig.AppLogLevel)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", apiConfig.EFCoreCommandLevel)
                .MinimumLevel.Override("System.Net.Http.HttpClient", apiConfig.AppLogLevel)
                .Destructure.ToMaximumStringLength(apiConfig.MaxLogLength);

            loggerConfiguration = loggerConfiguration.Enrich.FromLogContext();
            if (apiConfig.ConsoleLog)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console();
            }
            if (apiConfig.FileLog)
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
                        lc.File(
                             path,
                             rollingInterval: RollingInterval.Infinite,
                             rollOnFileSizeLimit: true,
                             fileSizeLimitBytes: apiConfig.MaxLogFileSize,
                             retainedFileCountLimit: 128,
                             hooks: archiveHooks,
                             outputTemplate: template);
                    });
            }

            return loggerConfiguration;
        }
    }
}