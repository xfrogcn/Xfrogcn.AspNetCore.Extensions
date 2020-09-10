using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 客户端认证管理器
    /// </summary>
    public class ClientCertificateManager
    {
        public const string HTTP_CLIENT_NAME = nameof(ClientCertificateManager);

        class BearerToken
        {
            public string token_type { get; set; }

            public string access_token { get; set; }

            public long expires_in { get; set; }

            public DateTime LastGetTime { get; set; }

            public bool IsExpired()
            {
                if (((DateTime.UtcNow - LastGetTime).TotalSeconds - expires_in) >= -30)
                {
                    return true;
                }
                return false;
            }
        }

        public string ClientID { get; }

        public string ClientName { get; }

        public string Url { get; }

        public string ClientSecret { get; }

        private readonly ILogger<ClientCertificateManager> _logger;

        private readonly IHttpClientFactory _clientFactory;

        private BearerToken token = null;

        private object locker = new object();

        readonly static Action<ILogger, string, string,string, Exception> _logRequestTokenStart
            = LoggerMessage.Define<string, string, string>(LogLevel.Information, new EventId(10, "RequestTokenStart"), "开始获取令牌：url: {url}, clientId: {clientId}, clientName: {clientName}");
        readonly static Action<ILogger, string, string, string, Exception> _logRequestTokenError
            = LoggerMessage.Define<string, string, string>(LogLevel.Error, new EventId(11, "RequestTokenError"), "获取令牌失败：url: {url}, clientId: {clientId}, clientName: {clientName}");
        readonly static Action<ILogger, long, string, string, string,string, Exception> _logRequestTokenEnd
            = LoggerMessage.Define<long,string, string, string, string>(LogLevel.Information, new EventId(12, "RequestTokenEnd"), "获取令牌完成：cost: {cost}ms, url: {url}, clientId: {clientId}, clientName: {clientName}, token: {token}");
        readonly static Action<ILogger,  string, string, string, Exception> _logAutoGetToken
            = LoggerMessage.Define<string, string, string>(LogLevel.Information, new EventId(13, "AutoGetToken"), "自动获取令牌： url: {url}, clientId: {clientId}, clientName: {clientName}");
        readonly static Action<ILogger, string, string, string, Exception> _logAutoGetTokenError
            = LoggerMessage.Define<string, string, string>(LogLevel.Information, new EventId(14, "AutoGetTokenError"), "自动获取令牌异常： url: {url}, clientId: {clientId}, clientName: {clientName}");

        public ClientCertificateManager(
            string url, 
            string clientId, 
            string clientName,
            string clientSecret, 
            ILogger<ClientCertificateManager> logger,
            IHttpClientFactory clientFactory)
        {
            Url = url;
            ClientID = clientId;
            ClientName = clientName;
            ClientSecret = clientSecret;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        private CancellationTokenSource cts = null;
        public async Task<string> GetAccessToken()
        {
            if (token == null || token.IsExpired())
            {
                bool isWait = false;
                lock (locker)
                {
                    if(cts == null)
                    {
                        cts = new CancellationTokenSource();
                    }
                    else
                    {
                        isWait = true;
                    }
                }

                if(isWait)
                {
                    cts.Token.WaitHandle.WaitOne();
                    return token?.access_token;
                }

                Dictionary<String, string> dic = new Dictionary<string, string>()
                    {
                        {"grant_type","client_credentials" },
                        {"client_id", ClientID },
                        {"client_secret", ClientSecret }
                    };
                var httpClient = _clientFactory.CreateClient(HTTP_CLIENT_NAME);
                Stopwatch sw = new Stopwatch();
                _logRequestTokenStart(_logger, Url, ClientID, ClientName ?? "", null);
                try
                {
                    httpClient.BaseAddress = new Uri(Url);
                    BearerToken bt = await httpClient.SubmitFormAsync<BearerToken>("/connect/token", dic);
                    if (bt != null && !String.IsNullOrEmpty(bt.access_token))
                    {
                        bt.LastGetTime = DateTime.UtcNow;
                    }

                    token = bt;
                }
                catch (Exception e)
                {
                    _logRequestTokenError(_logger, Url, ClientID, ClientName, e);
                }
                sw.Stop();
                _logRequestTokenEnd(_logger, sw.ElapsedMilliseconds, Url, ClientID, ClientName ?? "", token?.access_token, null);
                cts.Cancel();
                cts = null;
            }

            return token?.access_token;
        }

        public void ClearToken()
        {
            lock (locker)
            {
                token = null;
            }
        }



        public async Task<TResult> Execute<TResult>(Func<string, Task<TResult>> fun)
        {
            TResult result = default(TResult);
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    string accessToken = await GetAccessToken();
                    if (String.IsNullOrEmpty(accessToken))
                    {
                        continue;
                    }

                    result = await fun(accessToken);
                    break;
                }
                catch (UnauthorizedAccessException e)
                {
                    ClearToken();
                    _logAutoGetToken(_logger, Url, ClientID, ClientName ?? "", null);
                }
                catch (Exception e)
                {
                    _logAutoGetTokenError(_logger, Url, ClientID, ClientName ?? "", e);
                    throw;
                }

            }

            return result;
        }

    }
}
