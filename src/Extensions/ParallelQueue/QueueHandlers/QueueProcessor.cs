using System;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class QueueProcessor<TEntity, TState>
    {
        readonly QueueHandlerFactory<TEntity, TState> _factory;
        public QueueProcessor(QueueHandlerFactory<TEntity, TState> factory)
        {
            _factory = factory;
        }

        public async Task Process(TEntity msg, TState state, string name, Func<IServiceProvider, Exception, TEntity, TState, string, Task> errorHandler)
        {
            await _factory.Process(msg, state, name, errorHandler);
        }
    }
}
