using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MapperServiceCollectionExtensions
    {
        public static IServiceCollection AddLightweightMapper(this IServiceCollection serviceDescriptors, Action<MapperOptions> options=null)
        {
            serviceDescriptors.TryAddSingleton<IMapperProvider, DefaultMapperProvider>();
            serviceDescriptors.TryAddSingleton(typeof(IMapper<,>), typeof(DefaultMapper<,>));
            serviceDescriptors.Configure<MapperOptions>(o =>
            {
                options?.Invoke(o);
            });
            return serviceDescriptors;
        }
    }
}
