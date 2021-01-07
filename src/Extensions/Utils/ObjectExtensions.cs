using Xfrogcn.BinaryFormatter;

namespace Xfrogcn.AspNetCore.Extensions
{
    public static class ObjectExtensions
    {
        public static object GetEntity(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            return BinarySerializer.Deserialize(bytes);
        }

        public static T GetEntity<T>(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            return BinarySerializer.Deserialize<T>(bytes);
        }

        public static byte[] GetBytes(this object entity)
        {
            return BinarySerializer.Serialize(entity);
        }
    }
}
