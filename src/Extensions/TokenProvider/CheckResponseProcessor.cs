using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 应答检查，判断是否令牌失效
    /// </summary>
    public abstract class CheckResponseProcessor
    {
        /// <summary>
        /// 检查应答，如果令牌无效，触发UnauthorizedAccessException异常
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public abstract Task CheckResponseAsync(HttpResponseMessage response);


        public static CheckResponseProcessor NormalChecker = new NormalResponseProcessor();


        public static DelegateCheckResponseProcessor CreateDelegateCheckResponseProcessor(Func<HttpResponseMessage, Task> checker)
        {
            return new DelegateCheckResponseProcessor(checker);
        }


        /// <summary>
        /// 通过查询字符串传递访问令牌
        /// </summary>
        public class NormalResponseProcessor : CheckResponseProcessor
        {
           
            public NormalResponseProcessor()
            {

            }

            public override Task CheckResponseAsync(HttpResponseMessage response)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                       response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("验证失败");
                }
                return Task.CompletedTask;
            }
        }

        public class DelegateCheckResponseProcessor : CheckResponseProcessor
        {
            private readonly Func<HttpResponseMessage, Task> _checker;
            public DelegateCheckResponseProcessor(Func<HttpResponseMessage, Task> checker)
            {
                _checker = checker;
            }

            public override Task CheckResponseAsync(HttpResponseMessage response)
            {
                if (_checker != null)
                {
                    return _checker(response);
                }
                return Task.CompletedTask;
            }
        }
    }
}
