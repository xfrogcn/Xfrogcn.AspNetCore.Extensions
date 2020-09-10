using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class DistributedCacheExtensions
    {
        public static async Task<TEntity> GetAsync<TEntity>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            if(bytes == null || bytes.Length==0)
            {
                return default(TEntity);
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(bytes);
            return (TEntity)bf.Deserialize(ms);
        }

        public static async Task SetAsync<TEntity>(this IDistributedCache cache, string key, TEntity obj, CancellationToken cancellationToken = default)
        {
            byte[] bytes = new byte[0];
            if(obj != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                BinaryReader br = new BinaryReader(ms);
                bytes = br.ReadBytes((int)ms.Length);
            }

            await cache.SetAsync(key, bytes, cancellationToken);
        }
    }
}
