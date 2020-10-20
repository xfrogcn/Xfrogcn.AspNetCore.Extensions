using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public interface IQueueHandler<TEntity, TState>
    {
        int Order { get; }

        Task Process(QueueHandlerContext<TEntity, TState> context);
    }
}
