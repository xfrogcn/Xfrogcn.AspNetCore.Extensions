using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public static class LogLevelConverter
    {
        public static LogLevel Converter(LogEventLevel level)
        {
            LogLevel l = LogLevel.Trace;
            switch (level)
            {
                case LogEventLevel.Debug:
                    l = LogLevel.Debug;
                    break;
                case LogEventLevel.Error:
                    l = LogLevel.Error;
                    break;
                case LogEventLevel.Fatal:
                    l = LogLevel.Critical;
                    break;
                case LogEventLevel.Information:
                    l = LogLevel.Information;
                    break;
                case LogEventLevel.Verbose:
                    l = LogLevel.Trace;
                    break;
                case LogEventLevel.Warning:
                    l = LogLevel.Warning;
                    break;
            }

            return l;
        }
    }
}
