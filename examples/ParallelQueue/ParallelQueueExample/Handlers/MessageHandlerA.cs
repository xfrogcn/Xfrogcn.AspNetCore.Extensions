using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;

namespace ParallelQueueExample.Handlers
{
    /// <summary>
    /// 消息处理器，从QueueHandlerBase继承，实现对单个消息的处理
    /// </summary>
    class MessageHandlerA : QueueHandlerBase<NotifyMessage>
    { 
        /// <summary>
        /// 顺序，越小的排在前面
        /// </summary>
        public override int Order => base.Order;

        public override Task Process(QueueHandlerContext<NotifyMessage, object> context)
        {
            // 示例： 演示消息处理管道
            context.Message.Output = context.Message.Input + 1;
            return base.Process(context);
        }
    }
}
