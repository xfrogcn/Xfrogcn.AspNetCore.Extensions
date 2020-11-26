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

        public ClientCertificateInfo Client { get; }
        private readonly ILogger<ClientCertificateManager> _logger;

        private readonly IHttpClientFactory _clientFactory;

        private readonly CertificateProcessor _processor = null;
        private readonly SetTokenProcessor _tokenSetter = null;
        private readonly TokenCacheManager _cacheManager = null;
        private readonly CheckResponseProcessor _responseChecker = null;


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
            ClientCertificateInfo client,
            CertificateProcessor processor,
            SetTokenProcessor tokenSetter,
            CheckResponseProcessor responseChecker,
            TokenCacheManager cacheManager,
            ILogger<ClientCertificateManager> logger,
            IHttpClientFactory clientFactory)
        {
            Client = client;
            _processor = processor;
            _tokenSetter = tokenSetter;
            _responseChecker = responseChecker;
            _cacheManager = cacheManager;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        
        public async Task<string> GetAccessToken()
        {
            // 从缓存中获取token
            TokenCache token = null;
            try
            {
                token = await _cacheManager.GetToken();
            }
            catch
            {
                try
                {
                    await _cacheManager.RemoveToken();
                }
                catch { }
            }
           
            
            if (token == null || token.IsExpired())
            {
                bool isProcessWithThis = await _cacheManager.Enter();
                if (!isProcessWithThis)
                {
                    _logger.LogInformation($"由其他管理器处理, {Client.ClientID}");
                    token = await _cacheManager.GetToken();
                    return token?.access_token;
                }

                //
                Stopwatch sw = new Stopwatch();
                _logRequestTokenStart(_logger, Client.AuthUrl, Client.ClientID, Client.ClientName ?? "", null);
                try
                {
                    // 获取Token
                    DateTime dt = DateTime.Now;
                    var tokenResponse = await _processor.GetToken(Client, _clientFactory);
                    if(tokenResponse!=null)
                    {
                        token = new TokenCache()
                        {
                            access_token = tokenResponse.access_token,
                            token_type = tokenResponse.token_type,
                            expires_in = tokenResponse.expires_in,
                            LastGetTime = dt
                        };
                        await _cacheManager.SetToken(token);
                    }

                }
                catch (Exception e)
                {
                    _logRequestTokenError(_logger, Client.AuthUrl, Client.ClientID, Client.ClientName, e);
                }
                finally
                {
                    await _cacheManager.Exit();
                }
                sw.Stop();
                _logRequestTokenEnd(_logger, sw.ElapsedMilliseconds, Client.AuthUrl, Client.ClientID, Client.ClientName ?? "", token?.access_token, null);

            }

            return token?.access_token;
        }

        public async Task ClearToken()
        {
            await _cacheManager.RemoveToken();
        }



        public async Task<TResult> Execute<TResult>(Func<string, SetTokenProcessor, CheckResponseProcessor, Task<TResult>> fun)
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

                    result = await fun(accessToken, _tokenSetter, _responseChecker);
                    break;
                }
                catch (UnauthorizedAccessException e)
                {
                    await ClearToken();
                    _logAutoGetToken(_logger, Client.AuthUrl, Client.ClientID, Client.ClientName ?? "", e);
                }
                catch (Exception e)
                {
                    _logAutoGetTokenError(_logger, Client.AuthUrl, Client.ClientID, Client.ClientName ?? "", e);
                    throw;
                }

            }

            return result;
        }

    }
}
