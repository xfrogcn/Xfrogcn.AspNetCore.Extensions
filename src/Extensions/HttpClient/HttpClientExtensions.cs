using System.Collections.Generic;
using System.Collections.Specialized;
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


        public static async Task<TResponse> PostWithTokenAsync<TResponse>(this HttpClient client, string url, object body, string token, string userId = null, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            HttpRequestMessage request = await createRequestMessage(method, url, body, queryString, headers);
            if (!String.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            }
            if (!String.IsNullOrEmpty(userId))
            {
                request.Headers.Add("User", userId);
            }
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                       response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("验证失败");
            }
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }

        public static async Task<string> PostWithTokenAsync(this HttpClient client, string url, object body, string token, string userId = null, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            return await PostWithTokenAsync<string>(client, url, body, token, userId, method, queryString, headers);
        }


        public static async Task<TResponse> GetWithTokenAsync<TResponse>(this HttpClient client, string url, string token, string userId = "", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            HttpRequestMessage request = await createRequestMessage(HttpMethod.Get.Method, url, null, queryString, headers);
            if (!String.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            }
            if (!String.IsNullOrEmpty(userId))
            {
                request.Headers.Add("User", userId);
            }
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                       response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("验证失败");
            }
            if (!request.IsDisableEnsureSuccessStatusCode())
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.GetObjectAsync<TResponse>();
        }

        public static async Task<string> GetWithTokenAsync(this HttpClient client, string url, string token, string userId = "", NameValueCollection queryString = null, NameValueCollection headers = null)
        {
            return await GetWithTokenAsync<string>(client, url, token, userId, queryString, headers);
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
