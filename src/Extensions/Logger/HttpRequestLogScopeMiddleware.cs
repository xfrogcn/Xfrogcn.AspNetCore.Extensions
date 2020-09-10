using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    internal class HttpRequestLogScopeMiddleware : IMiddleware
    {
        private ILoggerFactory _loggerFactory;
        private WebApiConfig _config;

        private IJsonHelper _jsonHelper;

        public HttpRequestLogScopeMiddleware(ILoggerFactory loggerFactory, WebApiConfig config, IJsonHelper jsonHelper)
        {
            _loggerFactory = loggerFactory;
            _config = config;
            _jsonHelper = jsonHelper;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {


            Stopwatch sw = new Stopwatch();
            sw.Start();
            DateTime d1 = DateTime.Now;

            var logger = _loggerFactory.CreateLogger<HttpRequestLogScopeMiddleware>();

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

            IDisposable scope = new EmptyDisposable();
            if (scopeItems.Count > 0)
            {
                scope = logger.BeginScope(scopeItems);
            }

            string url = context.Request.Path + "?" + context.Request.QueryString.Value;

            LogLevel? logLevel = LogLevelConverter.Converter(_config.RequestLogLevel);
            //记录请求日志
            if (_config.RequestLogLevel != null && logger.IsEnabled(logLevel.Value))
            {

                try
                {
                    context.Request.EnableBuffering();
                    //  context.Request.EnableRewind();
                    StreamReader reader = new StreamReader(context.Request.Body);
                    string bodyString = await reader.ReadToEndAsync();
                    logger.Log(logLevel.Value, $"请求：{context.Request.Method} {url} \n { _jsonHelper.ToJson(context.Request.Headers) } \n {bodyString} ");
                    context.Request.Body.Position = 0;
                }
                catch (Exception e)
                {
                    context.Request.Body.Position = 0;
                    logger.LogError(e, "记录请求日志异常");
                }

            }

            bool logResposne = (_config.RequestLogLevel != null && logger.IsEnabled(logLevel.Value));
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
                        logger.Log(logLevel.Value, $"请求应答：{ context.Response?.StatusCode} {context.Request.Method} {url} \n {bodyString} ");
                        //logger.Log(logLevel.Value, $"请求应答：{ context.Response?.StatusCode} {context.Request.Method} {url} \n { _jsonHelper.ToJson(context.Response.Headers) } \n {bodyString} ");
                        tempResponseBodyStream.Seek(0, SeekOrigin.Begin);
                    }
                    catch (Exception e)
                    {
                        context.Request.Body.Position = 0;
                        logger.LogError(e, "记录请求应答日志异常");
                    }
                    finally
                    {
                        await tempResponseBodyStream.CopyToAsync(bodyStream);
                    }
                }
                else if (context.Response.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        || context.Response.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                        || context.Response.ContentType == "application/vnd.ms-excel"
                        || context.Response.ContentType == "application/msword")
                {
                    tempResponseBodyStream.Seek(0, SeekOrigin.Begin);
                    await tempResponseBodyStream.CopyToAsync(bodyStream);
                }
                else
                {
                    var buffer = tempResponseBodyStream.GetBuffer();
                    var len = buffer.Length;
                    if (len > 0)
                    {
                        var contentLen = len - 1;
                        for (; contentLen >= 0; contentLen--)
                        {
                            if (buffer[contentLen] > 0) break;
                        }
                        await bodyStream.WriteAsync(buffer, 0, contentLen + 1);
                    }
                }
            }

            sw.Stop();

            DateTime d5 = DateTime.Now;


            logger.LogDebug($"请求耗时：{sw.ElapsedMilliseconds}ms {context.Response?.StatusCode} {context.Request.Method} {url}");

            if (sw.ElapsedMilliseconds >= 2000)
            {
                logger.LogWarning($"请求耗时过长：{(d2 - d1).TotalMilliseconds}ms {(d3 - d2).TotalMilliseconds}ms {(d4 - d3).TotalMilliseconds}ms {(d5 - d4).TotalMilliseconds}ms ");
            }

            scope.Dispose();
        }
    }
}
