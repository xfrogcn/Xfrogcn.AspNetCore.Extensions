using System;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class QueueProcessor<TEntity, TState>
    {
        readonly QueueHandlerFactory<TEntity, TState> _factory;
        public QueueProcessor(QueueHandlerFactory<TEntity, TState> factory)
        {
            _factory = factory;
        }

        public async Task Process(TEntity msg, TState state, string name, Func<Exception, TEntity, TState, string, Task> errorHandler)
        {
            await _factory.Process(msg, state, name, errorHandler);
        }
    }
}
