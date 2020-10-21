using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions.ParallelQueue
{
    public class ParallelQueueHostedService<TEntity, TState> : IHostedService
    {
        private readonly IParallelQueueProducerFactory _producerFactory;
        private readonly IParallelQueueConsumerFactory _consumerFactory;
        private readonly ILogger<ParallelQueueHostedService<TEntity, TState>> _logger;
        private readonly string _queueName;
        private readonly TState _state;

        private IParallelQueueConsumer<TEntity, TState> _consumer;
        private IParallelQueueProducer<TEntity> _producer;
        private CancellationTokenSource _stopToken;

        public ParallelQueueHostedService(
            IParallelQueueConsumerFactory consumerFactory,
            IParallelQueueProducerFactory producerFactory,
            string queuqName,
            TState state,
            ILogger<ParallelQueueHostedService<TEntity, TState>> logger)
        {
            if (consumerFactory == null)
                throw new ArgumentNullException(nameof(consumerFactory));
            if (producerFactory == null)
                throw new ArgumentNullException(nameof(producerFactory));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _consumerFactory = consumerFactory;
            _producerFactory = producerFactory;
            _queueName = queuqName;
            _state = state;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_producer == null)
            {
                _producer = _producerFactory.CreateProducer<TEntity>(_queueName);
            }
            if (_consumer == null)
            {
                _consumer = _consumerFactory.CreateConsumer<TEntity, TState>(_queueName, _state);
            }

            await _consumer.StartAsync();
            _stopToken = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await QueueReciver();
            });
        }


        private CancellationTokenSource _stoppingToken = null;
        public async Task QueueReciver()
        {
            _logger.LogInformation($"队列接收器已启动:{_queueName}");
            while (!_stopToken.IsCancellationRequested)
            {
                var (msg, isOk) = await _producer.TryTakeAsync(TimeSpan.FromSeconds(5), _stopToken.Token);
                if (msg != null && isOk)
                {
                    while (true)
                    {
                        var isAdded = _consumer.TryAdd(msg, TimeSpan.FromSeconds(10));
                        if (isAdded)
                        {
                            break;
                        }
                    }
                }
            }
            _logger.LogInformation($"队列接收器已退出:{_queueName}");
            _stoppingToken.Cancel();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingToken = new CancellationTokenSource();
            _stopToken.Cancel();
            await _producer.StopAsync(cancellationToken);
            await _consumer.StopAsync();
            _stoppingToken.Token.WaitHandle.WaitOne();
        }
    }
}
