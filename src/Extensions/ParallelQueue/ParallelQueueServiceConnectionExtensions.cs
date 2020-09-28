using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ParallelQueueServiceConnectionExtensions
    {

        //public static ParallelQueueBuilder<TEntity, TState> AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, Func<TEntity, TState, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer(string.Empty, executeDelegate);

        //public static ParallelQueueBuilder<TEntity, object> AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, Func<TEntity, object, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer(string.Empty, executeDelegate);

        //public static ParallelQueueBuilder<TEntity, object> AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, string name,  Func<TEntity, object, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer<TEntity, object>(name, executeDelegate);

        //public static ParallelQueueBuilder<TEntity, TState> AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, string name, Func<TEntity, TState, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer(name, 1, 5, executeDelegate);


        //public static ParallelQueueBuilder<TEntity, TState> AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, int quequCapacity, int executorCount, Func<TEntity, TState, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer(string.Empty, quequCapacity, executorCount, executeDelegate);

        //public static ParallelQueueBuilder<TEntity, object> AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, int quequCapacity, int executorCount, Func<TEntity, object, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer(string.Empty, quequCapacity, executorCount, executeDelegate);

        //public static ParallelQueueBuilder<TEntity, object> AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, string name, int quequCapacity, int executorCount, Func<TEntity, object, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer<TEntity,object>(name, quequCapacity, executorCount, executeDelegate);

        //public static ParallelQueueBuilder<TEntity, TState> AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, string name, int quequCapacity, int executorCount, Func<TEntity, TState, string, Task> executeDelegate)
        //    => serviceDescriptors.AddParallelQueueConsumer<TEntity, TState>(name, options =>
        //    {
        //        options.ExecuteDelegate = executeDelegate;
        //        options.ExecutorCount = executorCount;
        //        options.ExecutorQueueCapacity = quequCapacity;
        //    });

 

        //public static ParallelQueueBuilder<TEntity, TState> AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, Action<ParallelQueueConsumerOptions<TEntity, TState>> configAction)
        //    => serviceDescriptors.AddParallelQueueConsumer(string.Empty, configAction);

        //public static ParallelQueueBuilder<TEntity, object> AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, Action<ParallelQueueConsumerOptions<TEntity, object>> configAction)
        //    => serviceDescriptors.AddParallelQueueConsumer<TEntity, object>(string.Empty, configAction);

        //public static ParallelQueueBuilder<TEntity, object> AddParallelQueueConsumer<TEntity>(this IServiceCollection serviceDescriptors, string name, Action<ParallelQueueConsumerOptions<TEntity, object>> configAction)
        //    => serviceDescriptors.AddParallelQueueConsumer<TEntity, object>(name, configAction);


        //public static ParallelQueueBuilder<TEntity, TState> AddParallelQueueConsumer<TEntity, TState>(this IServiceCollection serviceDescriptors, string name, Action<ParallelQueueConsumerOptions<TEntity, TState>> configAction)
        //{
        //    return AddParallelQueue<TEntity, TState>(serviceDescriptors, name)
        //        .ConfigConsumer(configAction);
        //}

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
