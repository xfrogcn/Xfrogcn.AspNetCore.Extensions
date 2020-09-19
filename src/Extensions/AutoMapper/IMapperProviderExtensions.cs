using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    public static class IMapperProviderExtensions
    {
        public static TTarget Convert<TSource, TTarget>(this IMapperProvider provider, TSource sourceObj)
            where TSource : class
            where TTarget : new()
        {
            var mapper = provider.GetMapper<TSource, TTarget>();
            return mapper.Convert(sourceObj);
        }
    }
}
