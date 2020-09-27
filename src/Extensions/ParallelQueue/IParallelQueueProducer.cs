using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    /// <summary>
    /// 队列生产者
    /// </summary>
    public interface IParallelQueueProducer<TEntity>
    {
        Task<bool> TryAddAsync(TEntity entity, CancellationToken token);

        Task<(TEntity, bool)> TryTakeAsync(TimeSpan timeout,  CancellationToken token);

        Task StopAsync(CancellationToken token);
    }
}
