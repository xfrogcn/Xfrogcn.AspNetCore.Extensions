using System.IO;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace System.Net.Http
{
    public static class HttpMessageExtensions
    {
        public static JsonHelper JsonHelper = new JsonHelper();

        /// <summary>
        /// 从Response中获取指定类型的对象
        /// </summary>
        /// <typeparam name="TResponse">应答对象类型</typeparam>
        /// <param name="response">应答消息</param>
        /// <returns>应答对象</returns>
        public static async Task<TResponse> GetObjectAsync<TResponse>(this HttpResponseMessage response)
        {
            if (response == null || response.Content == null)
            {
                return default;
            }

            if (typeof(TResponse) == typeof(string))
            {
                return (TResponse)(object)(await response.Content.ReadAsStringAsync());
            }
            else
            {
                return await JsonHelper.ToObjectAsync<TResponse>(await response.Content.ReadAsStreamAsync());
            }
        }

        public static async Task<TResponse> GetObjectAsync<TResponse>(this HttpRequestMessage request)
        {
            if (request == null || request.Content == null)
            {
                return default;
            }

            if (typeof(TResponse) == typeof(string))
            {
                return (TResponse)(object)(await request.Content.ReadAsStringAsync());
            }
            else
            {
                return await JsonHelper.ToObjectAsync<TResponse>(await request.Content.ReadAsStreamAsync());
            }
        }



        public static async Task WriteObjectAsync(this HttpRequestMessage request, object body)
        {
            MemoryStream ms = await createContentStream(body);
            request.Content = new StreamContent(ms);
        }

        public static async Task WriteObjectAsync(this HttpResponseMessage rsponse, object body)
        {
            MemoryStream ms = await createContentStream(body);
            rsponse.Content = new StreamContent(ms);
        }


        private static async Task<MemoryStream> createContentStream(object body)
        {
            MemoryStream ms = new MemoryStream();
            
            if (body != null)
            {
                if (body.GetType() == typeof(string))
                {
                    StreamWriter sw = new StreamWriter(ms);
                    await sw.WriteAsync((string)body);
                    await sw.FlushAsync();
                    ms.Position = 0;
                }
                else
                {
                    await JsonHelper.ToJsonAsync(ms, body);
                    ms.Flush();
                    ms.Position = 0;
                }

            }

            return ms;
        }
    }
}
