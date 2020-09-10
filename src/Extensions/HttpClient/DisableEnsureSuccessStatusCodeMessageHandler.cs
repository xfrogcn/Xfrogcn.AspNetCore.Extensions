using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 在请求管道中增加一个消息处理器，该处理器在请求消息中增加一个DisableEnsureSuccessStatusCode标记
    /// 在HttpClient扩展方法中读取此标记，如果存在，将忽略应答状态检查
    /// </summary>
    public class DisableEnsureSuccessStatusCodeMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.DisableEnsureSuccessStatusCode();
            return base.SendAsync(request, cancellationToken);
        }
    }
}
