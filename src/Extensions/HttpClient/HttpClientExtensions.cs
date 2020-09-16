using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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

        public static async Task<TResponse> SubmitFormAsync<TResponse>(this HttpClient client, string url, Dictionary<string, string> formData, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            url = CreateUrl(url, queryString);
            HttpMethod hm = new HttpMethod(method);
            HttpRequestMessage request = new HttpRequestMessage(hm, url);
            request.Content = new FormUrlEncodedContent(formData);
            
            MergeHttpHeaders(request, headers);

            var response = await client.SendAsync(request);
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }
        public static async Task<string> SubmitFormAsync(this HttpClient client, string url, Dictionary<string, string> formData, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            return await SubmitFormAsync<string>(client, url, formData, method, queryString, headers);
        }


        public static async Task<TResponse> UploadFile<TResponse>(
            this HttpClient client, 
            string url, 
            string fileKey,
            string file,
            Dictionary<string, string> formData = null, 
            string method = "POST", 
            NameValueCollection queryString = null, 
            NameValueCollection headers = null)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException(file);
            }

            url = CreateUrl(url, queryString);
            HttpMethod hm = new HttpMethod(method);
            HttpRequestMessage request = new HttpRequestMessage(hm, url);

            var content = new MultipartFormDataContent();
            if (formData != null)
            {
                foreach(var kv in formData)
                {
                    content.Add(new StringContent(kv.Value), kv.Key);
                }
            }

            FileStream fileStream = new FileStream(file, FileMode.Open);
            BinaryReader br = new BinaryReader(fileStream);
            ByteArrayContent fileContent = new ByteArrayContent(br.ReadBytes((int)fileStream.Length));
            content.Add(fileContent, fileKey, Path.GetFileName(file));

            //var fileContent = new StreamContent(fileStream);
            //fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            //{
            //    Name = fileKey,
            //    FileName = Path.GetFileName(file),
            //    Size = fileStream.Length,
            //};
            //content.Add(fileContent, fileKey);

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
