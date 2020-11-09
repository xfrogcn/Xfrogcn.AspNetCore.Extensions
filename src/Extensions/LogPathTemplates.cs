using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    public static class LogPathTemplates
    {
        public static readonly string DayFolderAndLoggerNameFile = "{Timestamp:yyyy-MM-dd}" + System.IO.Path.DirectorySeparatorChar +"{SourceContext}.log";
        public static readonly string DayFile = "{Timestamp:yyyy-MM-dd}.log";
        public static readonly string LoggerNameAndDayFile = "{SourceContext}_{Timestamp:yyyy-MM-dd}.log";
        public static readonly string LevelFile = "{Level:u3}.log";
        public static readonly string DayFolderAndLevelFile = "{Timestamp:yyyy-MM-dd}" + System.IO.Path.DirectorySeparatorChar + "{Level:u3}.log";
    }
}
