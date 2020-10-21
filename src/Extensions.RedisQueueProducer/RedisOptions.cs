using StackExchange.Redis;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class RedisOptions
    {
        //
        // 摘要:
        //     The configuration used to connect to Redis.
        public string Configuration { get; set; }
        //
        // 摘要:
        //     The configuration used to connect to Redis. This is preferred over Configuration.
        public ConfigurationOptions ConfigurationOptions { get; set; }
        //
        // 摘要:
        //     The Redis instance name.
        public string InstanceName { get; set; }

        /// <summary>
        /// 队列类型，暂不支持Bag
        /// </summary>
        public QueueType QueueType { get; set; }

    }
}
