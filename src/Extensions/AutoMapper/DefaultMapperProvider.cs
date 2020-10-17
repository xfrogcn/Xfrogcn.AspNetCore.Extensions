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

        public IServiceProvider ServiceProvider => _serviceProvider;

        public IMapper<TSource, TTarget> GetMapper<TSource, TTarget>()
        {
            return _serviceProvider.GetRequiredService<IMapper<TSource, TTarget>>();
        }
    }
}
