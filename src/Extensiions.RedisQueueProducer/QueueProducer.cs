using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensiions;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;

namespace Extensiions.RedisQueueProducer
{
    public class QueueProducer<TEntity> : IParallelQueueProducer<TEntity>
    {
        readonly RedisOptions _redisOptions;
        private volatile ConnectionMultiplexer _connection;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private IDatabase _cache;
        private readonly string _queueRedisKey;

        private readonly string _instance;
        QueueType _queueType;

        public QueueProducer(string name, RedisOptions redisOptions)
        {
            _redisOptions = redisOptions;
            _instance = _redisOptions.InstanceName ?? string.Empty;
            _queueRedisKey = $"{_instance}{name}";
            _queueType = _redisOptions.QueueType;
            if(_queueType == QueueType.Bag)
            {
                _queueType = QueueType.FIFO;
            }

        }

        public async Task<bool> TryAddAsync(TEntity entity, CancellationToken token)
        {
            await ConnectAsync(token);

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, entity);
            ms.Position = 0;
            BinaryReader br = new BinaryReader(ms);
            var bytes = br.ReadBytes((int)ms.Length);

            _cache.ListLeftPush(
                _queueRedisKey,
                bytes);

            ISubscriber sub = _connection.GetSubscriber();
            await  sub.PublishAsync(_queueRedisKey+"_msg", "1");
            return true;
        }

        class CacheItem {
            public CancellationTokenSource cts { get; set; }

            public TEntity entity { get; set; }

            public Action Callback { get; set; }

            public bool IsOk { get; set; }
        }

        private Action<RedisChannel, RedisValue> _subAction = null;
        private List<CacheItem> ctsQueue = new List<CacheItem>();
        private object _locker = new object();
        public long _dirCount = 0;
        public async Task<(TEntity, bool)> TryTakeAsync(TimeSpan timeout, CancellationToken token)
        {
            await ConnectAsync(token);

            bool isNoneData = false;
            // isLooping, 订阅处理过程正在处理
            if (_waitRequest == null && !_isLooping)
            {
                var (entity, isOK) = await InnerTryTakeAsync();
                if (isOK)
                {
                    _dirCount++;
                    return (entity, isOK);
                }
                isNoneData = true;
            }

            CancellationTokenSource cts = new CancellationTokenSource(timeout);
            CacheItem ci = new CacheItem()
            {
                cts = cts,
            };
            lock (_locker)
            {
                ci.Callback = () =>
                {
                    lock (_locker)
                    {
                        ctsQueue.Remove(ci);
                    }
                };

               
                cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

                if (_subAction == null)
                {
                    // 只允许一个订阅通道
                    _subAction = handleSubscribe;
                    ISubscriber sub = _connection.GetSubscriber();
                    sub.Subscribe(_queueRedisKey + "_msg", _subAction);
                }
                ctsQueue.Add(ci);

                if (_waitRequest != null)
                {
                    _waitRequest.Cancel();
                    _waitRequest = null;
                }
            }

            if(!isNoneData && !_isLooping && _waitRequest == null)
            {
                ISubscriber sub = _connection.GetSubscriber();
                await sub.PublishAsync(_queueRedisKey + "_msg", "1");
            }

            cts.Token.WaitHandle.WaitOne();
            if (!ci.IsOk)
            {
                ci.Callback();
            }

            return (ci.entity,ci.IsOk);
        }

        private CancellationTokenSource _waitRequest = null;
        private bool _isLooping = false;
        private void handleSubscribe(RedisChannel channel, RedisValue val)
        {
           
            while (true)
            {
                lock (_locker)
                {
                    _isLooping = true;
                    if (ctsQueue.Count == 0)
                    {
                        _isLooping = false;
                        break;
                    }
                }
                var (entity, isOK) = InnerTryTakeAsync().Result;
                if (!isOK)
                {
                    // 被其他机器获取了
                    break;
                }
                
                while (true)
                {
                    CacheItem ci = null;
                    lock (_locker)
                    {
                        while (ctsQueue.Count > 0)
                        {
                            ci = ctsQueue[0];
                            ctsQueue.Remove(ci);
                            if (ci.cts.IsCancellationRequested)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if(ci == null)
                        {
                            lock (_locker)
                            {
                                _waitRequest = new CancellationTokenSource();
                            }
                        }
                    }
                    if (ci != null)
                    {
                        ci.IsOk = true;
                        ci.entity = entity;
                        ci.cts.Cancel();
                        break;
                    }
                    else
                    {
                        //
                        _waitRequest.Token.WaitHandle.WaitOne();
                    }
                }

            }
            lock (_locker)
            {
                _isLooping = false;
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
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(bytes);
                TEntity entity = (TEntity)bf.Deserialize(ms);
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

            await _connectionLock.WaitAsync(token);
            try
            {
                if (_cache == null)
                {
                    if (_redisOptions.ConfigurationOptions != null)
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_redisOptions.ConfigurationOptions);
                    }
                    else
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_redisOptions.Configuration);
                    }
                    _cache = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }
    }
}
