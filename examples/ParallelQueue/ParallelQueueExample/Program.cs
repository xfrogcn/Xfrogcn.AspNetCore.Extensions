using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParallelQueueExample.Handlers;
using System;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace ParallelQueueExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 以下演示从一个队列接收消息，然后交给5个子队列进行处理，针对每个消息将通过处理管道的两个处理器进行处理

            const string QUEUE_NAME = "TEST_QUEUE";
            IHost host = Host.CreateDefaultBuilder()
                .UseExtensions()
                .ConfigureServices(services =>
                {
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
                        
                })
                .Build();

            await host.StartAsync();

            // 模拟消息生产
            _ = Task.Run(async () =>
              {
                  var factory = host.Services.GetRequiredService<IParallelQueueProducerFactory>();
                  factory.CreateProducer<NotifyMessage>(QUEUE_NAME);
                  while (true)
                  {

                      await Task.Delay(100);
                  }
              });

            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
