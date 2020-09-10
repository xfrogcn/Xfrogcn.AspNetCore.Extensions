using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class DisableEnsureSuccessStatusCodeMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.DisableEnsureSuccessStatusCode();
            return base.SendAsync(request, cancellationToken);
        }
    }
}
