using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 并行队列消费者工厂
    /// </summary>
    public interface IParallelQueueConsumerFactory
    {
        /// <summary>
        /// 创建队列消费者
        /// </summary>
        /// <typeparam name="TEntity">队列实体类型</typeparam>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="name">名称</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        IParallelQueueConsumer<TEntity, TState> CreateConsumer<TEntity, TState>(string name, TState state);
    }
}
