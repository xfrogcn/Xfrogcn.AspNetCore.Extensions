using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Xfrogcn.AspNetCore.Extensions.RedisQueueProducer
{
    public class RedisConnectionManager
    {
        readonly ILogger<RedisConnectionManager> _logger;
        private static volatile ConcurrentDictionary<int, ConnectionMultiplexer> _connections = new ConcurrentDictionary<int, ConnectionMultiplexer>();
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _connectionLocks = new ConcurrentDictionary<int, SemaphoreSlim>();

        public RedisConnectionManager(ILogger<RedisConnectionManager> logger)
        {
            _logger = logger;
        }

        public async Task<ConnectionMultiplexer> ConnectAsync(RedisOptions redisOptions, CancellationToken token)
        {
            int _hashCode = GetRedisOptionsHashCode(redisOptions);

            token.ThrowIfCancellationRequested();

            ConnectionMultiplexer connection = null;
            _connections.TryGetValue(_hashCode, out connection);
            if (connection != null)
            {
                return connection;
            }

            var connectionLock = _connectionLocks.GetOrAdd(_hashCode, k =>
            {
                return new SemaphoreSlim(initialCount: 1, maxCount: 1);
            });

            await connectionLock.WaitAsync(token);
            try
            {
                _connections.TryGetValue(_hashCode, out connection);
                if (connection != null)
                {
                    return connection;
                }

                if (redisOptions.ConfigurationOptions != null)
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.ConfigurationOptions);
                }
                else
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(redisOptions.Configuration);
                }
                _logger.LogInformation("创建新的Redis链接");
                _connections.TryAdd(_hashCode, connection);

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Connect Error");
            }
            finally
            {
                connectionLock.Release();
            }
            return connection;
        }

        private int GetRedisOptionsHashCode(RedisOptions redisOptions)
        {
            string cfg = "";
            if (redisOptions.ConfigurationOptions != null)
            {
                cfg = System.Text.Json.JsonSerializer.Serialize(redisOptions.ConfigurationOptions);
            }
            else
            {
                cfg = redisOptions.Configuration;
            }
            return cfg.GetHashCode();
        }

    }
}
