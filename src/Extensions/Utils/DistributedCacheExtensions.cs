using System.Threading;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class DistributedCacheExtensions
    {
        private static JsonHelper jsonHelper = new JsonHelper();
        public static async Task<TEntity> GetAsync<TEntity>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            if(bytes == null || bytes.Length==0)
            {
                return default(TEntity);
            }


            return (TEntity)bytes.GetEntity();
        }

 

        public static async Task SetAsync<TEntity>(this IDistributedCache cache, string key, TEntity obj, CancellationToken cancellationToken = default)
        {
            byte[] bytes = obj.GetBytes();

            await cache.SetAsync(key, bytes, cancellationToken);
        }

    }
}
