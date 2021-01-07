using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class MemoryQueueProducer<TEntity> : IParallelQueueProducer<TEntity>
    {
        private readonly BlockingCollection<TEntity> _queue = new BlockingCollection<TEntity>();

        public Task StopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task<bool> TryAddAsync(TEntity entity, CancellationToken token)
        {
            bool isOK = _queue.TryAdd(entity);
            return Task.FromResult(isOK);
        }

        public Task<(TEntity, bool)> TryTakeAsync(TimeSpan timeout, CancellationToken token)
        {
            bool isOk = _queue.TryTake(out TEntity item, (int)timeout.TotalMilliseconds, token);
            return Task.FromResult((item, isOk));
        }
    }
}
