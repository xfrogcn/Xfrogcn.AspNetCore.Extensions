using System;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    /// <summary>
    /// 并行队列处理器配置
    /// </summary>
    public class ParallelQueueConsumerOptions<TEntity, TState>
    {
        /// <summary>
        /// 执行器数量
        /// </summary>
        public int ExecutorCount { get; set; } = 5;

        /// <summary>
        /// 执行器队列容量
        /// </summary>
        public int ExecutorQueueCapacity { get; set; } = 1;

        /// <summary>
        /// 执行器队列类型<see cref="QueueType"/>
        /// </summary>
        public QueueType ExecutorQueueType { get; set; } = QueueType.FIFO;

        /// <summary>
        /// 默认等待队列超时时间
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 处理委托
        /// </summary>
        public Func<TEntity, TState, string, Task> ExecuteDelegate { get; set; }

        /// <summary>
        /// 统计周期
        /// 最小10秒
        /// </summary>
        public TimeSpan StatisticalPeriod { get; set; } = TimeSpan.FromMinutes(1);
    }

    public enum QueueType
    {
        /// <summary>
        /// 先进先出
        /// </summary>
        FIFO,
        /// <summary>
        /// 后进先出
        /// </summary>
        LIFO,
        /// <summary>
        /// 无序
        /// </summary>
        Bag
    }
}
