using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 队列执行器负载情况
    /// </summary>
    public class QueueExecutorLoadInfo
    {
        /// <summary>
        /// 空闲时间
        /// </summary>
        public double Idle { get; set; }
        /// <summary>
        /// 执行时间
        /// </summary>
        public double Busy { get; set; }
        /// <summary>
        /// 负载率 Busy / (Idle + Busy)
        /// </summary>
        public double LoadRatio { get; set; }

        /// <summary>
        /// 区间内执行计数
        /// </summary>
        public long Counter { get; set; }

        /// <summary>
        /// 创建空负载
        /// </summary>
        /// <returns></returns>
        public static QueueExecutorLoadInfo CreateEmptyLoad(TimeSpan period)
        {
            return new QueueExecutorLoadInfo()
            {
                Idle = period.TotalMilliseconds,
                Busy = 0,
                LoadRatio = 0
            };
        }

    }
}
