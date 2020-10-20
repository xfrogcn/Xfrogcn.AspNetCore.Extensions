using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class QueueHandlerBase<TEntity, TState> : IQueueHandler<TEntity, TState>
    {
        public virtual int Order => 100;


        public virtual Task Process(TEntity msg, TState state, string name)
        {
            return Task.CompletedTask;
        }
    }
}
