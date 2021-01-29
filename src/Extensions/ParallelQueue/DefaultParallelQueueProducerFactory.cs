using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class DefaultParallelQueueProducerFactory : IParallelQueueProducerFactory
    {
        readonly IServiceProvider _serviceProvider;
        private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();
        public DefaultParallelQueueProducerFactory(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IParallelQueueProducer<TEntity> CreateProducer<TEntity>(string queueName)
        {
            return _cache.GetOrAdd(queueName, (key) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var options = scope.ServiceProvider.GetService<IOptionsSnapshot<ParallelQueueProducerOptions<TEntity>>>();
                    if (options == null || options.Value == null)
                    {
                        throw new InvalidOperationException("未配置队列生产者");
                    }
                    var producer = options.Get(queueName).GetCreator();
                    if (producer == null)
                    {
                        throw new InvalidOperationException($"未配置队列:{queueName}");
                    }
                    return producer(scope.ServiceProvider, queueName);
                }
            }) as IParallelQueueProducer<TEntity>;

        }
    }
}
