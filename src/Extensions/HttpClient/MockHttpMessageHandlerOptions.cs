using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class MockHttpMessageHandlerOptions
    {
        internal class MockItem
        {
            /// <summary>
            /// 先决条件
            /// </summary>
            public Func<HttpRequestMessage, bool> Predicate { get; set; }
            /// <summary>
            /// 处理
            /// </summary>
            public Func<HttpRequestMessage, HttpResponseMessage, Task> Proc { get; set; }
        }

        private readonly List<MockItem> _mockList = new List<MockItem>();

        private readonly JsonHelper _jsonHelper = new JsonHelper();


        internal IReadOnlyList<MockItem> MockList => _mockList;


        private Func<HttpRequestMessage, bool> _urlPredicate(string url, HttpMethod method)
        {
            Func<HttpRequestMessage, bool> _predicate = (msg) =>
            {
                if( url == "*")
                {
                    return true;
                }
                return msg.RequestUri.AbsolutePath.ToLower() == url.ToLower() && msg.Method == method;
            };
            return _predicate;
        }


        public MockHttpMessageHandlerOptions AddMock(string url, HttpMethod method,  Func<HttpRequestMessage, HttpResponseMessage, Task> proc)
        {
            Func<HttpRequestMessage, bool> p = _urlPredicate(url, method);
            return AddMock(p, proc);
        }


        public MockHttpMessageHandlerOptions AddMock(string url, HttpMethod method, HttpStatusCode statusCode)
        {
            Func<HttpRequestMessage, bool> p = _urlPredicate(url, method);
            Func<HttpRequestMessage, HttpResponseMessage, Task> proc = (r, response) =>
            {
                response.StatusCode = statusCode;
                return Task.CompletedTask;
            };
            return AddMock(p, proc);
            
        }

        public MockHttpMessageHandlerOptions AddMock(string url, HttpMethod method, Exception exception)
        {
            Func<HttpRequestMessage, bool> p = _urlPredicate(url, method);
            Func<HttpRequestMessage, HttpResponseMessage, Task> proc = (r, response) =>
            {
                throw exception;
            };
            return AddMock(p, proc);
        }

        public MockHttpMessageHandlerOptions AddMock<TObject>(string url, HttpMethod method, TObject obj, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Func<HttpRequestMessage, bool> p = _urlPredicate(url, method);
            Func<HttpRequestMessage, HttpResponseMessage, Task> proc = (r, response) =>
            {
                response.StatusCode = statusCode;
                response.Content = new StringContent(obj == null ? "" : _jsonHelper.ToJson(obj));
                return Task.CompletedTask;
            };
            return AddMock(p, proc);
        }

        public MockHttpMessageHandlerOptions AddMock(string url, HttpMethod method, string str, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Func<HttpRequestMessage, bool> p = _urlPredicate(url, method);
            Func<HttpRequestMessage, HttpResponseMessage, Task> proc = (r, response) =>
            {
                response.StatusCode = statusCode;
                response.Content = new StringContent(str);
                return Task.CompletedTask;
            };
            return AddMock(p, proc);
        }



        public MockHttpMessageHandlerOptions AddMock(Func<HttpRequestMessage, bool> predicate, Func<HttpRequestMessage, HttpResponseMessage, Task> proc)
        {
            if( predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if( proc == null)
            {
                throw new ArgumentNullException(nameof(proc));
            }
            MockItem mi = new MockItem()
            {
                Predicate = predicate,
                Proc = proc
            };
            _mockList.Add(mi);
            return this;
        }

    }
}
