using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public interface IMapper<TSource, TTarget>
    {
        /// <summary>
        /// 将source转换为TTarget类型
        /// </summary>
        /// <param name="source">源</param>
        /// <returns></returns>
        TTarget Convert(TSource source);

        /// <summary>
        /// 拷贝
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CopyTo(TSource source, TTarget target);
    }
}
