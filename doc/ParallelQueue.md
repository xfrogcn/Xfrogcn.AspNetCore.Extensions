# 并行队列

并行队列处理可以将一个大的队列，拆分到多个子队列进行并行处理，以提高处理效率。同时，在每个子队列处理中实现了处理管道，可灵活扩展。
![Img](../并行队列.png)

- 生产者队列：代表原始队列，如Redis、MQ等
- 消费者队列：从生产者队列中获取消息，作为待处理队列
- 队列处理器：处理单个消息的处理管道

## 生产者队列

生产者队列代码原始队列，扩展库提供本地内存队列（用于测试）以及Redis队列（通过Xfrogcn.AspNetCore.Extensions.RedisQueueProducer库提供），当然，你也可是实现自己的生产者队列。

你可以通过ParallelQueueBuilder的ConfigProducer方法来配置生产者队列：

```c#
    // 添加一个名称为TEST_QUEUE的并行处理队列
    services.AddParallelQueue<NotifyMessage>(QUEUE_NAME)
        // 配置生产者队列
        .ConfigProducer(options =>
        {
            // 使用Redis队列
            options.UseRedis(redisOptions=>{
                // redis配置
            });
            // 或者使用内存队列
            // options.UseMemory();
        })
```

## 队列处理器

队列消息的处理采用管道模式，你可以在管道中添加自己的消息处理器，消息处理器从QueueHandlerBase&lt;TMessage&gt;继承，其中Order属性用于指定执行顺序，处理器按照Order升序方式排列执行。

```c#
    /// <summary>
    /// 消息处理器，从QueueHandlerBase继承，实现对单个消息的处理
    /// </summary>
    class MessageHandlerA : QueueHandlerBase<NotifyMessage>
    { 
        /// <summary>
        /// 顺序，越小的排在前面
        /// </summary>
        public override int Order => base.Order;

        public override Task Process(QueueHandlerContext<NotifyMessage, object> context)
        {
            // 示例： 演示消息处理管道
            context.Message.Output = context.Message.Input + 1;
            return base.Process(context);
        }
    }
```

## 配置

要使用并行队列，可通过IServiceCollection上的AddParallelQueue扩展方法进行配置：

```c#
    const string QUEUE_NAME = "TEST_QUEUE";

    // 添加一个名称为TEST_QUEUE的并行处理队列
    services.AddParallelQueue<NotifyMessage>(QUEUE_NAME)
        // 添加对应的默认HostedService，通过该托管服务将关联ParallelQueueProducer和ParallelQueueConsumer
        .AddHostedService()
        // 配置生产者队列
        .ConfigProducer(options =>
        {
            // 使用内存队列
            options.UseMemory();
        })
        // 配置使用管道模式的Consumer
        .ConfigConsumerHandler(1, 5)
        // 加入消息处理器A
        .AddConsumerHandler<MessageHandlerA>()
        // 加入消息处理器B
        .AddConsumerHandler<MessageHandlerB>();
```

有关并行队列的演示示例，请参考`examples/ParallelQueue/ParallelQueueExample`项目
