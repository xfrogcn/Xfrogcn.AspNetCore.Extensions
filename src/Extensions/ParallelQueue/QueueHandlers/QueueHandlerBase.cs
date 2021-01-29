using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class QueueHandlerBase<TEntity, TState> : IQueueHandler<TEntity, TState>
    {
        public virtual int Order => 100;


        public virtual Task Process(QueueHandlerContext<TEntity, TState> context)
        {
            return Task.CompletedTask;
        }
    }

    public class QueueHandlerBase<TEntity> : QueueHandlerBase<TEntity, object>
    {
    }
}
