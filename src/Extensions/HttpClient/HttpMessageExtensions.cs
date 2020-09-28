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
        public static async Task<TResponse> GetObjectAsync<TResponse>(this HttpResponseMessage response, bool copy = false)
        {
            if (response == null || response.Content == null)
            {
                return default;
            }

            Stream stream = null;
            if (copy)
            {
                await response.Content.LoadIntoBufferAsync();
            }

            stream = await response.Content.ReadAsStreamAsync();


            if (typeof(TResponse) == typeof(string))
            {
                StreamReader sr = new StreamReader(stream);
                var str = (TResponse)(object)(await sr.ReadToEndAsync());
                stream.Position = 0;
                return str;
            }
            else if (typeof(TResponse) == typeof(HttpResponseMessage))
            {
                // 如果类型为HttpResponseMessage直接返回
                return (TResponse)(object)response;
            }
            else
            {
                var obj = await JsonHelper.ToObjectAsync<TResponse>(stream);
                stream.Position = 0;
                return obj;
            }
        }

        public static async Task<TResponse> GetObjectAsync<TResponse>(this HttpRequestMessage request, bool copy = false)
        {
            if (request == null || request.Content == null)
            {
                return default;
            }

            Stream stream = null;
            if (copy)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            stream = await request.Content.ReadAsStreamAsync();


            if (typeof(TResponse) == typeof(string))
            {
                StreamReader sr = new StreamReader(stream);
                var str = (TResponse)(object)(await sr.ReadToEndAsync());
                stream.Position = 0;
                return str;
            }
            else if (typeof(TResponse) == typeof(HttpRequestMessage))
            {
                // 如果类型为HttpResponseMessage直接返回
                return (TResponse)(object)request;
            }
            else
            {
                var obj = await JsonHelper.ToObjectAsync<TResponse>(stream);
                stream.Position = 0;
                return obj;
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
