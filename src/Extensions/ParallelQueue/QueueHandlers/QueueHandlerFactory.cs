﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class QueueHandlerFactory<TEntity, TState>
    {
        readonly List<IQueueHandler<TEntity, TState>> _handlers = new List<IQueueHandler<TEntity, TState>>();
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<QueueHandlerFactory<TEntity, TState>> _logger;
        readonly IAutoRetry _retry;

        public QueueHandlerFactory(
            IServiceProvider serviceProvider,
            IAutoRetry retry,
            IEnumerable<IQueueHandler<TEntity, TState>> handlers,
            ILogger<QueueHandlerFactory<TEntity, TState>> logger)
        {
            if (handlers != null)
            {
                var list = handlers.ToList().OrderBy(h => h.Order);
                _handlers.AddRange(list);
            }
            _serviceProvider = serviceProvider;
            _logger = logger;
            _retry = retry;
        }

        public virtual async Task Process(TEntity msg, TState state, string name, Func<IServiceProvider, Exception,TEntity, TState, string, Task> errorHandler )
        {
            var list = _handlers.ToList();
            QueueHandlerContext<TEntity, TState> context = new QueueHandlerContext<TEntity, TState>()
            {
                QueueName = name,
                Message = msg,
                State = state
            };
            foreach(var h in list)
            {
                try
                {
                    await _retry.Retry(async () =>
                    {
                        using(var scope = _serviceProvider.CreateScope())
                        {
                            context.ServiceProvider = scope.ServiceProvider;
                            var handler = scope.ServiceProvider.GetRequiredService(h.GetType()) as IQueueHandler<TEntity, TState>;
                            await handler.Process(context);
                        }

                    }, 3, 100, true);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "处理队列消息失败");
                    if (errorHandler != null)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            await errorHandler(_serviceProvider, e, msg, state, name);
                        }
                    }
                }

                if (context.Stoped)
                {
                    break;
                }
                
            }
        }
    }
}
