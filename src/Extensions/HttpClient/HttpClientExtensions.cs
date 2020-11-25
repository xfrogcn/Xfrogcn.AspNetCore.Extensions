using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// HttpClient的扩展方法
    /// </summary>
    public static class HttpClientExtensions
    {
        

        public static async Task<TResponse> PostAsync<TResponse>(this HttpClient client, string url, object body, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)
        {    
            if (String.IsNullOrEmpty(method))
            {
                method = "POST";
            }
            HttpRequestMessage request = await createRequestMessage(method, url, body,queryString, headers);
            HttpResponseMessage response = await client.SendAsync(request);
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }
        public static async Task<string> PostAsync(this HttpClient client, string url, object body, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            return await PostAsync<string>(client, url, body, method, queryString, headers);
        }

        public static async Task<TResponse> GetAsync<TResponse>(this HttpClient client, string url, NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            HttpRequestMessage request = await createRequestMessage("GET", url, null, queryString, headers);
            HttpResponseMessage response = await client.SendAsync(request);
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }

        public static async Task<string> GetAsync(this HttpClient client, string url, NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            return await GetAsync<string>(client, url, queryString, headers);
        }

        public static async Task<TResponse> SubmitFormAsync<TResponse>(this HttpClient client, string url, Dictionary<string, string> formData, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null, bool ignoreEncode = false)
        {
            url = CreateUrl(url, queryString);
            HttpMethod hm = new HttpMethod(method);
            HttpRequestMessage request = new HttpRequestMessage(hm, url);
            if (ignoreEncode)
            {
               
                StringBuilder sb = new StringBuilder();
                if (formData != null)
                {
                    foreach (var kv in formData)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("&");
                        }
                        sb.Append(kv.Key).Append("=");
                        sb.Append(WebUtility.UrlEncode(kv.Value ?? ""));
                    }
                }
                request.Content = new StringContent(sb.ToString());
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }
            else
            {
                // 可能引发异常  System.UriFormatException : Invalid URI: The Uri string is too long.
                request.Content = new FormUrlEncodedContent(formData);
            }
            
            
            MergeHttpHeaders(request, headers);

            var response = await client.SendAsync(request);
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }
        public static async Task<string> SubmitFormAsync(this HttpClient client, string url, Dictionary<string, string> formData, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null, bool ignoreEncode = false)
        {
            return await SubmitFormAsync<string>(client, url, formData, method, queryString, headers, ignoreEncode);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <typeparam name="TResponse">应答类型</typeparam>
        /// <param name="client">HTTP客户端</param>
        /// <param name="url">上传接口地址</param>
        /// <param name="partKey">表达文件内容部分名称</param>
        /// <param name="file">文件路径</param>
        /// <param name="fileMediaType">文件MIME类型</param>
        /// <param name="boundary">分割字符串</param>
        /// <param name="formData">附加的表单数据</param>
        /// <param name="method">请求方法</param>
        /// <param name="queryString">查询字符串</param>
        /// <param name="headers">附加头</param>
        /// <returns>应答</returns>
        public static async Task<TResponse> UploadFileAsync<TResponse>(
            this HttpClient client, 
            string url, 
            string partKey,
            string file,
            string fileMediaType = null,
            string boundary = null,
            Dictionary<string, string> formData = null, 
            string method = "POST", 
            NameValueCollection queryString = null, 
            NameValueCollection headers = null)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(file);
            }

            if (string.IsNullOrEmpty(boundary))
            {
                boundary = $"--------------------------{DateTime.Now.Ticks}";
            }
 
            FileStream fileStream = new FileStream(file, FileMode.Open);
            string fileName = Path.GetFileName(file);

            return await UploadStreamAsync<TResponse>(
                client, url, partKey,
                fileStream, fileName, fileMediaType, boundary,
                formData, method, queryString, headers);
        }

        /// <summary>
        /// 上传，流方式
        /// </summary>
        /// <typeparam name="TResponse">应答类型</typeparam>
        /// <param name="client">Http客户端</param>
        /// <param name="url">上传接口地址</param>
        /// <param name="partKey">上传内容部分名称</param>
        /// <param name="fileStream">流</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="fileMediaType">文件MIME类型</param>
        /// <param name="boundary">分割符</param>
        /// <param name="formData">附加的表单数据</param>
        /// <param name="method">HTTP请求方法</param>
        /// <param name="queryString">查询字符串</param>
        /// <param name="headers">附加头</param>
        /// <returns>应答</returns>
        public static async Task<TResponse> UploadStreamAsync<TResponse>(
            this HttpClient client,
            string url,
            string partKey,
            Stream fileStream,
            string fileName,
            string fileMediaType = null,
            string boundary = null,
            Dictionary<string, string> formData = null,
            string method = "POST",
            NameValueCollection queryString = null,
            NameValueCollection headers = null)
        {
            
            if (string.IsNullOrEmpty(boundary))
            {
                boundary = $"--------------------------{DateTime.Now.Ticks}";
            }
            if (string.IsNullOrEmpty(partKey))
            {
                partKey = "\"\"";
            }
            else
            {
                partKey = $"\"{partKey}\"";
            }

            url = CreateUrl(url, queryString);
            HttpMethod hm = new HttpMethod(method);
            HttpRequestMessage request = new HttpRequestMessage(hm, url);

            var content = new MultipartFormDataContent(boundary);
            content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
            if (formData != null)
            {
                foreach (var kv in formData)
                {
                    content.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            StreamContent fileContent = new StreamContent(fileStream);

            
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = partKey,
            };
            // 跨平台兼容
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", $"\"{WebUtility.UrlEncode(fileName)}\""));
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename*", $"\"UTF-8''{WebUtility.UrlEncode(fileName)}\""));

            
            if (!string.IsNullOrEmpty(fileMediaType))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(fileMediaType);
            }

            content.Add(fileContent);

            request.Content = content;

            MergeHttpHeaders(request, headers);

            var response = await client.SendAsync(request);
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }

        private static async Task<HttpRequestMessage> createRequestMessage(string method, string url, object body,NameValueCollection queryString=null, NameValueCollection headers = null, string contentType = "application/json")
        {
            url = CreateUrl(url, queryString);
            HttpMethod hm = new HttpMethod(method);

            if (body!=null && body is HttpRequestMessage msg)
            {
                msg.RequestUri = new Uri(url);
                msg.Method = hm;
                MergeHttpHeaders(msg, headers);
                return msg;
            }

            
            HttpRequestMessage request = new HttpRequestMessage(hm, url);
            await request.WriteObjectAsync(body);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            MergeHttpHeaders(request, headers);
            return request;
        }



        private static void MergeHttpHeaders(HttpRequestMessage request, NameValueCollection headers)
        {
            if(  request == null || headers == null)
            {
                return;
            }

            var keys = headers.AllKeys;
            foreach(string k in keys)
            {
                request.Headers.Add(k, headers[k]);
            }
        }





        /// <summary>
        /// 获取url
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="qs">查询字符串</param>
        /// <returns>组装后的url</returns>
        public static string CreateUrl(string url, NameValueCollection qs)
        {
            if (qs != null && qs.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                List<string> kl = qs.AllKeys.ToList();
                foreach (string k in kl)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(k).Append("=");
                    if (!String.IsNullOrEmpty(qs[k]))
                    {

                        sb.Append(System.Net.WebUtility.UrlEncode(qs[k]));
                    }
                }


                if (url.Contains("?"))
                {
                    url = url + "&" + sb.ToString();
                }
                else
                {
                    url = url + "?" + sb.ToString();
                }
            }

            return url;

        }


    }
}
