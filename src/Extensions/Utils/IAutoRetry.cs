using System;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public interface IAutoRetry
    {
        Task<TResult> Retry<TResult>(Func<Task<TResult>> proc, Func<TResult, bool> checkResult = null, int retryCount = 3, int delay = 100, bool throwError = true);
        TResult Retry<TResult>(Func<TResult> proc, Func<TResult, bool> checkResult = null, int retryCount = 3, int delay = 100, bool throwError = true);

        Task Retry(Func<Task> proc, int retryCount = 3, int delay = 100, bool throwError = true);

        void Retry(Action proc, int retryCount = 3, int delay = 100, bool throwError = true);
    }
}