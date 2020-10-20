﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
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

        public ParallelQueueBuilder<TEntity, TState> ConfigConsumer(Func<IServiceProvider, TEntity, TState, string, Task> executeDelegate)
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

        public ParallelQueueBuilder<TEntity, TState> ConfigConsumer(int quequCapacity, int executorCount, Func<IServiceProvider, TEntity, TState, string, Task> executeDelegate)
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

        /// <summary>
        /// 采用Handler模式处理消息队列
        /// </summary>
        /// <param name="quequCapacity"></param>
        /// <param name="executorCount"></param>
        /// <returns></returns>
        public ParallelQueueBuilder<TEntity, TState> ConfigConsumerHandler(int quequCapacity, int executorCount, Func<Exception, TEntity, TState, string, Task> errorHandler = null)
        {
            Services.AddScoped<QueueProcessor<TEntity, TState>>();
            return ConfigConsumer(quequCapacity, executorCount, (sp, msg, state, name) =>
            {
                QueueProcessor<TEntity, TState> processor = sp.GetRequiredService<QueueProcessor<TEntity, TState>>();
                return processor.Process(msg, state, name, errorHandler);
            });
        }

        public ParallelQueueBuilder<TEntity, TState> AddConsumerHandler<THandler>()
            where THandler : class, IQueueHandler<TEntity,TState>
        {
            Services.TryAddEnumerable(ServiceDescriptor.Scoped<IQueueHandler<TEntity, TState>, THandler>());
            Services.TryAddScoped<THandler>();
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

        public ParallelQueueBuilder<TEntity, TState> AddHostedService(TState state = default)
        {
            Services.TryAddSingleton<ParallelQueueHostedService<TEntity, TState>>(sp =>
            {
                IParallelQueueConsumerFactory consumerFactory = sp.GetRequiredService<IParallelQueueConsumerFactory>();
                IParallelQueueProducerFactory producerFactory = sp.GetRequiredService<IParallelQueueProducerFactory>();
                ILogger<ParallelQueueHostedService<TEntity, TState>> logger = sp.GetRequiredService<ILogger<ParallelQueueHostedService<TEntity, TState>>>();
                return new ParallelQueueHostedService<TEntity, TState>(consumerFactory, producerFactory, Name, state, logger);
            });
            return this;
        }
    }
}
