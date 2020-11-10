using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Xfrogcn.AspNetCore.Extensions
{
    internal class ClearLogsHostService : IHostedService
    {
        readonly WebApiConfig _config;
        System.Threading.Timer _timer;
        readonly ILogger<ClearLogsHostService> _logger;

        public ClearLogsHostService(
            WebApiConfig config,
            ILogger<ClearLogsHostService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _timer = new Timer(ClearProc, null, TimeSpan.FromHours(1), Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        private void ClearProc(object state)
        {

            try
            {
                if (_config.MaxLogDays <= 0)
                {
                    return;
                }

                _logger.LogInformation("开始清理日志");


            }
            catch (Exception e)
            {
                _logger.LogError(e, "清理日志时发生异常");
            }
            finally
            {
                _timer.Change(TimeSpan.FromHours(1), Timeout.InfiniteTimeSpan);
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            return Task.CompletedTask;
        }
    }
}
