using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ParallelQueueServiceConnectionExtensions
    {

        public static IServiceCollection AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, Func<TEntity, TState, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer(string.Empty, executeDelegate);

        public static IServiceCollection AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, Func<TEntity, object, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer(string.Empty, executeDelegate);

        public static IServiceCollection AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, string name,  Func<TEntity, object, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer<TEntity, object>(name, executeDelegate);

        public static IServiceCollection AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, string name, Func<TEntity, TState, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer(name, 1, 5, executeDelegate);


        public static IServiceCollection AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, int quequCapacity, int executorCount, Func<TEntity, TState, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer(string.Empty, quequCapacity, executorCount, executeDelegate);

        public static IServiceCollection AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, int quequCapacity, int executorCount, Func<TEntity, object, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer(string.Empty, quequCapacity, executorCount, executeDelegate);

        public static IServiceCollection AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, string name, int quequCapacity, int executorCount, Func<TEntity, object, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer<TEntity,object>(name, quequCapacity, executorCount, executeDelegate);

        public static IServiceCollection AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, string name, int quequCapacity, int executorCount, Func<TEntity, TState, string, Task> executeDelegate)
            => serviceDescriptors.AddParallelQueueConsumer<TEntity, TState>(name, options =>
            {
                options.ExecuteDelegate = executeDelegate;
                options.ExecutorCount = executorCount;
                options.ExecutorQueueCapacity = quequCapacity;
            });

 

        public static IServiceCollection AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, Action<ParallelQueueConsumerOptions<TEntity, TState>> configAction)
            => serviceDescriptors.AddParallelQueueConsumer(string.Empty, configAction);

        public static IServiceCollection AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, Action<ParallelQueueConsumerOptions<TEntity, object>> configAction)
            => serviceDescriptors.AddParallelQueueConsumer<TEntity, object>(string.Empty, configAction);

        public static IServiceCollection AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, string name, Action<ParallelQueueConsumerOptions<TEntity, object>> configAction)
            => serviceDescriptors.AddParallelQueueConsumer<TEntity, object>(name, configAction);


        public static IServiceCollection AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, string name, Action<ParallelQueueConsumerOptions<TEntity, TState>> configAction)
        {
            serviceDescriptors.AddOptions();
            serviceDescriptors.TryAddSingleton<IParallelQueueConsumerFactory, DefaultParallelQueueConsumerFactory>();
            serviceDescriptors.TryAddSingleton<IParallelQueueProducerFactory, DefaultParallelQueueProducerFactory>();
            configAction = configAction ?? (_ => { });
            serviceDescriptors.Configure<ParallelQueueConsumerOptions<TEntity, TState>>(name, configAction);
            return serviceDescriptors;
        }

        public static IServiceCollection AddParallelQueueProducer<TEntity>(this IServiceCollection serviceDescriptors, string name, Action<ParallelQueueProducerOptions<TEntity>> configAction)
        {
            serviceDescriptors.AddOptions();
            serviceDescriptors.TryAddSingleton<IParallelQueueProducerFactory, DefaultParallelQueueProducerFactory>();
            serviceDescriptors.Configure<ParallelQueueProducerOptions<TEntity>>(name, configAction);
            return serviceDescriptors;
        }
    }
}
