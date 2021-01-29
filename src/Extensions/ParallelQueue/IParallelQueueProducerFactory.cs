namespace Xfrogcn.AspNetCore.Extensions
{
    public interface IParallelQueueProducerFactory
    {
        /// <summary>
        /// 创建队列生产者
        /// </summary>
        /// <typeparam name="TEntity">队列项类型</typeparam>
        /// <param name="queueName">队列名称</param>
        /// <returns></returns>
        IParallelQueueProducer<TEntity> CreateProducer<TEntity>(string queueName);
    }
}
