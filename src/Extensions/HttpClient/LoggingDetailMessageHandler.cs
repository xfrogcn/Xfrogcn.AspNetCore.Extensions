using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 消息日志Handler
    /// </summary>
    class LoggingDetailMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
       
        private static readonly EventId requestEventId = new EventId(500, "RequestDetail");
        private static readonly EventId responseEventId = new EventId(501, "ResponseDetail");
        private static readonly EventId requestErrorEventId = new EventId(505, "RequestError");
        private static readonly Action<ILogger, HttpMethod, Uri, string, Exception> _requestLog = LoggerMessage.Define<HttpMethod, Uri, string>(LogLevel.Trace, requestEventId, "请求方法：{httpMethod}, 请求URI：{uri}, 请求内容：{content}");
        private static readonly Action<ILogger, HttpMethod, Uri, string, Exception> _requestErrorLog = LoggerMessage.Define<HttpMethod, Uri, string>(LogLevel.Error, requestErrorEventId, "请求发生异常，请求方法：{httpMethod}, 请求URI：{uri}, 请求内容：{content}");
        private static readonly Action<ILogger, HttpMethod, Uri, string, Exception> _responseLog = LoggerMessage.Define<HttpMethod, Uri, string>(LogLevel.Trace, responseEventId, "请求方法：{httpMethod}, 请求URI：{uri}, 应答内容：{content}");
       
        public LoggingDetailMessageHandler(ILogger logger)
        {
            _logger = logger;
        }


        private bool isTextContent(string mediaType)
        {
            if(!string.IsNullOrEmpty(mediaType) && (mediaType.Contains("json") ||
                mediaType.Contains("xml") ||
                mediaType.Contains("html") ||
                mediaType.Contains("text"))
                )
            {
                return true;
            }
            return false;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            string requestContent = null;
            
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                if (request.Method != HttpMethod.Get && request.Content != null)
                {
                    if (isTextContent(request.Content.Headers?.ContentType?.MediaType))
                    {
                        requestContent = await request.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        requestContent = $"stream content, length: {request.Content.Headers.ContentLength}";
                    }
                }
                if (string.IsNullOrEmpty(requestContent))
                {
                    requestContent = string.Empty;
                }
                _requestLog(_logger, request.Method, request.RequestUri, requestContent, null);
            }
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                string responseContent = string.Empty;
                if (response != null && response.Content != null && isTextContent(response.Content.Headers?.ContentType?.MediaType) && (response.Content.Headers.ContentLength??0) <= (1024 * 1024 * 5))
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    if(response == null)
                    {
                        responseContent = "请求应答返回null";
                    }
                    _responseLog(_logger, request.Method, request.RequestUri, responseContent, null);
                }
                return response;

            }
            catch (Exception e)
            {
                if (request.Method != HttpMethod.Get && requestContent==null)
                {
                    requestContent = await request.Content.ReadAsStringAsync();
                }
                _requestErrorLog(_logger, request.Method, request.RequestUri, requestContent, e);
                throw;
            }
        }
    }
}
