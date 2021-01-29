using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.RedisQueueProducer
{
    public class QueueProducer<TEntity> : IParallelQueueProducer<TEntity>
    {
        readonly RedisOptions _redisOptions;
        readonly ILogger<QueueProducer<TEntity>> _logger;
        readonly RedisConnectionManager _connectionManager;
        private IDatabase _cache;
        private readonly string _queueRedisKey;

        private readonly string _instance;
       
        QueueType _queueType;
        ISubscriber sub = null;
        CancellationTokenSource _stopTokenSource = null;
        private Action<RedisChannel, RedisValue> _subAction = null;
        private object _locker = new object();

      
        public QueueProducer(string name, RedisOptions redisOptions, RedisConnectionManager connectionManager, ILogger<QueueProducer<TEntity>> logger)
        {
            _redisOptions = redisOptions;
            _connectionManager = connectionManager;

      
            _logger = logger;
            _instance = _redisOptions.InstanceName ?? string.Empty;
            _queueRedisKey = $"{_instance}{name}";
            _queueType = _redisOptions.QueueType;
            if(_queueType == QueueType.Bag)
            {
                _queueType = QueueType.FIFO;
            }
            _stopTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> TryAddAsync(TEntity entity, CancellationToken token)
        {
            bool isOk = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await ConnectAsync(token);

                    var bytes = entity.GetBytes();

                    _cache.ListLeftPush(
                        _queueRedisKey,
                        bytes);

                    await sub.PublishAsync(_queueRedisKey + "_msg", "1");
                    isOk = true;
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "TryAddAsync异常重试");
                }
            }
           
            return isOk;
        }

        class CacheItem
        {
            public CancellationTokenSource cts { get; set; }

            public TEntity entity { get; set; }

            public bool IsOk { get; set; }

        }
        
        public async Task<(TEntity, bool)> TryTakeAsync(TimeSpan timeout, CancellationToken token)
        {
           
            await ConnectAsync(token);

            CancellationTokenSource cts = new CancellationTokenSource(timeout);
            cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
            CacheItem ci = new CacheItem()
            {
                cts = cts,
            };
            _readQueue.Add(ci);
           
            cts.Token.WaitHandle.WaitOne();
           

            return (ci.entity,ci.IsOk);
        }

        private void handleSubscribe(RedisChannel channel, RedisValue val)
        {
            lock (_locker)
            {
               if(_waitQueue != null)
                {
                    _waitQueue.Cancel();
                }
            }
        }

        protected virtual async Task<(TEntity, bool)> InnerTryTakeAsync()
        {
            RedisValue item;
            if (_queueType == QueueType.FIFO)
            {
                item = await _cache.ListRightPopAsync(_queueRedisKey);
            }
            else
            {
                item = await _cache.ListLeftPopAsync(_queueRedisKey);
            }

            if (item.HasValue)
            {
                byte[] bytes = item;

                TEntity entity = bytes.GetEntity<TEntity>();
                return (entity, true);
            }

            return (default(TEntity), false);
        }

        private async Task ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            if (_cache != null)
            {
                return;
            }

            var connection = await _connectionManager.ConnectAsync(_redisOptions, token);
            if (connection == null)
            {
                _logger.LogWarning($"链接失败，返回null: {_queueRedisKey}");
                return;
            }
            if (_cache != null)
            {
                return;
            }

            _cache = connection.GetDatabase();
            // 设置
            if (_subAction == null)
            {
                sub = connection.GetSubscriber();
                _subAction = handleSubscribe;
                sub.Subscribe(_queueRedisKey + "_msg", _subAction);
                _ = Task.Run(() =>
                {
                    _ = reader(_stopTokenSource.Token);
                });
            }
        }

        CancellationTokenSource _stoppingTokenSource = null;
        public Task StopAsync(CancellationToken token)
        {
            _stoppingTokenSource = new CancellationTokenSource();
            _stopTokenSource.Cancel();
            _stoppingTokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
            // 清除队列
            while (_readQueue.Count > 0)
            {
                var ci = _readQueue.Take();
                ci.IsOk = false;
                ci.cts.Cancel();
            }

            return Task.CompletedTask;
        }


        //private int GetRedisOptionsHashCode()
        //{
        //    string cfg = "";
        //    if (_redisOptions.ConfigurationOptions != null)
        //    {
        //        cfg = System.Text.Json.JsonSerializer.Serialize(_redisOptions.ConfigurationOptions);
        //    }
        //    else
        //    {
        //        cfg = _redisOptions.Configuration;
        //    }
        //    return cfg.GetHashCode();
        //}

        BlockingCollection<CacheItem> _readQueue = new BlockingCollection<CacheItem>();
        CancellationTokenSource _waitQueue = null;
        CancellationTokenSource _queueNotify = null;
        private async Task reader(CancellationToken cancellationToken)
        {
            bool needReadCache = true;
            CacheItem lastCacheItem = null;
            _logger.LogInformation($"Redis 读取队列已启动, {_queueRedisKey}");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    CacheItem ci = null;
                    if (needReadCache)
                    {
                        if (!_readQueue.TryTake(out ci, -1, cancellationToken))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        ci = lastCacheItem;
                    }

                    // 已经被取消
                    if (ci.cts.IsCancellationRequested)
                    {
                        continue;
                    }


                    // 读取缓存
                    var (entity, isOk) = await InnerTryTakeAsync();
                    if (isOk)
                    {
                        ci.entity = entity;
                        ci.IsOk = true;
                        ci.cts.Cancel();
                        needReadCache = true;
                        lastCacheItem = null;
                    }
                    else
                    {
                        // 等待信号 (有队列数据或者过期）
                        lock (_locker)
                        {
                            _queueNotify = new CancellationTokenSource();
                            _waitQueue = CancellationTokenSource.CreateLinkedTokenSource(
                                _queueNotify.Token,
                                ci.cts.Token);
                        }
                        if (_waitQueue != null)
                        {
                            _waitQueue.Token.WaitHandle.WaitOne();
                        }
                        // 读取过期
                        if (ci.cts.IsCancellationRequested)
                        {
                            continue;
                        }
                        else
                        {
                            needReadCache = false;
                            lastCacheItem = ci;
                        }

                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Redis队列接收异常: {_queueRedisKey}");
                }
            }
            _logger.LogInformation($"Redis 读取队列已停止, {_queueRedisKey}");
            if (_stoppingTokenSource!=null)
            {
                _stoppingTokenSource.Cancel();
            }
        }
    }
}
