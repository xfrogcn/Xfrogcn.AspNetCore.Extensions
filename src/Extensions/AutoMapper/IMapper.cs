using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions.AutoMapper
{
    public interface IMapper<TSource, TTarget>
        where TSource: class
        where TTarget : new()
    {
        /// <summary>
        /// 将source转换为TTarget类型
        /// </summary>
        /// <param name="source">源</param>
        /// <returns></returns>
        TTarget Convert(TSource source);
    }
}
