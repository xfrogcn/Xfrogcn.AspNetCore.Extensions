using Extensiions.RedisQueueProducer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xfrogcn.AspNetCore.Extensiions;
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
