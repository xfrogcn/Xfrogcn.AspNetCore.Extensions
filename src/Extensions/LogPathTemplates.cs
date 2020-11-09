using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    public static class LogPathTemplates
    {
        public static readonly string DayFolderAndLoggerNameFile = "{Timestamp:yyyyMMdd}" + System.IO.Path.PathSeparator +"{SourceContext}.log";
        public static readonly string DayFile = "{Timestamp:yyyyMMdd}.log";
        public static readonly string LoggerNameAndDayFile = "{SourceContext}_{Timestamp:yyyyMMdd}.log";
        public static readonly string LevelFile = "{Level:u3}.log";
        public static readonly string DayFolderAndLevelFile = "{Timestamp:yyyyMMdd}" + System.IO.Path.PathSeparator + "{Level:u3}.log";
    }
}
