using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 队列生产者
    /// </summary>
    public interface IParallelQueueProducer<TEntity>
    {
        Task<bool> TryAddAsync(TEntity entity);

        Task<bool> TryTakeAsync(TimeSpan timeout, out TEntity entity);
    }
}
