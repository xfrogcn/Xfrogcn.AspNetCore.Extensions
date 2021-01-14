using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    public static class LogPathTemplates
    {
        /// <summary>
        /// 以每天日期为目录，日志名称为文件名
        /// </summary>
        public static readonly string DayFolderAndLoggerNameFile = "{Timestamp:yyyy-MM-dd}" + System.IO.Path.DirectorySeparatorChar +"{SourceContext}.log";
        /// <summary>
        /// 以每天日期为日志名称
        /// </summary>
        public static readonly string DayFile = "{Timestamp:yyyy-MM-dd}.log";
        /// <summary>
        /// 以[日志名称_每天日志]为日志文件名称
        /// </summary>
        public static readonly string LoggerNameAndDayFile = "{SourceContext}_{Timestamp:yyyy-MM-dd}.log";
        /// <summary>
        /// 以日志级别缩写为日志文件名称
        /// </summary>
        public static readonly string LevelFile = "{Level:u3}.log";
        /// <summary>
        /// 以每天日期为目录，日志级别缩写为日志名称
        /// </summary>
        public static readonly string DayFolderAndLevelFile = "{Timestamp:yyyy-MM-dd}" + System.IO.Path.DirectorySeparatorChar + "{Level:u3}.log";
    }
}
