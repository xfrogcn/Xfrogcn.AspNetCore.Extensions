using Extensions.RedisQueueProducer;
using System;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptionsExtensions
    {
        public static ParallelQueueProducerOptions<TEntity> UseRedis<TEntity>(
            this ParallelQueueProducerOptions<TEntity> options,
            Action<RedisOptions> redisOptions)
        {
            options.SetProducer((sp, name) =>
            {
                RedisOptions ro = new RedisOptions();
                redisOptions?.Invoke(ro);
                return new QueueProducer<TEntity>(name, ro );
            });
            return options;
        }
    }
}
