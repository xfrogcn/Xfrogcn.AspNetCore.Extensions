using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// Token缓存管理
    /// </summary>
    public abstract class TokenCacheManager
    {
        public string ClientID { get; protected set; }
        public TokenCacheManager(string clientId)
        {
            ClientID = clientId;
        }

        public abstract Task<TokenCache> GetToken();

        /// <summary>
        /// 进入获取令牌过程
        /// </summary>
        /// <returns>如果是此管理器获取token，返回true，否则返回false，表示token正在由其他管理器获取</returns>
        public abstract Task<bool> Enter();

        public abstract Task Exit();


        public abstract Task SetToken(TokenCache cache);

        public abstract Task RemoveToken();


        public static Func<IServiceProvider, string, TokenCacheManager> MemoryCacheFactory = (_, clientId) => new MemoryTokenCacheManager(clientId);

        public static Func<IServiceProvider, string, TokenCacheManager> DistributedCacheFactory = (sp, clientId) =>
        {
            IDistributedCache cache = sp.GetRequiredService<IDistributedCache>();
            return new DistributedTokenCacheManager(cache, clientId);
        };
    }


    public class MemoryTokenCacheManager : TokenCacheManager
    {
        public MemoryTokenCacheManager(string clientId):base(clientId)
        {
           
        }

        private TokenCache _cache = null;
        object locker = new object();
        CancellationTokenSource cts = null;

        public override Task<bool> Enter()
        {
            bool isWait = false;
            lock (locker)
            {
                if (cts == null)
                {
                    cts = new CancellationTokenSource();
                }
                else
                {
                    isWait = true;
                }
            }

            if (isWait)
            {
                cts.Token.WaitHandle.WaitOne();
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public override Task Exit()
        {
            if(cts != null)
            {
                cts.Cancel();
                cts = null;
            }
            return Task.CompletedTask;
        }

        public override Task<TokenCache> GetToken()
        {
            return Task.FromResult(_cache);
        }

        public override Task SetToken(TokenCache cache)
        {
            _cache = cache;
            return Task.CompletedTask;
        }

        public override Task RemoveToken()
        {
            _cache = null;
            return Task.CompletedTask;
        }
    }

    public class DistributedTokenCacheManager : TokenCacheManager
    {
        private IDistributedCache _cache;
        public const string CACHE_KEY_PREFIX = "TOKEN_PROVIDER_CACHE_";
        
        public DistributedTokenCacheManager(IDistributedCache distributedCache, string clientId) : base(clientId)
        {
            _cache = distributedCache;
        }

        public override async Task<bool> Enter()
        {
            string value = Guid.NewGuid().ToString("N").ToLower();
            string cacheKey = $"{CACHE_KEY_PREFIX}{ClientID}_ENTER";
            string val = await _cache.GetStringAsync(cacheKey);
           
            if(string.IsNullOrEmpty(val))
            {
                // 没有其他管理器
                await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                });
                // 再次获取值，如果相等
                string eq = await _cache.GetStringAsync(cacheKey);
                if (eq == value)
                {
                    return true;
                }
                else if (string.IsNullOrEmpty(eq))
                {
                    // 极端情况，其他管理器以及处理完成
                    return false;
                }
            }

            // 等待完成
            while (true)
            {
                await Task.Delay(10);
                val = await _cache.GetStringAsync(cacheKey);
                if (string.IsNullOrEmpty(val))
                {
                    break;
                }
            }

            return false;
        }

        public override async Task Exit()
        {
            string cacheKey = $"{CACHE_KEY_PREFIX}{ClientID}_ENTER";

            await _cache.RemoveAsync(cacheKey);
        }

        public override Task<TokenCache> GetToken()
        {
            return _cache.GetAsync<TokenCache>(CacheKey);
        }

        public override async Task SetToken(TokenCache cache)
        {
            await _cache.SetAsync(CacheKey, cache);
        }

        public async override Task RemoveToken()
        {
            await _cache.RemoveAsync(CacheKey);
        }

        protected virtual string CacheKey
        {
            get
            {
                return $"{CACHE_KEY_PREFIX}{ClientID}";
            }
        }
    }
}
