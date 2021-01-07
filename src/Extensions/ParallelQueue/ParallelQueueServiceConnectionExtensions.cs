using Microsoft.Extensions.DependencyInjection.Extensions;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ParallelQueueServiceCollectionionExtensions
    {
        public static ParallelQueueBuilder<TEntity, TState> AddParallelQueue<TEntity, TState>(this IServiceCollection serviceDescriptors, string name)
        {
            serviceDescriptors.AddOptions();
            serviceDescriptors.TryAddSingleton<IParallelQueueConsumerFactory, DefaultParallelQueueConsumerFactory>();
            serviceDescriptors.TryAddSingleton<IParallelQueueProducerFactory, DefaultParallelQueueProducerFactory>();
            ParallelQueueBuilder<TEntity, TState> builder = new ParallelQueueBuilder<TEntity, TState>(name, serviceDescriptors);
            return builder;
        }

        public static ParallelQueueBuilder<TEntity,object> AddParallelQueue<TEntity>(this IServiceCollection serviceDescriptors, string name)
        {
            return AddParallelQueue<TEntity, object>(serviceDescriptors, name);
        }

        public static ParallelQueueBuilder<TEntity, object> AddParallelQueue<TEntity>(this IServiceCollection serviceDescriptors)
        {
            return AddParallelQueue<TEntity, object>(serviceDescriptors, string.Empty);
        }

        public static ParallelQueueBuilder<TEntity, TState> AddParallelQueue<TEntity, TState>(this IServiceCollection serviceDescriptors)
        {
            return AddParallelQueue<TEntity, TState>(serviceDescriptors, string.Empty);
        }

       
    }
}
