using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;
using Xunit;

namespace Extensions.Tests.ParallelQueue
{
    [Trait("", "ParallelQueue")]
    public class DefaultParallelQueueConsumerTests
    {
        [Fact(DisplayName = "默认处理器")]
        public async Task Test1()
        {
            Func<string, string, string, Task> proc = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };

            DefaultParallelQueueConsumer<string, string> consumer
                = new DefaultParallelQueueConsumer<string, string>(
                    new ParallelQueueConsumerOptions<string, string>()
                    {
                        ExecutorCount = 5,
                        ExecutorQueueCapacity = 1,
                        ExecuteDelegate = proc
                    },
                    "test",
                    "state",
                    new Microsoft.Extensions.Logging.LoggerFactory()
                    );

            await consumer.StartAsync();

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();

            _ = Task.Run(() =>
              {
                  for (int i = 0; i < 10; i++)
                  {
                      consumer.TryAdd(i.ToString(), TimeSpan.FromSeconds(10));
                  }

                  cts.Cancel();
              });

            cts.Token.WaitHandle.WaitOne();
            await consumer.StopAsync();


            var loadInfo = consumer.GetDetailLoadInfo().ToList();
            Assert.Equal(10, consumer.ExecutedCount);
            Assert.True(loadInfo.Count(x => x.Counter > 0) > 1);
        }


        [Fact(DisplayName = "排队")]
        public async Task Test2()
        {
            Func<string, string, string, Task> proc = async (entity, state, name) =>
            {
                await Task.Delay(1000);
            };

            DefaultParallelQueueConsumer<string, string> consumer
                = new DefaultParallelQueueConsumer<string, string>(
                    new ParallelQueueConsumerOptions<string, string>()
                    {
                        ExecutorCount = 1,
                        ExecutorQueueCapacity = 1,
                        ExecuteDelegate = proc
                    },
                    "test",
                    "state",
                    new Microsoft.Extensions.Logging.LoggerFactory()
                    );

            await consumer.StartAsync();

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            List<long> list = new List<long>();
            _ = Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    // 1 正常 2 正常 3、4、5等待1秒
                    consumer.TryAdd(i.ToString(), TimeSpan.FromSeconds(10));
                    sw.Stop();
                    list.Add(sw.ElapsedMilliseconds);
                }
                cts.Cancel();
            });

            cts.Token.WaitHandle.WaitOne();
            await consumer.StopAsync();


            var loadInfo = consumer.GetDetailLoadInfo().ToList();
            Assert.Equal(5, consumer.ExecutedCount);
            Assert.True(list.Sum() >= 3000);
        }

        [Fact(DisplayName = "排队-队列容量不为1")]
        public async Task Test3()
        {
            Func<string, string, string, Task> proc = async (entity, state, name) =>
            {
                await Task.Delay(1000);
            };

            DefaultParallelQueueConsumer<string, string> consumer
                = new DefaultParallelQueueConsumer<string, string>(
                    new ParallelQueueConsumerOptions<string, string>()
                    {
                        ExecutorCount = 1,
                        ExecutorQueueCapacity = 2,
                        ExecuteDelegate = proc
                    },
                    "test",
                    "state",
                    new Microsoft.Extensions.Logging.LoggerFactory()
                    );

            await consumer.StartAsync();

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            List<long> list = new List<long>();
            _ = Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    // 1 正常 2 正常 3 正常、4、5等待1秒
                    consumer.TryAdd(i.ToString(), TimeSpan.FromSeconds(10));
                    sw.Stop();
                    list.Add(sw.ElapsedMilliseconds);
                }
                cts.Cancel();
            });

            cts.Token.WaitHandle.WaitOne();
            await consumer.StopAsync();


            var loadInfo = consumer.GetDetailLoadInfo().ToList();
            Assert.Equal(5, consumer.ExecutedCount);
            Assert.True(list.Sum() >= 2000);
        }

        [Fact(DisplayName = "排队-多执行器，多容量")]
        public async Task Test4()
        {
            Func<string, string, string, Task> proc = async (entity, state, name) =>
            {
                await Task.Delay(1000);
            };

            DefaultParallelQueueConsumer<string, string> consumer
                = new DefaultParallelQueueConsumer<string, string>(
                    new ParallelQueueConsumerOptions<string, string>()
                    {
                        ExecutorCount = 2,
                        ExecutorQueueCapacity = 2,
                        ExecuteDelegate = proc
                    },
                    "test",
                    "state",
                    new Microsoft.Extensions.Logging.LoggerFactory()
                    );

            await consumer.StartAsync();

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            List<long> list = new List<long>();
            _ = Task.Run(() =>
            {
                for (int i = 0; i < 8; i++)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    // 0、1、2、3、4、5正常，6，7等待1秒(并行）
                    consumer.TryAdd(i.ToString(), TimeSpan.FromSeconds(10));
                    sw.Stop();
                    list.Add(sw.ElapsedMilliseconds);
                }
                cts.Cancel();
            });

            cts.Token.WaitHandle.WaitOne();
            await consumer.StopAsync();


            var loadInfo = consumer.GetDetailLoadInfo().ToList();
            Assert.Equal(8, consumer.ExecutedCount);
            Assert.Equal(4, loadInfo[1].Counter);
            // 
            Assert.True(list.Sum() >= 1000 && list.Sum()<1500);
        }
    }
}
