using System;
using System.Collections.Generic;

namespace Xfrogcn.AspNetCore.Extensions
{
    public static class IMapperProviderExtensions
    {
        public static TTarget Convert<TSource, TTarget>(this IMapperProvider provider, TSource sourceObj)
        {
            var mapper = provider.GetMapper<TSource, TTarget>();
            return mapper.Convert(sourceObj);
        }

        public static List<TTarget> ConvertList<TSource, TTarget>(this IMapperProvider provider, List<TSource> sourceObj)
        {
            var mapper = provider.GetMapper<List<TSource>, List<TTarget>>();
            return mapper.Convert(sourceObj);
        }


        public static void CopyTo<TSource, TTarget>(this IMapperProvider provider, TSource sourceObj, TTarget targetObj)
        {
            var mapper = provider.GetMapper<TSource, TTarget>();
            mapper.CopyTo(sourceObj, targetObj);
        }
    }
}
