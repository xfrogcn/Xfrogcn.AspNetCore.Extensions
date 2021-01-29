using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    public static class FactoryExtensions
    {
        public static IParallelQueueConsumer<TEntity, object> CreateConsumer<TEntity>(this IParallelQueueConsumerFactory factory, string name)
        {
            return factory.CreateConsumer<TEntity, object>(name, null);
        }

        public static IParallelQueueConsumer<TEntity, object> CreateConsumer<TEntity>(this IParallelQueueConsumerFactory factory)
        {
            return factory.CreateConsumer<TEntity, object>(string.Empty, null);
        }

        public static IParallelQueueConsumer<TEntity, TState> CreateConsumer<TEntity, TState>(this IParallelQueueConsumerFactory factory, TState state)
        {
            return factory.CreateConsumer<TEntity, TState>(string.Empty, state);
        }
    }
}
