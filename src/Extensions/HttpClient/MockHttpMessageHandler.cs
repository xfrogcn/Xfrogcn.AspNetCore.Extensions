using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
   
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        readonly MockHttpMessageHandlerOptions _options = null;
        public MockHttpMessageHandler(MockHttpMessageHandlerOptions options)
        {
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage r = new HttpResponseMessage();
            foreach (MockHttpMessageHandlerOptions.MockItem mi in _options.MockList)
            {
                bool canProc = mi.Predicate(request);
                if (canProc)
                {
                    await mi.Proc(request, r);
                    break;
                }
            }
            return r;
        }
    }
}
