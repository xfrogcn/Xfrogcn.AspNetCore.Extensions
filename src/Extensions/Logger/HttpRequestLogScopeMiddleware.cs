using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Xfrogcn.AspNetCore.Extensions
{
    internal class HttpRequestLogScopeMiddleware : IMiddleware
    {
        private ILoggerFactory _loggerFactory;
        private WebApiConfig _config;

        private JsonHelper _jsonHelper;

        private static readonly EventId requestEventId = new EventId(100, "ReceiveRequest");
        private static readonly EventId responseEventId = new EventId(101, "SendResponse");
        private static readonly EventId requestErrorEventId = new EventId(102, "RequestError");
        private static readonly EventId requestWarningEventId = new EventId(103, "RequestWarning");
        private static readonly EventId requestCostEventId = new EventId(104, "RequestCost");

        private static Action<ILogger, string, string, string, string, Exception> _requestLog
            = LoggerMessage.Define<string, string, string, string>( LogLevel.Trace, requestEventId, "请求：{method} {url} {headers} \n {bodyString}");
        private static Action<ILogger, string, string, int?, string, Exception> _responseLog
            = LoggerMessage.Define<string, string, int?, string>(LogLevel.Trace, responseEventId, "请求应答：{method} {url} {status} \n {bodyString}" );

        private static readonly Action<ILogger, string, string, int?, double, Exception> _costLog
            = LoggerMessage.Define<string, string, int?, double>(LogLevel.Debug, requestCostEventId, "请求耗时: {method} {url} {status} {time}");
        private static readonly Action<ILogger, string, string, double, double, double, double, Exception> _costLongLog
            = LoggerMessage.Define<string, string, double, double, double, double>(LogLevel.Warning, requestWarningEventId, "请求耗时过长：{mtehod} {url} {t1}ms {t2}ms {t3}ms {t4}ms");
        private static readonly Action<ILogger, string, Exception> _errorLog
            = LoggerMessage.Define<string>(LogLevel.Error, requestErrorEventId, "请求日志记录异常：{msg}");


        private static Action<ILogger, string, string, int?, string, Exception> getResponseLog(LogLevel logLevel)
        {
            return LoggerMessage.Define<string, string, int?, string>(
                logLevel, responseEventId, "请求应答：{method} {url} {status} \n {bodyString} ");
        }

        private static Action<ILogger, string, string, string, string, Exception> getRequestLog(LogLevel logLevel)
        {
            return LoggerMessage.Define<string, string, string, string>(
                logLevel, responseEventId, "请求：{method} {url} {headers} \n {bodyString} ");
        }

        public HttpRequestLogScopeMiddleware(ILoggerFactory loggerFactory, WebApiConfig config, JsonHelper jsonHelper)
        {
            _loggerFactory = loggerFactory;
            _config = config;
            _jsonHelper = jsonHelper;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var logger = _loggerFactory.CreateLogger("ServerRequest.Logger");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            DateTime d1 = DateTime.Now;

            Dictionary<string, object> scopeItems = new Dictionary<string, object>();
            if (context != null && context.Request != null && context.Request.Headers != null)
            {
                StringValues sv;
                foreach (string key in _config.HttpHeaders)
                {
                    if (context.Request.Headers.TryGetValue(key, out sv))
                    {
                        scopeItems.Add(key, sv.ToString());
                    }
                }

            }

            DateTime d2 = DateTime.Now;

            IDisposable scope = null;
            if (scopeItems.Count > 0)
            {
                scope = logger.BeginScope(scopeItems);
            }

            string url = context.Request.GetAbsoluteUri();


            //记录请求日志
            if (logger.IsEnabled(LogLevel.Trace))
            {

                try
                {
                    context.Request.EnableBuffering();
                    StreamReader reader = new StreamReader(context.Request.Body);
                    string bodyString = await reader.ReadToEndAsync();
                    _requestLog(logger, context.Request.Method, url, _jsonHelper.ToJson(context.Request.Headers), bodyString, null);
                    context.Request.Body.Position = 0;
                }
                catch (Exception e)
                {
                    context.Request.Body.Position = 0;
                    _errorLog(logger, "请求日志", e);
                }
            }

            bool logResposne = logger.IsEnabled(LogLevel.Trace);
            var bodyStream = context.Response.Body;
            MemoryStream tempResponseBodyStream = null;
            if (logResposne)
            {
                tempResponseBodyStream = new MemoryStream();
                context.Response.Body = tempResponseBodyStream;
            }

            DateTime d3 = DateTime.Now;

            await next(context);

            DateTime d4 = DateTime.Now;

            //记录应答日志
            if (logResposne && tempResponseBodyStream != null)
            {
                if (context.Response.ContentType != null && (context.Response.ContentType.Contains("json") || context.Response.ContentType.Contains("text")))
                {
                    try
                    {
                        string bodyString = "";
                        tempResponseBodyStream.Seek(0, SeekOrigin.Begin);

                        if (tempResponseBodyStream.Length > 0)
                        {
                            StreamReader reader = new StreamReader(context.Response.Body);
                            bodyString = await reader.ReadToEndAsync();
                        }
                        _responseLog(logger, context.Request.Method, url, context.Response?.StatusCode, bodyString, null);
                        tempResponseBodyStream.Seek(0, SeekOrigin.Begin);
                    }
                    catch (Exception e)
                    {
                        context.Request.Body.Position = 0;
                        _errorLog(logger, "应答日志", e);
                    }
                    finally
                    {
                        await tempResponseBodyStream.CopyToAsync(bodyStream);
                    }
                }
                else
                {
                    tempResponseBodyStream.Seek(0, SeekOrigin.Begin);
                    await tempResponseBodyStream.CopyToAsync(bodyStream);
                }
            }

            sw.Stop();

            DateTime d5 = DateTime.Now;

            _costLog(logger, context.Request.Method, url, context.Response?.StatusCode, sw.ElapsedMilliseconds, null);

            if (sw.ElapsedMilliseconds >= 2000)
            {
                _costLongLog(
                    logger,
                    context.Request.Method,
                    context.Request.Path,
                    (d2 - d1).TotalMilliseconds,
                    (d3 - d2).TotalMilliseconds,
                    (d4 - d3).TotalMilliseconds,
                    (d5 - d4).TotalMilliseconds,
                    null);
            }

            scope?.Dispose();
        }
    }
}
