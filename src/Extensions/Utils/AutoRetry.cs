using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class AutoRetry : IAutoRetry
    {
        private readonly ILogger<AutoRetry> _logger = null; 
        public AutoRetry(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<AutoRetry>();
        }

 
        public async Task Retry(Func<Task> proc, int retryCount = 3, int delay = 100, bool throwError = true)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    await proc();
                    break;
                }
                catch (System.Exception e)
                {
                    _logger.LogWarning($"执行失败，自动重试, {i}， 异常：\r\n{e.ToString()}");
                    if (i == (retryCount-1))
                    {
                        if (throwError)
                            throw;
                        else
                            break;
                    }
                }
            }
        }

        public async Task<TResult> Retry<TResult>(Func<Task<TResult>> proc, Func<TResult, bool> checkResult = null, int retryCount = 3, int delay = 100, bool throwError = true)
        {
            Func<TResult, bool> checkProc = checkResult ?? (a => true);
            TResult r = default(TResult);
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    r =await proc();
                    if (checkProc != null)
                    {
                        if (checkProc(r))
                        {
                            break;
                        }
                        else
                        {
                            _logger.LogWarning($"执行失败，自动重试, {i}");
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                catch (System.Exception e)
                {
                    _logger.LogWarning($"执行失败，自动重试, {i}， 异常：\r\n{e.ToString()}");
                    if (i == (retryCount - 1))
                    {
                        if (throwError)
                            throw;
                        else
                            break;
                    }
                }
            }
            return r;
        }

        public TResult Retry<TResult>(Func<TResult> proc, Func<TResult, bool> checkResult = null, int retryCount = 3, int delay = 100, bool throwError = true)
        {
            Func<TResult, bool> checkProc = checkResult ?? (a => true);
            TResult r = default(TResult);
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    r = proc();
                    if (checkProc != null)
                    {
                        if (checkProc(r))
                        {
                            break;
                        }
                        else
                        {
                            _logger.LogWarning($"执行失败，自动重试, {i}");
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                catch (System.Exception e)
                {
                    _logger.LogWarning($"执行失败，自动重试, {i}， 异常：\r\n{e.ToString()}");
                    if (i == retryCount)
                    {
                        if (throwError)
                            throw;
                        else
                            break;
                    }
                }
            }
            return r;
        }

        public void Retry(Action proc, int retryCount = 3, int delay = 100, bool throwError = true)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    proc();
                    break;
                }
                catch (System.Exception e)
                {
                    _logger.LogWarning($"执行失败，自动重试, {i}， 异常：\r\n{e.ToString()}");
                    if (i == retryCount)
                    {
                        if (throwError)
                            throw;
                        else
                            break;
                    }
                }
            }
        }
    }


}

