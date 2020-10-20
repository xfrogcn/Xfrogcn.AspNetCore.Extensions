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


            return (TEntity)bytes.GetEntity();
        }

 

        public static async Task SetAsync<TEntity>(this IDistributedCache cache, string key, TEntity obj, CancellationToken cancellationToken = default)
        {
            byte[] bytes = obj.GetBytes();

            await cache.SetAsync(key, bytes, cancellationToken);
        }

        public static object GetEntity(this byte[] bytes)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(bytes);
            return bf.Deserialize(ms);
        }

        public static byte[] GetBytes(this object entity)
        {
            byte[] bytes = new byte[0];
            if (entity != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, entity);
                ms.Position = 0;
                BinaryReader br = new BinaryReader(ms);
                bytes = br.ReadBytes((int)ms.Length);
            }
            return bytes;
        }
    }
}
