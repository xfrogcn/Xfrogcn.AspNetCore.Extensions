using System;
using System.Collections.Generic;
using System.Text;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryOptionsExtensions
    {
        public static ParallelQueueProducerOptions<TEntity> UseRedis<TEntity>(
           this ParallelQueueProducerOptions<TEntity> options)
        {
            options.SetProducer((sp, name) =>
            {
                return new MemoryQueueProducer<TEntity>();
            });
            return options;
        }
    }
}
