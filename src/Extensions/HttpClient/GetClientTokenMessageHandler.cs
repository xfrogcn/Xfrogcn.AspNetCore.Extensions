using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 此类自动向发出的HTTP请求头中添加token
    /// </summary>
    class GetClientTokenMessageHandler : DelegatingHandler
    {
        public const string IGNORE_TOKEN_PROPERTY = nameof(IGNORE_TOKEN_PROPERTY);
        private readonly ClientCertificateManager _tokenManager;
        public GetClientTokenMessageHandler(ClientCertificateManager tokenManager)
        {
            _tokenManager = tokenManager;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 忽略token
            if (request.Properties.ContainsKey(IGNORE_TOKEN_PROPERTY))
            {
                return await base.SendAsync(request, cancellationToken);
            }
            HttpResponseMessage lastResponse = null;
            try
            {
                var response = await _tokenManager.Execute<HttpResponseMessage>(async (token, setter, checker) =>
               {
                   await setter.SetTokenAsync(request, token);
                   var response = await base.SendAsync(request, cancellationToken);
                   lastResponse = response;
                   await checker.CheckResponseAsync(response);
                   return response;
               });
            }
            catch
            {

            }

            return lastResponse;
        }
    }
}
