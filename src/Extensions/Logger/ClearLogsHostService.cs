using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class ClearLogsHostService : IHostedService
    {
        readonly WebApiConfig _config;
        System.Threading.Timer _timer;
        readonly IOptionsMonitor<WebApiConfig> _monitor;
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<ClearLogsHostService> _logger;

        readonly string[] logExtensions = new string[] { ".log", ".txt" };

        public ClearLogsHostService(
            WebApiConfig config,
            IOptionsMonitor<WebApiConfig> monitor, 
            IServiceProvider serviceProvider,
            ILogger<ClearLogsHostService> logger)
        {
            _monitor = monitor;
            _serviceProvider = serviceProvider;
            _config = config;
            _logger = logger;

            monitor.OnChange(onConfigChanged);
        }

        private void onConfigChanged(WebApiConfig config, string name)
        {
            WebApiHostBuilderExtensions._configAction?.Invoke(config);
            WebApiConfig old = _serviceProvider.GetRequiredService<WebApiConfig>();
            if (old != config)
            {
                IMapperProvider mapper = _serviceProvider.GetRequiredService<IMapperProvider>();
                mapper.CopyTo<WebApiConfig, WebApiConfig>(config, old);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _timer = new Timer(ClearProc, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        public void ClearProc(object state)
        {

            try
            {
                if (_config.MaxLogDays <= 0)
                {
                    return;
                }

                _logger.LogInformation("开始清理日志");

                string dir = _config.GetLogPath();

                DirectoryInfo di = new DirectoryInfo(dir);
                if (!di.Exists)
                {
                    return;
                }

                DateTime now = DateTime.UtcNow;
                DateTime startTime = now.AddDays(-_config.MaxLogDays);

                // 获取所有子目录
                List<DirectoryInfo> folders = new List<DirectoryInfo>();
                folders.Add(di);
                folders.AddRange(di.GetDirectories("*", SearchOption.AllDirectories));
                folders = folders.OrderByDescending(d => d.FullName.Length).ToList();

                for (int i = 0; i < folders.Count; i++)
                {
                    DirectoryInfo item = folders[i];
                    FileSystemInfo[] files = item.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
                    foreach (var f in files)
                    {
                        if (logExtensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase) && 
                            f.LastWriteTimeUtc<= startTime)
                        {
                            string relativePath = Path.GetRelativePath(di.FullName, f.FullName);
                            _logger.LogInformation("删除日志文件：{0}", relativePath);
                            try
                            {
                                File.Delete(f.FullName);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "删除日志文件失败, {0}", relativePath);
                            }
                        }
                    }

                    // 是否空文件夹
                    int fileCount = item.GetFiles("*", SearchOption.TopDirectoryOnly).Length;
                    if (fileCount == 0)
                    {
                        string relativePath = Path.GetRelativePath(di.FullName, item.FullName);
                        try
                        {
                            _logger.LogInformation("删除空目录：{0}", relativePath);
                            item.Delete();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "删除空目录失败：{0}", relativePath);
                        }
                    }
                }
                _logger.LogInformation("已清理完毕");
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
            Log.CloseAndFlush();
            return Task.CompletedTask;
        }
    }
}
