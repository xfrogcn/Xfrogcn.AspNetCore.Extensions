using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using Xfrogcn.AspNetCore.Extensions.AutoMapper;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class DefaultMapper<TSource, TTarget> : IMapper<TSource, TTarget>
        //where TSource : class
        //where TTarget : new()
    {

        private object locker = new object();

        protected Func<TSource, TTarget> _converter = null;

        protected Action<TSource, TTarget> _copy = null;

        private static Type[] numbericTypes = new Type[]
        {
            typeof(Byte), typeof(SByte),
            typeof(Int16), typeof(UInt16),
            typeof(Int32),typeof(UInt32),
            typeof(Int64), typeof(UInt64),
            typeof(Single),typeof(Double), 
            typeof(Decimal)
        };

        private readonly IMapperProvider _provider;
        private readonly MapperOptions _options;
        public DefaultMapper(
            IMapperProvider provider,
            IOptions<MapperOptions> options)
        {
            _provider = provider;
            _options = options.Value;
        }

        public TTarget Convert(TSource source)
        {
            if (_converter == null)
            {
                lock (locker)
                {
                    if (_converter == null)
                    {
                        _converter = GenerateConvertDelegate();
                    }
                }
            }

            return _converter(source);
        }

        public void CopyTo(TSource source, TTarget target)
        {
            if (_copy == null)
            {
                lock (locker)
                {
                    if (_copy == null)
                    {
                        _copy = GenerateCopyToDelegate();
                    }
                }
            }
            _copy.Invoke(source, target);
        }

        public virtual Action<TSource, TTarget> GenerateCopyToDelegate()
        {
            Action<TSource, TTarget> converter = GenerateDefaultCopyToDelegate();
            ParameterExpression sourcePar = Expression.Parameter(typeof(TSource));
            ParameterExpression targetPar = Expression.Parameter(typeof(TTarget));

            List<Expression> expList = new List<Expression>();
            expList.Add(Expression.Invoke(Expression.Constant(converter), sourcePar, targetPar));

            runConverter(sourcePar, targetPar, expList);


            return Expression.Lambda<Action<TSource, TTarget>>(
                Expression.Block(
                expList
                ), sourcePar, targetPar).Compile();

        }

        public virtual Action<TSource, TTarget> GenerateDefaultCopyToDelegate()
        {
            return (s, t) =>
            {
            };
            //Type sType = typeof(TSource);
            //Type tType = typeof(TTarget);

            //ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            //ParameterExpression targetPar = Expression.Parameter(tType, "target");

            //List<Expression> expList = new List<Expression>() { };

            //expList.AddRange(GeneratePropertyAssignExpression(sourcePar, targetPar));

            //return Expression.Lambda<Action<TSource, TTarget>>(
            //    Expression.Block(
            //        expList
            //    ), sourcePar, targetPar).Compile();
        }

        public virtual Func<TSource, TTarget> GenerateConvertDelegate()
        {
            Func<TSource, TTarget> converter = GenerateDefaultConvertDelegate();
            ParameterExpression sourcePar = Expression.Parameter(typeof(TSource));
            ParameterExpression targetVar = Expression.Variable(typeof(TTarget));

            List<Expression> expList = new List<Expression>();
            expList.Add(Expression.Assign(targetVar, Expression.Invoke(Expression.Constant(converter), sourcePar)));

            runConverter(sourcePar, targetVar, expList);

            expList.Add(targetVar);

            return Expression.Lambda<Func<TSource, TTarget>>(
                Expression.Block(
                new ParameterExpression[] { targetVar },
                expList
                ), sourcePar).Compile();

        }

        private void runConverter(ParameterExpression sourcePar, ParameterExpression targetVar, List<Expression> expList)
        {
            var list = _options?.GetConverter<TSource, TTarget>();
            if (list != null && list.Count > 0)
            {
                foreach (var m in list)
                {
                    var constM = Expression.Constant(m);
                    PropertyInfo pi = m.GetType().GetProperty("Convert", BindingFlags.Public | BindingFlags.Instance);
                    if (pi != null)
                    {
                        Expression piAccess = Expression.MakeMemberAccess(constM, pi);
                        expList.Add(Expression.IfThen(
                            Expression.NotEqual(piAccess, Expression.Convert(Expression.Constant(null), pi.PropertyType)),
                            Expression.Invoke(piAccess, Expression.Constant(_provider), sourcePar, targetVar)));
                    }
                }
            }
        }

        public virtual Func<TSource, TTarget> GenerateDefaultConvertDelegate()
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);

            ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            ParameterExpression targetVar = Expression.Variable(tType, "target");


            var g = new PropertyAssignGenerator(sourcePar, targetVar, _provider);
            var exp = g.GenerateExpression();


            List<Expression> expList = new List<Expression>() { };

         

            if (exp != null)
            {
                expList.Add(exp);
            }
            else
            {
                Expression newExpression = Expression.Assign(targetVar, Expression.New(tType));
                expList.Add(newExpression);
                // 类转换
                expList.AddRange(GeneratePropertyAssignExpression(sourcePar, targetVar, false));
            }
            expList.Add(targetVar);


            return Expression.Lambda<Func<TSource, TTarget>>(
                Expression.Block(
                    new ParameterExpression[] { targetVar },
                    expList
                ), sourcePar).Compile();
        }

        public virtual List<Expression> GeneratePropertyAssignExpression(ParameterExpression sourcePar, ParameterExpression targetPar, bool isCopy=false)
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);
            List<PropertyInfo> spis = sType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).ToList();
            List<PropertyInfo> tpis = tType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            List<Expression> expList = new List<Expression>() { };
            foreach (var pi in spis)
            {
                var property = tpis.FirstOrDefault(p => p.Name == pi.Name);
                property = property ?? tpis.FirstOrDefault(p => p.Name.Equals(pi.Name, StringComparison.OrdinalIgnoreCase));

                // 直接转换
                if (property != null && property.CanWrite)
                {
                    var directExp = ConvertProperty(sourcePar, pi, targetPar, property, isCopy);
                    if (directExp != null)
                    {
                        expList.Add(directExp);
                    }

                    property = null;
                }


                // 特性
                var sAttrs = pi.GetCustomAttributes<MapperPropertyNameAttribute>();
                var tAttrs = pi.GetCustomAttributes<MapperPropertyNameAttribute>();
                var targetNames = sAttrs.Where(a => a.TargetType != null && a.TargetType.IsAssignableFrom(tType)).Select(a => a.Name).ToList();
                if (targetNames.Count > 0)
                {
                    property = tpis.FirstOrDefault(p => targetNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                }
                if (property == null)
                {
                    targetNames = tAttrs.Where(a => a.SourceType != null && a.SourceType.IsAssignableFrom(sType)).Select(a => a.Name).ToList();
                    if (targetNames.Count > 0)
                    {
                        property = tpis.FirstOrDefault(p => targetNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                    }
                }
                if (property == null)
                {
                    // 默认
                    targetNames = sAttrs.Where(a => a.TargetType == null).Select(a => a.Name).ToList();
                    if (targetNames.Count > 0)
                    {
                        property = tpis.FirstOrDefault(p => targetNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                    }
                }
                if (property == null)
                {
                    // 默认
                    targetNames = tAttrs.Where(a => a.SourceType == null).Select(a => a.Name).ToList();
                    if (targetNames.Count > 0)
                    {
                        property = tpis.FirstOrDefault(p => targetNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                    }
                }

                if (property == null || !property.CanWrite)
                {
                    continue;
                }


                var exp = ConvertProperty(sourcePar, pi, targetPar, property, isCopy);
                if (exp != null)
                {
                    expList.Add(exp);
                }


            }
            return expList;
        }

        /// <summary>
        /// 当转换类型为字典或列表时，可封装成ValueWrapper类型来转换
        /// </summary>
        /// <returns></returns>
        //public virtual Func<TSource, TTarget> GenerirWrapperConvertDelegate()
        //{
        //    var keyMapper = _provider.GetMapper<ValueWrapper<TSource>, ValueWrapper<TTarget>>();

        //    return (s) =>
        //    {
        //        return keyMapper.Convert(new ValueWrapper<TSource>() { Value = s }).Value;
        //    };
        //}

        public virtual Expression ConvertProperty(ParameterExpression source, PropertyInfo spi, Expression target, PropertyInfo tpi, bool isCopy)
        {
            ParameterExpression se = ParameterExpression.Variable(spi.PropertyType);
            ParameterExpression te = ParameterExpression.Variable(tpi.PropertyType);

            List<Expression> expList = new List<Expression>();
            

            PropertyAssignGenerator g = new PropertyAssignGenerator(se, te, _provider);
            Expression e = g.GenerateExpression(true);
            if (e != null)
            {
                expList.Add(Expression.Assign(se, Expression.MakeMemberAccess(source, spi)));
                if (isCopy)
                {
                    //拷贝
                }
                else
                {
                    expList.Add(e);
                    expList.Add(Expression.Assign(
                        Expression.MakeMemberAccess(target, tpi),
                        te));

                    return Expression.Block(
                        new ParameterExpression[] { se, te },
                        expList);
                }
            }

            return null;
        }

        //public virtual Expression ConvertClassType(ParameterExpression source, PropertyInfo spi, Expression target, PropertyInfo tpi)
        //{
        //    if(spi.PropertyType.IsInterface && tpi.PropertyType.IsInterface &&
        //        spi.PropertyType == tpi.PropertyType)
        //    {
        //        return Expression.Assign(
        //               Expression.MakeMemberAccess(target, tpi),
        //               Expression.MakeMemberAccess(source, spi)
        //               );
        //    }

        //    if(spi.PropertyType.IsClass && tpi.PropertyType.IsClass)
        //    {
        //        if(spi.PropertyType==typeof(string) && tpi.PropertyType == typeof(string))
        //        {
        //            return Expression.Assign(
        //               Expression.MakeMemberAccess(target, tpi),
        //               Expression.MakeMemberAccess(source, spi)
        //               );
        //        }

        //        var exp = ConvertIDictionaryType(source, spi, target, tpi);
        //        if(exp !=null)
        //        {
        //            return exp;
        //        }
        //        exp = ConvertIListType(source, spi, target, tpi);
        //        if(exp !=null)
        //        {
        //            return exp;
        //        }

        //        if(spi.PropertyType == typeof(string) || tpi.PropertyType == typeof(string))
        //        {
        //            return null;
        //        }

        //        Expression provider = Expression.Constant(_provider);
        //        MethodInfo mi = _provider.GetType().GetMethod(nameof(IMapperProvider.GetMapper));

        //        mi = mi.MakeGenericMethod(spi.PropertyType, tpi.PropertyType);
        //        var mapperType = typeof(IMapper<,>).MakeGenericType(spi.PropertyType, tpi.PropertyType);
        //        ParameterExpression mapperVar = Expression.Variable(mapperType);
        //        Expression assign = Expression.Assign(mapperVar, Expression.Call(provider, mi));

        //        MethodInfo convertMethod = mapperType.GetMethod("Convert");

        //        var assign2 = Expression.Assign(
        //            Expression.MakeMemberAccess(target, tpi),
        //            Expression.Call(mapperVar, convertMethod, Expression.MakeMemberAccess(source, spi)));

        //        var ifExp = Expression.IfThen(
        //            Expression.NotEqual(Expression.MakeMemberAccess(source, spi), Expression.Constant(null)),
        //            Expression.Block(
        //            new ParameterExpression[] { mapperVar },
        //            assign,
        //            assign2
        //            ));

        //        return ifExp;
        //    }
        //    return null;
        //}

        //public virtual Expression ConvertIDictionaryType(ParameterExpression source, PropertyInfo spi, Expression target, PropertyInfo tpi)
        //{
        //    bool isSourceDic = IsDictionaryType(spi.PropertyType);
        //    bool isTargetDic = IsDictionaryType(tpi.PropertyType);
        //    if( isSourceDic && isTargetDic)
        //    {
        //        var sTypes = spi.PropertyType.GetGenericArguments();
        //        var tType = tpi.PropertyType.GetGenericArguments();

        //        MethodInfo mi = this.GetType().GetMethod(nameof(DictionayConvert), BindingFlags.Instance | BindingFlags.NonPublic);
        //        mi = mi.MakeGenericMethod(sTypes[0], sTypes[1], tpi.PropertyType, tType[0], tType[1]);

        //        return Expression.Assign(
        //            Expression.MakeMemberAccess(target, tpi),
        //            Expression.Call(
        //                Expression.Constant(this),
        //                mi,
        //                Expression.MakeMemberAccess(source, spi)));
        //    }
        //    return null;
        //}

        //public virtual bool IsDictionaryType(Type type)
        //{
        //    var dicType = typeof(IDictionary<,>);
        //    bool isDic = false;
        //    if (type.IsGenericType)
        //    {
        //        isDic = type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == dicType);
        //    }

        //    return isDic;
        //}


        //private TTargetDicValue DictionayConvert<TKey, TValue, TTargetDicValue, TTargetKey, TTargetValue>(IDictionary<TKey, TValue> source)
        //     where TTargetDicValue : IDictionary<TTargetKey, TTargetValue>, new()
        //{
        //    if (source == null)
        //    {
        //        return default;
        //    }
        //    TTargetDicValue dic = new TTargetDicValue();
        //    var keyMapper = _provider.GetMapper<ValueWrapper<TKey>, ValueWrapper<TTargetKey>>();
        //    var valueMapper = _provider.GetMapper<ValueWrapper<TValue>, ValueWrapper<TTargetValue>>();
        //    foreach (var kv in source)
        //    {
        //        var tKeyVal = keyMapper.Convert(new ValueWrapper<TKey>() { Value = kv.Key }).Value;
        //        if (tKeyVal == null)
        //        {
        //            continue;
        //        }
        //        var tVal = valueMapper.Convert(new ValueWrapper<TValue>() { Value = kv.Value }).Value;
        //        dic.Add(tKeyVal, tVal);
        //    }
        //    return dic;
        //}


        //public virtual Expression ConvertIListType(ParameterExpression source, PropertyInfo spi, Expression target, PropertyInfo tpi)
        //{
        //    bool isSourceList = IsEnumerableType(spi.PropertyType);
        //    bool isTargetList = IsListType(tpi.PropertyType);
        //    if (isSourceList && (isTargetList || tpi.PropertyType.IsArray) )
        //    {
        //        var sType = typeof(object);
        //        if(spi.PropertyType.IsArray)
        //        {
        //            sType = spi.PropertyType.GetElementType();
        //        }else if (spi.PropertyType.IsGenericType)
        //        {
        //            sType = spi.PropertyType.GetGenericArguments()[0];
        //        }
        //        var tType = typeof(object);
        //        if (tpi.PropertyType.IsGenericType)
        //        {
        //            tType = tpi.PropertyType.GetGenericArguments()[0];
        //        }
        //        else if (tpi.PropertyType.IsArray)
        //        {
        //            tType = tpi.PropertyType.GetElementType();
        //        }
        //        if(tType == typeof(object))
        //        {
        //            tType = sType;
        //        }

        //        MethodInfo mi = this.GetType().GetMethod(nameof(ListConvert), BindingFlags.Instance | BindingFlags.NonPublic);
        //        mi = mi.MakeGenericMethod(sType, tpi.PropertyType, tType);

        //        Expression tlistInstance = Expression.Convert(Expression.Constant(null), tpi.PropertyType);
        //        if (isTargetList)
        //        {
        //            // 先New
        //            tlistInstance = Expression.New(tpi.PropertyType);
        //        }

        //        return Expression.Assign(
        //            Expression.MakeMemberAccess(target, tpi),
        //            Expression.Call(
        //                Expression.Constant(this),
        //                mi,
        //                Expression.MakeMemberAccess(source, spi), tlistInstance));
        //    }
        //    return null;
        //}

        //public virtual bool IsListType(Type type)
        //{
        //    var listType = typeof(IList<>);
        //    bool isList = false;
        //    if (type.IsGenericType)
        //    {
        //        isList = type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == listType);
        //    }
        //    return isList;
        //}

        //public virtual bool IsEnumerableType(Type type)
        //{
        //    var listType = typeof(IEnumerable<>);
        //    bool isList = false;
        //    if( type.IsArray && type.GetElementType()!= typeof(object))
        //    {
        //        return true;
        //    }
        //    if (type.IsGenericType)
        //    {
        //        isList = type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == listType);
        //    }
        //    return isList;
        //}

        //private TTargetList ListConvert<TItem, TTargetList, TTargetItem>(IEnumerable<TItem> source, TTargetList tlist)
        //{
        //    if(source == null)
        //    {
        //        return default;
        //    }

        //    var mapper = _provider.GetMapper<ValueWrapper<TItem>, ValueWrapper<TTargetItem>>();
        //    if (typeof(TTargetList).IsArray)
        //    {
        //        var list = new TTargetItem[source.Count()];
        //        var itor = source.GetEnumerator();
        //        int i = 0;
        //        while (itor.MoveNext())
        //        {
        //            list[i] = mapper.Convert(new ValueWrapper<TItem>() { Value = itor.Current }).Value;
        //            i++;
        //        }
        //        return (TTargetList)(object)list;
        //    }
        //    else if (IsListType(typeof(TTargetList)))
        //    {
        //        IList list = tlist as IList;
        //        if(list !=null)
        //        {
        //            var itor = source.GetEnumerator();
        //            while (itor.MoveNext())
        //            {
        //                list.Add(mapper.Convert(new ValueWrapper<TItem>() { Value = itor.Current }).Value);
        //            }
        //        }
        //        return tlist;
        //    }
        //    return default;
        //}


        //public virtual Expression ConvertNullableType(ParameterExpression source, PropertyInfo spi, Expression target, PropertyInfo tpi)
        //{
        //    bool isTargetNullable = tpi.PropertyType.IsGenericType && tpi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        //    bool isSourceNullable = spi.PropertyType.IsGenericType && spi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        //    if (isTargetNullable || isSourceNullable)
        //    {
        //        // 如果source等于null，不赋值给目标
        //        // 原值
        //        var svalVar = Expression.Variable(spi.PropertyType);
        //        var sval = Expression.Assign(svalVar, Expression.MakeMemberAccess(source, spi));

        //        var sValType = isSourceNullable ? spi.PropertyType.GetGenericArguments()[0] : spi.PropertyType;
        //        var tValType = isTargetNullable ? tpi.PropertyType.GetGenericArguments()[0] : tpi.PropertyType;

        //        Expression body = ConvertValueType(source, spi, sValType, target, tpi, tValType);
        //        if (body != null)
        //        {
        //            if(isSourceNullable)
        //            {
        //                return Expression.Block(
        //                new ParameterExpression[] { svalVar },
        //                sval,
        //                Expression.IfThen(
        //                Expression.MakeBinary(ExpressionType.NotEqual, svalVar, Expression.Constant(null)),
        //                body));
        //            }
        //            else
        //            {
        //                return body;
        //            }

        //        }
        //    }
        //    return null;

        //}

        //public virtual Expression ConvertValueType(ParameterExpression source, PropertyInfo spi, Type sourceType, Expression target, PropertyInfo tpi, Type targetType)
        //{
        //    if (!sourceType.IsValueType || !targetType.IsValueType)
        //    {
        //        return null;
        //    }

        //    if (spi.PropertyType == tpi.PropertyType)
        //    {
        //        return Expression.Assign(
        //            Expression.MakeMemberAccess(target, tpi),
        //            Expression.MakeMemberAccess(source, spi)
        //            );
        //    }


        //    var exp = ConvertNumbericType(source, spi, sourceType, target, tpi, targetType);
        //    if(exp == null)
        //    {
        //        //其他值类型，如结构体，尚未处理
        //    }

        //    return exp;

        //}



        //public virtual Expression ConvertNumbericType(ParameterExpression source, PropertyInfo spi, Type sourceType, Expression target, PropertyInfo tpi, Type targetType)
        //{
        //    if (numbericTypes.Contains(sourceType) && numbericTypes.Contains(targetType))
        //    {
        //        bool isNullableSource = spi.PropertyType.IsGenericType && spi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

        //        PropertyInfo valueProperty = null;
        //        if (isNullableSource)
        //        {
        //            valueProperty = spi.PropertyType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
        //        }


        //        // 原值
        //        var svalVar = Expression.Variable(sourceType);
        //        var sval = Expression.Assign(
        //            svalVar,
        //            isNullableSource? Expression.MakeMemberAccess(Expression.MakeMemberAccess(source, spi), valueProperty) :
        //            Expression.MakeMemberAccess(source, spi));

        //        // 最大值与最小值
        //        var tminVar = Expression.Variable(targetType);
        //        var tmaxVar = Expression.Variable(targetType);

        //        var minFieldInfo = targetType.GetField(nameof(byte.MinValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        //        var maxFieldInfo = targetType.GetField(nameof(byte.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        //        var minVal = Expression.Assign(tminVar, Expression.MakeMemberAccess(null, minFieldInfo));
        //        var maxVal = Expression.Assign(tmaxVar, Expression.MakeMemberAccess(null, maxFieldInfo));

        //        Type decimalType = typeof(Decimal);
        //        var assign = Expression.IfThen(
        //        Expression.AndAlso(
        //            Expression.MakeBinary(ExpressionType.GreaterThanOrEqual,Expression.Convert( svalVar, decimalType),Expression.Convert( tminVar,decimalType)),
        //            Expression.MakeBinary(ExpressionType.LessThanOrEqual, Expression.Convert( svalVar,decimalType), Expression.Convert( tmaxVar,decimalType))),
        //        Expression.Assign(
        //             Expression.MakeMemberAccess(target, tpi),
        //             Expression.Convert( svalVar, tpi.PropertyType)));

        //       return Expression.Block(
        //            new ParameterExpression[] { svalVar, tminVar, tmaxVar },
        //            sval, minVal, maxVal, assign
        //            );

        //    }

        //    return null;
        //}
    }
}
