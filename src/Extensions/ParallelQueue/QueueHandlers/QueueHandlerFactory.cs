using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class QueueHandlerFactory<TEntity, TState>
    {
        readonly SortedList<int, IQueueHandler<TEntity, TState>> _handlers = new SortedList<int, IQueueHandler<TEntity, TState>>();
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
                foreach(var h in handlers)
                {
                    _handlers.Add(h.Order, h);
                }
            }
            _serviceProvider = serviceProvider;
            _logger = logger;
            _retry = retry;
        }

        public virtual async Task Process(TEntity msg, TState state, string name, Func<IServiceProvider, Exception,TEntity, TState, string, Task> errorHandler )
        {
            var list = _handlers.Values.ToList();
            foreach(var h in list)
            {
                try
                {
                    await _retry.Retry(async () =>
                    {
                        using(var scope = _serviceProvider.CreateScope())
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(h.GetType()) as IQueueHandler<TEntity, TState>;
                            await handler.Process(msg, state, name);
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
                
            }
        }
    }
}
