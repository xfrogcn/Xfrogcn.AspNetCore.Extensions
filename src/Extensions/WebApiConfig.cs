using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class WebApiConfig
    {
        internal class SerilogLevels
        {
            public LoggingLevelSwitch SystemLogLevel { get;  }

            public LoggingLevelSwitch AppLogLevel { get; }

            public LoggingLevelSwitch EFCoreCommandLevel { get; }

            public LoggingLevelSwitch ServerRequestLevel { get; }

            public LoggingLevelSwitch ClientRequestLevel { get; }

            public SerilogLevels()
            {
                SystemLogLevel = new LoggingLevelSwitch();
                AppLogLevel = new LoggingLevelSwitch();
                EFCoreCommandLevel = new LoggingLevelSwitch();
                ServerRequestLevel = new LoggingLevelSwitch();
                ClientRequestLevel = new LoggingLevelSwitch();
            }
        }

        internal SerilogLevels InnerSerilogLevels { get; } = new SerilogLevels();

        /// <summary>
        /// 是否启用Serilog
        /// </summary>
        public bool EnableSerilog { get; set; }
        /// <summary>
        /// 系统日志级别
        /// </summary>
        public LogEventLevel SystemLogLevel
        {
            get
            {
                return InnerSerilogLevels.SystemLogLevel.MinimumLevel;
            }
            set
            {
                InnerSerilogLevels.SystemLogLevel.MinimumLevel = value;
            }
        }

        /// <summary>
        /// 全局日志级别
        /// </summary>
        public LogEventLevel AppLogLevel
        {
            get
            {
                return InnerSerilogLevels.AppLogLevel.MinimumLevel;
            }
            set
            {
                InnerSerilogLevels.AppLogLevel.MinimumLevel = value;
            }
        }

        /// <summary>
        /// EF Core Command日志记录级别
        /// </summary>
        public LogEventLevel EFCoreCommandLevel
        {
            get
            {
                return InnerSerilogLevels.EFCoreCommandLevel.MinimumLevel;
            }
            set
            {
                InnerSerilogLevels.EFCoreCommandLevel.MinimumLevel = value;
            }
        }

        /// <summary>
        /// 请求日志级别
        /// </summary>
        public LogEventLevel ServerRequestLevel
        {
            get
            {
                return InnerSerilogLevels.ServerRequestLevel.MinimumLevel;
            }
            set
            {
                InnerSerilogLevels.ServerRequestLevel.MinimumLevel = value;
            }
        }

        /// <summary>
        /// 客户端请求日志级别
        /// </summary>
        public LogEventLevel ClientRequestLevel
        {
            get
            {
                return InnerSerilogLevels.ClientRequestLevel.MinimumLevel;
            }
            set
            {
                InnerSerilogLevels.ClientRequestLevel.MinimumLevel = value;
            }
        }

        public bool EnableClientRequestLog { get; set; }

        /// <summary>
        /// 日志是否输出到控制台
        /// </summary>
        public bool ConsoleLog { get; set; } 

        /// <summary>
        /// 文件日志
        /// </summary>
        public bool FileLog { get; set; }

        /// <summary>
        /// 需要记录到日志的HTTP请求头键
        /// </summary>
        public List<string> HttpHeaders { get; set; }

        /// <summary>
        /// 日志最大长度
        /// </summary>
        public int MaxLogLength { get; set; }

        /// <summary>
        /// 日志路径模版
        /// </summary>
        public string LogPathTemplate { get; set; }

        /// <summary>
        /// 日志路径
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// 日志文件最大容量
        /// </summary>
        public long MaxLogFileSize { get; set; }

        /// <summary>
        /// 日志模版
        /// </summary>
        public string LogTemplate { get; set; }

        /// <summary>
        /// 日志最长保留天数
        /// </summary>
        public int MaxLogDays { get; set; }

        public WebApiConfig()
        {
            EnableSerilog = true;
            SystemLogLevel = LogEventLevel.Warning;
            AppLogLevel = LogEventLevel.Verbose;
            //默认记录EFCore查询语句
            EFCoreCommandLevel = LogEventLevel.Debug;
            ServerRequestLevel = LogEventLevel.Information;
            ClientRequestLevel = LogEventLevel.Verbose;
            EnableClientRequestLog = true;
            ConsoleLog = false;
            FileLog = true;
            HttpHeaders = new List<string>() { "x-request-id" };
            MaxLogLength = 1024 * 8; //8kb
            LogPathTemplate = LogPathTemplates.DayFolderAndLoggerNameFile;
            LogPath = "Logs";
            MaxLogFileSize = 1024 * 1024 * 100;
            LogTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{NewLine}{Exception}";
        }
    }
}
