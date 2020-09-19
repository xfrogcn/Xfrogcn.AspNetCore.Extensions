namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 映射转换
    /// </summary>
    public interface IMapperProvider
    {
        /// <summary>
        /// 获取TSource-->TTarget的映射转换器
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <returns></returns>
        IMapper<TSource, TTarget> GetMapper<TSource, TTarget>()
            where TSource: class
            where TTarget : new();

    }
}
