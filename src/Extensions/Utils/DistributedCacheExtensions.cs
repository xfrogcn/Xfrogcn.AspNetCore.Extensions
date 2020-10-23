using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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

        class JsonTypeWrapper
        {
            public string TypeName { get; set; }

            public string Json { get; set; }

            public string AssemblyName { get; set; }
        }

        public static object GetEntity(this byte[] bytes)
        {
            if( bytes==null || bytes.Length == 0)
            {
                return default;
            }
            
            string json = Encoding.UTF8.GetString(bytes);
            JsonTypeWrapper tw = jsonHelper.ToObject<JsonTypeWrapper>(json);
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName == tw.AssemblyName);
            Type t = assembly.GetType(tw.TypeName);
            return jsonHelper.ToObject(tw.Json, t);

            //BinaryFormatter bf = new BinaryFormatter();
            //MemoryStream ms = new MemoryStream(bytes);
            //return bf.Deserialize(ms);
        }

        public static byte[] GetBytes(this object entity)
        {
            byte[] bytes = new byte[0];
            if (entity != null)
            {
                JsonTypeWrapper tw = new JsonTypeWrapper()
                {
                    TypeName = entity.GetType().FullName,
                    AssemblyName = entity.GetType().Assembly.FullName,
                    Json = jsonHelper.ToJson(entity)
                };
                string json = jsonHelper.ToJson(tw);
                bytes = Encoding.UTF8.GetBytes(json);

                // 二进制序列化存在跨程序集的问题
                //BinaryFormatter bf = new BinaryFormatter();
                //MemoryStream ms = new MemoryStream();
                //bf.Serialize(ms, entity);
                //ms.Position = 0;
                //BinaryReader br = new BinaryReader(ms);
                //bytes = br.ReadBytes((int)ms.Length);
            }
            return bytes;
        }
    }
}
