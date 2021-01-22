using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 将请求来源的请求头传递给发出请求
    /// </summary>
    class TransRequestHeadersMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WebApiConfig _config;
        public TransRequestHeadersMessageHandler(IHttpContextAccessor httpContextAccessor, WebApiConfig config)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor != null)
            {
                SetRequest(request);
            }
            return base.SendAsync(request, cancellationToken);
        }

        protected virtual void SetRequest(HttpRequestMessage client)
        {
            HttpContext httpContext = null;
            if (_httpContextAccessor != null)
            {
                httpContext = _httpContextAccessor.HttpContext;
            }

            if (httpContext == null || httpContext.Request == null || httpContext.Request.Headers == null || client == null)
                return;

            IHeaderDictionary header = httpContext.Request.Headers;
            var incomingHeaders = _config?.TrackingHeaders?.HeaderRegex;
            if (incomingHeaders == null)
                return;

            foreach (string key in header.Keys)
            {
                if (incomingHeaders.Any(x=>x.IsMatch(key)))
                {
                    StringValues sv = header[key];

                    // 当且仅当当前请求头中不存在对应头时才添加
                    if(client.Headers.Contains(key))
                    {
                        continue;
                    }

                    client.Headers.Add(key, sv.ToString());

                }
            }
        }

    }
}
