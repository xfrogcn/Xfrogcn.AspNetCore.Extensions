using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class ClientCertificateProvider : IClientCertificateProvider
    {
        readonly ClientCertificateOptions _options;
        readonly ILoggerFactory _loggerFactory;
        readonly IHttpClientFactory _httpFactory;
        readonly ILogger<ClientCertificateProvider> _logger;
        readonly IServiceProvider _serviceProvider;
        readonly ConcurrentDictionary<string, ClientCertificateManager> _cache = new ConcurrentDictionary<string, ClientCertificateManager>();

        static readonly Action<ILogger, string, string, string, Exception> _logGetClient =
            LoggerMessage.Define<string, string, string>(LogLevel.Debug, new EventId(20, "GetClient"), "获取客户端：clientId: {clientId}, clientName: {clientName}, url: {url}");

        public ClientCertificateProvider(
            IOptions<ClientCertificateOptions> options,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpFactory,
            IServiceProvider serviceProvider,
            ILogger<ClientCertificateProvider> logger)
        {
            _options = options.Value;
            _loggerFactory = loggerFactory;
            _httpFactory = httpFactory;
            _serviceProvider = serviceProvider;

            _logger = logger;

            if (_options != null && _options.ClientList != null)
            {
                _logger.LogDebug($"认证中心Url：{_options.DefaultUrl ?? ""}");
                foreach (var ci in _options.ClientList)
                {
                    _logger.LogDebug($"客户端：id: {ci.ClientID} name: {ci.ClientName ?? ""} url: {ci.AuthUrl ?? ""}");
                }
            }

        }

        public ClientCertificateManager GetClientCertificateManager(string clientId)
        {
            _logger.LogDebug($"获取客户端管理器：{clientId??""}");
            var cm = _cache.GetOrAdd(clientId, (id) =>
            {
                var client =_options.ClientList.FirstOrDefault(c => c.ClientID == id);
                if( client == null)
                {
                    return null;
                }
                if (String.IsNullOrEmpty(client.AuthUrl))
                {
                    client.AuthUrl = _options.DefaultUrl;
                }

                TokenCacheManager cacheManager = null;
                if(client.TokenCacheManager != null)
                {
                    cacheManager = client.TokenCacheManager(_serviceProvider, clientId);
                }
                else
                {
                    cacheManager = TokenCacheManager.MemoryCacheFactory(_serviceProvider, clientId); 
                }

                return new ClientCertificateManager(
                    client,
                    client.Processor ?? CertificateProcessor.OIDC,
                    client.TokenSetter ?? SetTokenProcessor.Bearer,
                    client.ResponseChecker ?? CheckResponseProcessor.NormalChecker,
                    cacheManager,
                    _loggerFactory.CreateLogger<ClientCertificateManager>(), 
                    _httpFactory);
            });

            if(cm == null)
            {
                _logger.LogError($"客户端未配置：{clientId}");
                throw new InvalidOperationException($"客户端未配置：{clientId}");
            }
            else
            {
                _logGetClient(_logger, clientId, cm.Client?.ClientName, cm.Client?.AuthUrl, null);
            }
            return cm;
        }
    }
}
