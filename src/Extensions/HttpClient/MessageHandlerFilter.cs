using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class MessageHandlerFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory = null;
        private readonly IHttpContextAccessor _httpContextAccessor;

        
        public MessageHandlerFilter(
            ILoggerFactory loggerFactory, 
            IHttpContextAccessor httpContextAccessor)
        {
            _loggerFactory = loggerFactory;
            _httpContextAccessor = httpContextAccessor;
        }



        public MessageHandlerFilter(ILoggerFactory loggerFactory) :
             this(loggerFactory, (IHttpContextAccessor)null)
        {
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return (builder) =>
            {
                // 先调用其他的配置
                next(builder);
                
                // 将自定义的LoggingDetailMessageHandler放到首位
                // 分类名称与内置日志名称保持一致
                ILogger requestLogger = _loggerFactory.CreateLogger("System.Net.Http.HttpClient.Default.LogicalHandler");
                builder.AdditionalHandlers.Insert(0, new LoggingDetailMessageHandler(requestLogger));
                if (_httpContextAccessor != null)
                {
                    builder.AdditionalHandlers.Insert(0, new TransRequestHeadersMessageHandler(_httpContextAccessor));
                }
            };
        }
    }
}
