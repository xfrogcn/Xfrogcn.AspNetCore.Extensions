using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serilog
{
    public static  class LoggerConfigurationExtension
    {
        /// <summary>
        /// 指定需要记录的HTTP请求头
        /// </summary>
        /// <param name="config"></param>
        /// <param name="headerKeys"></param>
        /// <returns></returns>
        public static LoggerConfiguration LogHttpHeaders(this LoggerConfiguration config, string[] headerKeys)
        {
            if (headerKeys == null || headerKeys.Length == 0)
            {
                return config;
            }

            var hl = WebApiHostBuilderExtensions.config.HttpHeaders;
            foreach (string k in headerKeys)
            {
                if(!hl.Any(a=>a == k))
                {
                    hl.Add(k);
                }
            }

            return config;
        }
    }
}
