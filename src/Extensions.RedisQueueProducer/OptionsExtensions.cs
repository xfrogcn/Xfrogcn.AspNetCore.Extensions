﻿using System;
using Microsoft.Extensions.Logging;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;
using Xfrogcn.AspNetCore.Extensions.RedisQueueProducer;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                ILogger<QueueProducer<TEntity>> logger = sp.GetRequiredService<ILogger<QueueProducer<TEntity>>>();
                RedisConnectionManager connManager = sp.GetRequiredService<RedisConnectionManager>();
                return new QueueProducer<TEntity>(name, ro, connManager, logger );
            });
            return options;
        }
    }
}
