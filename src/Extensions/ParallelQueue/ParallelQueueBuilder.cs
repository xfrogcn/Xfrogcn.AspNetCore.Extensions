using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class ParallelQueueBuilder<TEntity, TState>
    {
        public IServiceCollection Services { get; }

        public string Name { get; }

        public ParallelQueueBuilder(string name, IServiceCollection serviceDescriptors)
        {
            Name = name;
            Services = serviceDescriptors;
        }

        public ParallelQueueBuilder<TEntity,TState> ConfigConsumer(Action<ParallelQueueConsumerOptions<TEntity, TState>> configAction)
        {
            if (configAction != null)
            {
                Services.Configure<ParallelQueueConsumerOptions<TEntity, TState>>(Name, configAction);
            }
            return this;
        }

        public ParallelQueueBuilder<TEntity, TState> ConfigConsumer(Func<TEntity, TState, string, Task> executeDelegate)
        {
            if (executeDelegate != null)
            {
                Services.Configure<ParallelQueueConsumerOptions<TEntity, TState>>(Name, options=>
                {
                    options.ExecuteDelegate = executeDelegate;
                });
            }
            return this;
        }

        public ParallelQueueBuilder<TEntity, TState> ConfigConsumer(int quequCapacity, int executorCount, Func<TEntity, TState, string, Task> executeDelegate)
        {
            if (executeDelegate != null)
            {
                Services.Configure<ParallelQueueConsumerOptions<TEntity, TState>>(Name, options =>
                {
                    options.ExecuteDelegate = executeDelegate;
                    options.ExecutorCount = executorCount;
                    options.ExecutorQueueCapacity = quequCapacity;
                });
            }
            return this;
        }

        public ParallelQueueBuilder<TEntity,TState> ConfigProducer(Action<ParallelQueueProducerOptions<TEntity>> configAction)
        {
            if (configAction != null)
            {
                Services.Configure<ParallelQueueProducerOptions<TEntity>>(Name, configAction);
            }
            return this;
        }
    }
}
