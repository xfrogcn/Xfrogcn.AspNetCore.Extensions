using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;
using Xunit;

namespace Extensions.Tests.ParallelQueue
{
    [Trait("", "ParallelQueue")]
    public class RedisQueueTest
    {
        [Fact(DisplayName = "Redis: 超时")]
        public async Task Test1()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddExtensions();
            string queueName = $"test_{StringExtensions.RandomString(4)}";
            sc.AddParallelQueue<string>(queueName)
                .ConfigProducer(options =>
                {
                    options.UseRedis(redis =>
                    {
                        redis.Configuration = "localhost:6379";
                    });
                });
           

            var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IParallelQueueProducerFactory>();
            var producer = factory.CreateProducer<string>(queueName);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var (s,b) = await  producer.TryTakeAsync(TimeSpan.FromSeconds(1), default);
            sw.Stop();
            Assert.False(b);
            Assert.True(sw.Elapsed.TotalSeconds >= 1);

            //立即
            sw.Reset();
            sw.Start();
            await producer.TryAddAsync("A", default);
            (s, b) = await producer.TryTakeAsync(TimeSpan.FromSeconds(1), default);
            sw.Stop();
            Assert.True(b);
            Assert.True(sw.Elapsed.TotalSeconds <1);
        }

        [Fact(DisplayName = "Redis:Take排队-不应该这样使用")]
        public async Task Task2()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddExtensions();

            string queueName = $"test_{StringExtensions.RandomString(4)}";
            sc.AddParallelQueue<string>(queueName)
                .ConfigProducer(options =>
            {
                options.UseRedis(redis =>
                {
                    redis.Configuration = "localhost:6379";
                });
            });
            var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IParallelQueueProducerFactory>();
            var producer = factory.CreateProducer<string>(queueName);

            List<Task> task = new List<Task>();
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            for(int i = 0; i < 10; i++)
            {
                task.Add(Task.Run(async () =>
               {
                   try
                   {
                       var (entity, b) = await producer.TryTakeAsync(TimeSpan.FromSeconds(30), default);
                       if (b)
                       {
                           result.Add(entity);
                       }
                   }catch(Exception e)
                   {

                   }
               }));
            }

            await Task.Delay(500);

            for (int i = 0; i < 10; i++)
            {
                await producer.TryAddAsync($"A{i}", default);
            }

            Task.WaitAll(task.ToArray());

            Assert.Equal(10, result.Count);
            
        }

        [Fact(DisplayName = "Redis:队列处理")]
        public async Task Task3()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddExtensions();

            string queueName = $"test_{StringExtensions.RandomString(4)}";
            sc.AddParallelQueue<string>(queueName)
                .ConfigProducer(options =>
            {
                options.UseRedis(redis =>
                {
                    redis.Configuration = "localhost:6379";
                });
            });
            var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IParallelQueueProducerFactory>();
            var producer = factory.CreateProducer<string>(queueName);

            List<Task> taskList = new List<Task>();
            List<string> result = new List<string>();
            taskList.Add(Task.Run(async () =>
            {
                bool isok = true;
                string entity = "";
                int i = 0;
                while (i < 100)
                {
                    (entity, isok) = await producer.TryTakeAsync(TimeSpan.FromSeconds(5), default);
                    i++;
                    if (isok)
                    {
                        result.Add(entity);
                    }
                }
            }));



            for (int i = 0; i < 10; i++)
            {
                int local = i;
                taskList.Add(Task.Run(async () =>
                {
                    
                    for (int j = 0; j < 10; j++)
                    {
                        await Task.Delay(50);
                        await producer.TryAddAsync($"A{local}_{j}", default);
                    }
                }));
            }

            Task.WaitAll(taskList.ToArray());

            Assert.Equal(100, result.Count);
            Assert.Empty(result.Where(x => string.IsNullOrEmpty(x)));
        }

        [Fact(DisplayName = "Redis:停止")]
        public async Task Task4()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddExtensions();

            string queueName = $"test_{StringExtensions.RandomString(4)}";
            sc.AddParallelQueue<string>(queueName)
                .ConfigProducer(options =>
            {
                options.UseRedis(redis =>
                {
                    redis.Configuration = "localhost:6379";
                });
            });
            var sp = sc.BuildServiceProvider();
            var factory = sp.GetRequiredService<IParallelQueueProducerFactory>();
            var producer = factory.CreateProducer<string>(queueName);

            _ = Task.Run(async () =>
              {
                  while (true)
                  {
                      await producer.TryTakeAsync(TimeSpan.FromSeconds(10), default);
                  }
              });
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await producer.StopAsync(default);
            sw.Stop();
            Assert.True(sw.Elapsed.TotalSeconds < 5);

        }
    }
}
