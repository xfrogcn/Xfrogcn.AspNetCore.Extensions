using Microsoft.Extensions.DependencyInjection.Extensions;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.RedisQueueProducer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class QueueProducerServiceCollectionExtensions
    {
        public static ParallelQueueBuilder<TEntity, TState> AddRedisQueueProducer<TEntity, TState>(this ParallelQueueBuilder<TEntity, TState> builder)
        {
            builder.Services.TryAddSingleton<RedisConnectionManager>();
            return builder;
        }
    }
}
