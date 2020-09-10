using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessageHandlerServiceCollectionExtensions
    {
        /// <summary>
        /// 注入HTTP请求日志记录
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpMessageHandlerFilter(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, MessageHandlerFilter>());
            return serviceDescriptors;
        }

    }
}
