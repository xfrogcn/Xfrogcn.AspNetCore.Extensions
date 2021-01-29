using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace ParallelQueueExample.Handlers
{
    class MessageHandlerB : QueueHandlerBase<NotifyMessage>
    {
        // 排在A的后面
        public override int Order => base.Order + 1;

        public override Task Process(QueueHandlerContext<NotifyMessage, object> context)
        {
            context.Message.Output = context.Message.Output + 1;
            return base.Process(context);
        }
    }
}
