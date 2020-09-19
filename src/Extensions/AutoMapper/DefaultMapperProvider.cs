using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class DefaultMapperProvider : IMapperProvider
    {
        private readonly IServiceProvider _serviceProvider;
        public DefaultMapperProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMapper<TSource, TTarget> GetMapper<TSource, TTarget>()
            where TSource : class
            where TTarget : new()
        {
            return _serviceProvider.GetRequiredService<IMapper<TSource, TTarget>>();
        }
    }
}
