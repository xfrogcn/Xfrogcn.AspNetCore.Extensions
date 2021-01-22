using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        /// 服务端请求日志记录级别
        /// 服务端请求详情日志默认级别为Trace，故将此属性设置为Trace可开启服务端详细日志
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

        /// <summary>
        /// 是否允许客户端请求日志
        /// </summary>
        public bool EnableClientRequestLog { get; set; }

        /// <summary>
        /// 日志是否输出到控制台，默认为false
        /// </summary>
        public bool ConsoleLog { get; set; } = false;

        /// <summary>
        /// 将日志输出到控制台，并使用JSON格式，默认为false
        /// </summary>
        public bool ConsoleJsonLog { get; set; } = false;

        /// <summary>
        /// 文件日志，默认为true
        /// </summary>
        public bool FileLog { get; set; } = true;

        /// <summary>
        /// 采用JSON格式的文件日志, 默认为false
        /// </summary>
        public bool FileJsonLog { get; set; } = false;

        /// <summary>
        /// 需要记录到日志的HTTP请求头键
        /// </summary>
        public List<string> HttpHeaders { get; set; }

        /// <summary>
        /// 链路跟踪Headers头，列表中的头将自动传递到下一个Http请求，可以使用*匹配
        /// </summary>
        public TrackingHeaders TrackingHeaders { get; private set; }

      
        /// <summary>
        /// 日志最大长度, 默认为8KB，此设置仅支持JSON格式，超过此长度的日志将被拆分或忽略（取决于IgnoreLongLog设置）
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
        /// 日志文件最大容量,默认为100mb，当超过此设置大小时，产生新的日志序列文件
        /// </summary>
        public long MaxLogFileSize { get; set; }

        /// <summary>
        /// 保留的旋转日志文件数量，超过此数量的文件将自动被压缩，默认为31, 数量必须大于等于1
        /// </summary>
        public int RetainedFileCount { get; set; } = 31;

        /// <summary>
        /// 日志模版
        /// </summary>
        public string LogTemplate { get; set; }

        /// <summary>
        /// 日志最长保留天数，默认0，不自动清理日志
        /// </summary>
        public int MaxLogDays { get; set; }

        /// <summary>
        /// 是否忽略长日志拆分，此设置仅支持JSON格式
        /// </summary>
        public bool IgnoreLongLog { get; set; } = false;


        public WebApiConfig()
        {
            EnableSerilog = true;
            SystemLogLevel = LogEventLevel.Warning;
            AppLogLevel = LogEventLevel.Information;
            //默认记录EFCore查询语句
            EFCoreCommandLevel = LogEventLevel.Information;
            ServerRequestLevel = LogEventLevel.Information;
            ClientRequestLevel = LogEventLevel.Information;
            EnableClientRequestLog = true;
            ConsoleLog = false;
            FileLog = true;
            HttpHeaders = new List<string>() { "x-request-id" };
            TrackingHeaders = new TrackingHeaders()
            {
                "x-*"
            };
            MaxLogLength = 1024 * 8; //8kb
            LogPathTemplate = LogPathTemplates.DayFolderAndLoggerNameFile;
            LogPath = "Logs";
            MaxLogFileSize = 1024 * 1024 * 100;
            LogTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{NewLine}{Exception}";
        }
    }
}
