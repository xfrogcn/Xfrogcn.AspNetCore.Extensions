using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace ParallelQueueExample.Handlers
{
    class MessageHandlerB : QueueHandlerBase<NotifyMessage>
    {
        readonly ILogger<MessageHandlerB> _logger;
        public MessageHandlerB(ILogger<MessageHandlerB> logger)
        {
            _logger = logger;
        }

        // 排在A的后面
        public override int Order => base.Order + 1;

        public override async Task Process(QueueHandlerContext<NotifyMessage, object> context)
        {
            context.Message.Output = context.Message.Output + 1;

            _logger.LogInformation("{name}-->{output}", context.QueueName, context.Message.Output);

            await Task.Delay(500);

        }
    }
}
