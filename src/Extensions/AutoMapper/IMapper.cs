using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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

        /// <summary>
        /// 获取一个除去传入属性列表的CopyTo Action
        /// </summary>
        /// <param name="excludeProperties"></param>
        /// <returns></returns>
        Action<TSource, TTarget> DefineCopyTo(Expression<Func<TSource, object>> excludeProperties);

        Action<TSource, TTarget> GenerateDefaultCopyToDelegateWithExclude(Dictionary<MemberInfo, Expression> excludeProperties);
    }
}
