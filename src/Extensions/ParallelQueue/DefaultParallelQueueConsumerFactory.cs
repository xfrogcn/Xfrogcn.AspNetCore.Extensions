using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    /// <summary>
    /// 默认并行队列处理器工厂
    /// </summary>
    public class DefaultParallelQueueConsumerFactory : IParallelQueueConsumerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;

        class ConsumerCacheItem : IEqualityComparer<ConsumerCacheItem>
        {
            public Type EntityType { get; set; }

            public Type StateType { get; set; }

            public string Name { get; set; }


            public bool Equals([AllowNull] ConsumerCacheItem x, [AllowNull] ConsumerCacheItem y)
            {
                if (x == y)
                {
                    return true;
                }
                if (x == null || y == null)
                {
                    return false;
                }

                if (x.EntityType == y.EntityType &&
                    x.StateType == y.StateType &&
                    (x.Name ?? "").Equals(y.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode([DisallowNull] ConsumerCacheItem obj)
            {
                string str = $"{obj.EntityType.FullName}-{obj.StateType.FullName}-{obj.Name ?? ""}".ToLower();

                return str.GetHashCode();
            }
        }

        private ConcurrentDictionary<ConsumerCacheItem, object> _cache = new ConcurrentDictionary<ConsumerCacheItem, object>(new ConsumerCacheItem());


        public DefaultParallelQueueConsumerFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }

        public IParallelQueueConsumer<TEntity, TState> CreateConsumer<TEntity, TState>(string name, TState state)
        {
            ConsumerCacheItem cacheKey = new ConsumerCacheItem()
            {
                EntityType = typeof(TEntity),
                StateType = typeof(TState),
                Name = name??""
            };

            var instance = _cache.GetOrAdd(cacheKey, (key, sp) =>
            {
                using (var scope = sp.CreateScope())
                {
                    var optionsManager = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ParallelQueueConsumerOptions<TEntity, TState>>>();
                    var options = optionsManager.Get(name);
                    if (options.ExecuteDelegate == null)
                    {
                        throw new InvalidOperationException("必须设置执行委托ExecuteDelegate");
                    }
                    DefaultParallelQueueConsumer<TEntity, TState> consumer = new DefaultParallelQueueConsumer<TEntity, TState>(
                        options,
                        name ?? "",
                        state,
                        _loggerFactory
                        );
                    return consumer;
                }
            }, _serviceProvider);


            return (IParallelQueueConsumer<TEntity, TState>)instance;

        }
    }
}
