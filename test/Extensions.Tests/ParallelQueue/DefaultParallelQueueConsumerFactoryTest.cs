using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xfrogcn.AspNetCore.Extensions.ParallelQueue;
using Xunit;

namespace Extensions.Tests.ParallelQueue
{
    [Trait("", "ParallelQueue")]
    public class DefaultParallelQueueConsumerFactoryTest
    {
        [Fact(DisplayName = "默认TState")]
        public void DefaultState()
        {
            Func<string, object, string, Task> proc1 = async (entity, state, name) =>
              {
                  await Task.Delay(100);
              };
            Func<string, object, string, Task> testProc1 = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };
            Func<int, object, string, Task> proc2 = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };
            IServiceCollection sc = new ServiceCollection()
                .AddLogging();

            sc.AddParallelQueueConsumer<string>(proc1);
            sc.AddParallelQueueConsumer<string>("test", testProc1);
            sc.AddParallelQueueConsumer<string>("test", options =>
            {
                options.ExecutorQueueCapacity = 10;
                options.ExecutorCount = 10;
            });
            sc.AddParallelQueueConsumer<string>("test2", 10, 10, proc1);
            sc.AddParallelQueueConsumer<string>("test2", options =>
            {
                    // 覆盖
                    options.ExecutorCount = 8;
            });
            sc.AddParallelQueueConsumer<int>(proc2);

            var sp = sc.BuildServiceProvider();

            IParallelQueueConsumerFactory factory = sp.GetRequiredService<IParallelQueueConsumerFactory>();
            var c1 = factory.CreateConsumer<string>() as DefaultParallelQueueConsumer<string, object>;
            var c2 = factory.CreateConsumer<int>() as DefaultParallelQueueConsumer<int, object>;
            Assert.Equal(proc1, c1.Options.ExecuteDelegate);
            Assert.Equal(proc2, c2.Options.ExecuteDelegate);

            Assert.Equal(5, c1.Options.ExecutorCount);
            Assert.Equal(1, c1.Options.ExecutorQueueCapacity);

            var c11 = factory.CreateConsumer<string>();

            Assert.Equal(c1, c11);

            var c1_name = factory.CreateConsumer<string>("test") as DefaultParallelQueueConsumer<string, object>;
            Assert.Equal(testProc1, c1_name.Options.ExecuteDelegate);
            Assert.Equal(10, c1_name.Options.ExecutorCount);
            Assert.Equal(10, c1_name.Options.ExecutorQueueCapacity);

            var test2 = factory.CreateConsumer<string>("test2") as DefaultParallelQueueConsumer<string, object>;
            Assert.Equal(proc1, test2.Options.ExecuteDelegate);
            Assert.Equal(8, test2.Options.ExecutorCount);
            Assert.Equal(10, test2.Options.ExecutorQueueCapacity);

        }


        [Fact(DisplayName = "正常")]
        public void Normal()
        {
            Func<string, string, string, Task> proc1 = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };
            Func<string, string, string, Task> testProc1 = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };
            Func<int, string, string, Task> proc2 = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };
            IServiceCollection sc = new ServiceCollection()
                .AddLogging();

            sc.AddParallelQueueConsumer(6, 6, proc1);
            sc.AddParallelQueueConsumer<string, string>(options =>
                {
                    options.ExecutorCount = 7;
                });
            sc.AddParallelQueueConsumer("test", testProc1);
            sc.AddParallelQueueConsumer<string, string>("test", options =>
            {
                options.ExecutorQueueCapacity = 10;
                options.ExecutorCount = 10;
            });
            sc.AddParallelQueueConsumer("test2", 10, 10, proc1);
            sc.AddParallelQueueConsumer<string, string>("test2", options =>
            {
                    // 覆盖
                    options.ExecutorCount = 8;
            });
            sc.AddParallelQueueConsumer(proc2);

            var sp = sc.BuildServiceProvider();

            IParallelQueueConsumerFactory factory = sp.GetRequiredService<IParallelQueueConsumerFactory>();
            var c1 = factory.CreateConsumer<string, string>("") as DefaultParallelQueueConsumer<string, string>;
            var c2 = factory.CreateConsumer<int, string>("") as DefaultParallelQueueConsumer<int, string>;
            Assert.Equal(proc1, c1.Options.ExecuteDelegate);
            Assert.Equal(proc2, c2.Options.ExecuteDelegate);

            Assert.Equal(7, c1.Options.ExecutorCount);
            Assert.Equal(6, c1.Options.ExecutorQueueCapacity);

            var c11 = factory.CreateConsumer<string, string>("");

            Assert.Equal(c1, c11);

            var c1_name = factory.CreateConsumer<string, string>("test", "") as DefaultParallelQueueConsumer<string, string>;
            Assert.Equal(testProc1, c1_name.Options.ExecuteDelegate);
            Assert.Equal(10, c1_name.Options.ExecutorCount);
            Assert.Equal(10, c1_name.Options.ExecutorQueueCapacity);

            var test2 = factory.CreateConsumer<string, string>("test2", "") as DefaultParallelQueueConsumer<string, string>;
            Assert.Equal(proc1, test2.Options.ExecuteDelegate);
            Assert.Equal(8, test2.Options.ExecutorCount);
            Assert.Equal(10, test2.Options.ExecutorQueueCapacity);

        }

        [Fact(DisplayName = "未配置")]
        public void Invalid()
        {
            Func<string, string, string, Task> proc1 = async (entity, state, name) =>
            {
                await Task.Delay(100);
            };

            IServiceCollection sc = new ServiceCollection()
                .AddLogging();

            sc.AddParallelQueueConsumer(proc1);


            var sp = sc.BuildServiceProvider();

            IParallelQueueConsumerFactory factory = sp.GetRequiredService<IParallelQueueConsumerFactory>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var abc = factory.CreateConsumer<int, int>(0);
            });

        }
    }
}
