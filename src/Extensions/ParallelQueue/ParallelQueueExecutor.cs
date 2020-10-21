using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    /// <summary>
    /// 并行队列执行器
    /// </summary>
    /// <typeparam name="TEntity">队列项类型</typeparam>
    /// <typeparam name="TState">状态对象</typeparam>
    public class ParallelQueueExecutor<TEntity,TState>
    {
        public BlockingCollection<TEntity> Queue { get; protected set; }

        public TState State { get; protected set; }

        public ParallelQueueConsumerOptions<TEntity,TState> Options { get; protected set; }

        public string Name { get; set; }

        public long Counter { get; set; }

        public TimeSpan StatisticalPeriod { get; protected set; }

        private List<CostItem> costList = new List<CostItem>();

        private long _periodCounter = 0;

        private DateTime _lastStatisticalTime = DateTime.UtcNow;

        static Action<ILogger, string, double,Exception> _loadLogger =
            LoggerMessage.Define<string, double>(LogLevel.Information, new EventId(200, "ParallelQueueExecutorLoadInfo"), "执行器执行负载, 名称：{name} 当前负载：{load}");

        public class CostItem
        {
            public long Cost { get; set; }

            public DateTime BeginTime { get; set; }

            public DateTime EndTime { get; set; }
        }

        ILogger<ParallelQueueExecutor<TEntity,TState>> _logger = null;
        IServiceProvider _serviceProvider;

        public ParallelQueueExecutor(
            IServiceProvider serviceProvider,
            ParallelQueueConsumerOptions<TEntity, TState> options,
            ILogger<ParallelQueueExecutor<TEntity, TState>> logger,
            string name,
            TState state)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if(logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (options.ExecuteDelegate == null)
            {
                throw new ArgumentNullException(nameof(options.ExecuteDelegate));
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _serviceProvider = serviceProvider;

            int capacity = options.ExecutorQueueCapacity;
            if (capacity <= 0)
            {
                capacity = 1;
            }
          
            IProducerConsumerCollection<TEntity> innerQueue = null;
            switch (options.ExecutorQueueType)
            {
                case QueueType.FIFO:
                    innerQueue = new ConcurrentQueue<TEntity>();
                    break;
                case QueueType.LIFO:
                    innerQueue = new ConcurrentStack<TEntity>();
                    break;
                case QueueType.Bag:
                    innerQueue = new ConcurrentBag<TEntity>();
                    break;
                default:
                    innerQueue = new ConcurrentQueue<TEntity>();
                    break;
            }

            Queue = new BlockingCollection<TEntity>(innerQueue, capacity);
            Options = options;
            Name = name;
            Counter = 0;
            State = state;
            _logger = logger;

            var period = options.StatisticalPeriod;
            if (period.TotalSeconds < 10)
            {
                period = TimeSpan.FromSeconds(10);
            }
            // 最长时间1小时
            if (period.TotalHours > 1)
            {
                period = TimeSpan.FromHours(1);
            }
            StatisticalPeriod = period;

        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        public void ResetCounter()
        {
            Counter = 0;
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            using(var scope = _logger.BeginScope("ExecutorName: {name}", Name ?? ""))
            {
                await InnerRun();
            }
        }

        /// <summary>
        /// 内部执行循环
        /// </summary>
        /// <returns></returns>
        protected virtual async Task InnerRun()
        {
            TimeSpan timeOut = Options.DefaultTimeout;
            if (timeOut.TotalSeconds < 1)
            {
                timeOut = TimeSpan.FromSeconds(1);
            }

            _logger.LogInformation($"队列消费者已启动：{Name}");

            while (!Queue.IsCompleted)
            {
                // 从队列中获取
                TEntity item;
                if (!Queue.TryTake(out item, timeOut))
                {
                    continue;
                }

                // 开始执行
                Stopwatch sw = new Stopwatch();
                sw.Start();
                DateTime begin = DateTime.UtcNow;
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        await Options.ExecuteDelegate(scope.ServiceProvider, item, State, Name);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "队列执行失败");
                }
                finally
                {
                    DateTime end = DateTime.UtcNow;
                    sw.Stop();
                    AddStatisticItem(begin, end, sw.ElapsedMilliseconds);
                }

                if( (DateTime.UtcNow - _lastStatisticalTime)>= StatisticalPeriod)
                {
                    var loadInfo = GetCurrentLoad();
                    _loadLogger(_logger, Name, loadInfo.LoadRatio, null);
                    _lastStatisticalTime = DateTime.UtcNow;
                    _periodCounter = 0;
                }

                Counter++;
                _periodCounter++;
            }

            _logger.LogInformation($"队列消费者已停止：{Name}");
        }


        protected void AddStatisticItem(DateTime begin, DateTime end, long cost)
        {
            costList.Add(new CostItem()
            {
                BeginTime = begin,
                EndTime = end,
                Cost = cost
            });

            DateTime removeStartTime = DateTime.UtcNow.Add(-StatisticalPeriod);

            costList.Where(c => c.EndTime < removeStartTime).ToList().ForEach(c =>
            {
                costList.Remove(c);
            });
        }


        public QueueExecutorLoadInfo GetCurrentLoad()
        {
            if (costList.Count == 0)
            {
                return QueueExecutorLoadInfo.CreateEmptyLoad(StatisticalPeriod);
            }
            DateTime dt = DateTime.UtcNow;
            CostItem first = costList.FirstOrDefault();
            if (first == null)
            {
                return QueueExecutorLoadInfo.CreateEmptyLoad(StatisticalPeriod);
            }

            long totalBusy = costList.Sum(c => c.Cost);

            DateTime beginTime = dt.Add(-StatisticalPeriod);
            if (beginTime > first.BeginTime)
            {
                beginTime = first.BeginTime;
            }
            TimeSpan realPeriod = dt - beginTime;

            double length = realPeriod.TotalMilliseconds;

            return new QueueExecutorLoadInfo()
            {
                Busy = totalBusy,
                Idle = length - totalBusy,
                LoadRatio = totalBusy / length,
                Counter = _periodCounter
            };

        }
    }
}
