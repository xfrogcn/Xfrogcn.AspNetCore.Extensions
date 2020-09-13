﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 设置令牌的处理器
    /// </summary>
    public abstract class SetTokenProcessor
    {
        public abstract Task SetTokenAsync(HttpRequestMessage request, string token);


        public static readonly SetTokenProcessor Bearer = new SetBearerTokenProcessor();

        public static readonly SetTokenProcessor QueryString = new QueryStringTokenProcessor();

        public static DelegateSetTokenProcessor CreateDelegateSetTokenProcessor(Func<HttpRequestMessage, string, Task> setter)
        {
            return new DelegateSetTokenProcessor(setter);
        }


        /// <summary>
        /// Bearer令牌
        /// </summary>
        public class SetBearerTokenProcessor : SetTokenProcessor
        {
            public override Task SetTokenAsync(HttpRequestMessage request, string token)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return Task.CompletedTask;

            }
        }

        /// <summary>
        /// 通过查询字符串传递访问令牌
        /// </summary>
        public class QueryStringTokenProcessor : SetTokenProcessor
        {
            private readonly string _queryKey = "";
            public QueryStringTokenProcessor() : this("access_token")
            {

            }

            public QueryStringTokenProcessor(string queryKey)
            {
                _queryKey = queryKey;
                if (string.IsNullOrEmpty(_queryKey))
                {
                    _queryKey = "access_token";
                }
            }

            public override Task SetTokenAsync(HttpRequestMessage request, string token)
            {
                UriBuilder ub = new UriBuilder(request.RequestUri);
                string tokenQuery = $"{_queryKey}={WebUtility.UrlEncode(token)}"; 
                if (string.IsNullOrEmpty(ub.Query))
                {
                    ub.Query = tokenQuery;
                }
                else
                {
                    ub.Query = $"{ub.Query}&{tokenQuery}";
                }
                request.RequestUri = ub.Uri;

                return Task.CompletedTask;
            }
        }

        public class DelegateSetTokenProcessor : SetTokenProcessor
        {
            private readonly Func<HttpRequestMessage, string, Task> _setter;
            public DelegateSetTokenProcessor(Func<HttpRequestMessage, string, Task> setter)
            {
                _setter = setter;
            }

            public override Task SetTokenAsync(HttpRequestMessage request, string token)
            {
                if (_setter!=null)
                {
                    return _setter(request, token);
                }
                return Task.CompletedTask;
            }
        }
    }
}
