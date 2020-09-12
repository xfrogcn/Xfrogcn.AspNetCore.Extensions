using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("Extensions.Tests")]

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class DefaultParallelQueueConsumer<TEntity,TState> : IParallelQueueConsumer<TEntity, TState>
    {
        private List<ParallelQueueExecutor<TEntity, TState>> _executorList = new List<ParallelQueueExecutor<TEntity, TState>>();

        private readonly ParallelQueueConsumerOptions<TEntity, TState> _options;

        private readonly ILoggerFactory _loggerFactory;

        private readonly string _namePrefix;

        private readonly TState _state;

        private readonly BlockingCollection<TEntity>[] _queueList;

        private List<Task> _taskList = new List<Task>();

        private bool _isStarting = false;

        internal ParallelQueueConsumerOptions<TEntity, TState> Options => _options;

        public DefaultParallelQueueConsumer(
            ParallelQueueConsumerOptions<TEntity, TState> options,
            string namePrefix,
            TState state,
            ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options;
            _loggerFactory = loggerFactory;
            _namePrefix = namePrefix;
            _state = state;

            int executorCount = _options.ExecutorCount;
            if (_options.ExecutorCount < 1)
            {
                executorCount = 1;
            }

            var _logger = _loggerFactory.CreateLogger<ParallelQueueExecutor<TEntity, TState>>();

            List<BlockingCollection<TEntity>> queueList = new List<BlockingCollection<TEntity>>();
            for (int i = 0; i < executorCount; i++)
            {
                string name = $"{namePrefix}{i}";
                var executor = new ParallelQueueExecutor<TEntity, TState>(
                    _options,
                    _logger,
                    name,
                    state);
                _executorList.Add(executor);
                queueList.Add(executor.Queue);
            }
            _queueList = queueList.ToArray();
            
        }

        public long ExecutedCount
        {
            get
            {
                return _executorList.Sum(e => e.Counter);
            }
        }

        public double TotalLoad
        {
            get
            {
                List<QueueExecutorLoadInfo> loadList = _executorList.Select(e => e.GetCurrentLoad()).ToList();
                return loadList.Sum(li => li.Busy) / loadList.Max(li => (li.Idle + li.Busy));
            }
        }

        public IReadOnlyList<QueueExecutorLoadInfo> GetDetailLoadInfo()
        {

            List<QueueExecutorLoadInfo> loadList = _executorList.Select(e => e.GetCurrentLoad()).ToList();
            return loadList;

        }

        public Task StartAsync()
        {
            if (_isStarting)
            {
                return Task.CompletedTask;
            }

            foreach(var e in _executorList)
            {
                _taskList.Add(Task.Run(e.RunAsync));
            }
            _isStarting = true;

            return Task.CompletedTask;
        }

        public bool TryAdd(TEntity entity, TimeSpan timeout)
        {
            if (!_isStarting)
            {
                return false;
            }
            int idx = BlockingCollection<TEntity>.TryAddToAny(_queueList, entity, timeout);
            return idx < 0 ? false : true;
        }

        public Task StopAsync()
        {
            if (!_isStarting)
            {
                return Task.CompletedTask;
            }
            _isStarting = false;
            foreach (var queue in _queueList)
            {
                queue.CompleteAdding();
            }

            Task.WaitAll(_taskList.ToArray());
            return Task.CompletedTask;
        }

    }
}
