using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public interface IQueueHandler<TEentity, TState>
    {
        int Order { get; }

        Task Process(TEentity msg, TState state, string name);
    }
}
