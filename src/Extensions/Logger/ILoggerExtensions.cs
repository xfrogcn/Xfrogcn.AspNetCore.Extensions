using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.Logging
{
    public static class ILoggerExtensions
    {
        public static bool IsEnabled(this ILogger logger, LogEventLevel level)
        {
            LogLevel logLevel = LogLevelConverter.Converter(level);
            return logger.IsEnabled(logLevel);
        }
    }
}
