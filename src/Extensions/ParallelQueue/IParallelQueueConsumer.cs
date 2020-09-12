using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    /// <summary>
    /// 并行队列消费者
    /// </summary>
    /// <typeparam name="TEntity">消息队列实体类型</typeparam>
    /// <typeparam name="TState">状态数据类型</typeparam>
    public interface IParallelQueueConsumer<TEntity, TState>
    {
        bool TryAdd(TEntity entity, TimeSpan timeout);

        Task StartAsync();

        Task StopAsync();

        long ExecutedCount { get; }

        double TotalLoad { get; }

        IReadOnlyList<QueueExecutorLoadInfo> GetDetailLoadInfo();
    }
}
