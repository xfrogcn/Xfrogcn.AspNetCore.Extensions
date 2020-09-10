﻿using System.Collections.Generic;
using Serilog.Events;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class WebApiConfig
    {

        /// <summary>
        /// 系统日志级别
        /// </summary>
        public LogEventLevel SystemLogLevel { get; set; }

        /// <summary>
        /// 全局日志级别
        /// </summary>
        public LogEventLevel AppLogLevel { get; set; }

        /// <summary>
        /// EF Core Command日志记录级别
        /// </summary>
        public LogEventLevel EFCoreCommandLevel { get; set; }

        /// <summary>
        /// 请求日志级别
        /// </summary>
        public LogEventLevel? RequestLogLevel { get; set; }

        /// <summary>
        /// 日志是否输出到控制台
        /// </summary>
        public bool ConsoleLog { get; set; }

        /// <summary>
        /// 需要记录到日志的HTTP请求头键
        /// </summary>
        public List<string> HttpHeaders { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 日志最大长度
        /// </summary>
        public int MaxLogLength { get; set; }
        /// <summary>
        /// 是否忽略长日志, 默认忽略，除非设置了IGNORE_LONG_LOG
        /// </summary>
        public bool IgnoreLongLog{get;set;}

        public WebApiConfig()
        {
            SystemLogLevel = LogEventLevel.Warning;
            AppLogLevel = LogEventLevel.Verbose;
            //默认记录EFCore查询语句
            EFCoreCommandLevel = LogEventLevel.Debug;
            RequestLogLevel = LogEventLevel.Verbose;
            ConsoleLog = true;
            HttpHeaders = new List<string>() { "x-request-id" };
            MaxLogLength = 1024 * 8; //8kb
            IgnoreLongLog = true;
        }
    }
}
