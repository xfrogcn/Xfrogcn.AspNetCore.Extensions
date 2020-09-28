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
            Func<IServiceProvider, string, object, string, Task> proc1 = async (sp, entity, state, name) =>
              {
                  await Task.Delay(100);
              };
            Func<IServiceProvider, string, object, string, Task> testProc1 = async (sp, entity, state, name) =>
            {
                await Task.Delay(100);
            };
            Func<IServiceProvider, int, object, string, Task> proc2 = async (sp, entity, state, name) =>
            {
                await Task.Delay(100);
            };
            IServiceCollection sc = new ServiceCollection()
                .AddLogging();

            sc.AddParallelQueue<string>().ConfigConsumer(proc1);
            sc.AddParallelQueue<string>("test").ConfigConsumer(testProc1);
            sc.AddParallelQueue<string>("test").ConfigConsumer(options =>
            {
                options.ExecutorQueueCapacity = 10;
                options.ExecutorCount = 10;
            });
            sc.AddParallelQueue<string>("test2").ConfigConsumer(10, 10, proc1);
            sc.AddParallelQueue<string>("test2").ConfigConsumer(options =>
            {
                    // 覆盖
                    options.ExecutorCount = 8;
            });
            sc.AddParallelQueue<int>().ConfigConsumer(proc2);

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
            Func<IServiceProvider, string, string, string, Task> proc1 = async (sp, entity, state, name) =>
            {
                await Task.Delay(100);
            };
            Func<IServiceProvider, string, string, string, Task> testProc1 = async (sp, entity, state, name) =>
            {
                await Task.Delay(100);
            };
            Func<IServiceProvider, int, string, string, Task> proc2 = async (sp, entity, state, name) =>
            {
                await Task.Delay(100);
            };
            IServiceCollection sc = new ServiceCollection()
                .AddLogging();

            sc.AddParallelQueue<string,string>().ConfigConsumer(6, 6, proc1);
            sc.AddParallelQueue<string, string>().ConfigConsumer(options =>
                {
                    options.ExecutorCount = 7;
                });
            sc.AddParallelQueue<string,string>("test").ConfigConsumer(testProc1);
            sc.AddParallelQueue<string, string>("test").ConfigConsumer(options =>
            {
                options.ExecutorQueueCapacity = 10;
                options.ExecutorCount = 10;
            });
            sc.AddParallelQueue<string,string>("test2").ConfigConsumer(10, 10, proc1);
            sc.AddParallelQueue<string, string>("test2").ConfigConsumer(options =>
            {
                    // 覆盖
                    options.ExecutorCount = 8;
            });
            sc.AddParallelQueue<int,string>().ConfigConsumer(proc2);

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
            Func<IServiceProvider, string, string, string, Task> proc1 = async (sp, entity, state, name) =>
            {
                await Task.Delay(100);
            };

            IServiceCollection sc = new ServiceCollection()
                .AddLogging();

            sc.AddParallelQueue<string,string>().ConfigConsumer(proc1);


            var sp = sc.BuildServiceProvider();

            IParallelQueueConsumerFactory factory = sp.GetRequiredService<IParallelQueueConsumerFactory>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var abc = factory.CreateConsumer<int, int>(0);
            });

        }
    }
}
