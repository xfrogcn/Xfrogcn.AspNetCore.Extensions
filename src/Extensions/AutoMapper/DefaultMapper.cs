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
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);

            ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            ParameterExpression targetPar = Expression.Parameter(tType, "target");
            ParameterExpression targetVar = Expression.Variable(tType);


            var g = new PropertyAssignGenerator(sourcePar, targetVar, _provider);
            var exp = g.GenerateExpression();


            List<Expression> expList = new List<Expression>() { };

            if (exp != null)
            {
                if (PropertyAssignGenerator.IsDictionaryType(tType))
                {
                    var argTypes = tType.GetGenericArguments();
                    MethodInfo mi = this.GetType().GetMethod("CopyToDictionay");
                    mi = mi.MakeGenericMethod(argTypes[0], argTypes[1]);

                    expList.Add(exp);
                    expList.Add(Expression.Call(Expression.Constant(this), mi, targetVar, targetPar));
                    
                }
                else if (PropertyAssignGenerator.IsListType(tType))
                {
                    var argTypes = tType.GetGenericArguments();
                    MethodInfo mi = this.GetType().GetMethod("CopyToList");
                    mi = mi.MakeGenericMethod(argTypes[0]);

                    expList.Add(exp);
                    expList.Add(Expression.Call(Expression.Constant(this), mi, targetVar, targetPar));
                   
                }
                else
                {
                    // 单值不存在CopyTo
                    return (s, t) => { };
                }
            }
            else
            {
                // 类转换
                expList.AddRange(GeneratePropertyAssignExpression(sourcePar, targetPar, true));
            }
           

            return Expression.Lambda<Action<TSource, TTarget>>(
                Expression.Block(
                    new ParameterExpression[] { targetVar},
                    Expression.IfThen(
                        Expression.NotEqual(Expression.Constant(targetPar), Expression.Constant(null)),
                        Expression.Block( expList))
                ), sourcePar, targetPar).Compile();
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
                    Type tType = tpi.PropertyType;
                    if ((tType.IsClass && tType != typeof(string)) ||
                        PropertyAssignGenerator.IsDictionaryType(tType) ||
                        PropertyAssignGenerator.IsListType(tType))
                    {
                        

                        List<Expression> exp = new List<Expression>();
                        if (tType.IsClass)
                        {
                            exp.Add(Expression.Assign(te, Expression.MakeMemberAccess(target, tpi)));
                            exp.Add(g.GenerateCopyToExpression());
                        }
                        else if (PropertyAssignGenerator.IsDictionaryType(tType))
                        {
                            var argTypes = tType.GetGenericArguments();
                            MethodInfo mi = this.GetType().GetMethod("CopyToDictionay");
                            mi = mi.MakeGenericMethod(argTypes[0], argTypes[1]);
                            exp.Add(e);
                            exp.Add(Expression.Call(Expression.Constant(this), mi, te, Expression.MakeMemberAccess(target, tpi)));
                        }
                        else if (PropertyAssignGenerator.IsListType(tType))
                        {
                            var argTypes = tType.GetGenericArguments();
                            MethodInfo mi = this.GetType().GetMethod("CopyToList");
                            mi = mi.MakeGenericMethod(argTypes[0]);
                            exp.Add(e);
                            exp.Add(Expression.Call(Expression.Constant(this), mi, te, Expression.MakeMemberAccess(target, tpi)));
                        }

                        // 如果te等于null，new
                        expList.Add(Expression.IfThenElse(
                            Expression.Equal(Expression.MakeMemberAccess(target, tpi), Expression.Constant(null)),
                            Expression.Block(
                                e,
                                Expression.Assign(
                                    Expression.MakeMemberAccess(target, tpi),
                                    te)
                            ),
                            Expression.IfThenElse(
                                Expression.Equal(se, Expression.Constant(null)),
                                Expression.Assign(Expression.MakeMemberAccess(target, tpi), Expression.Constant(null, tpi.PropertyType)),
                                Expression.Block(exp)
                                )
                            
                            ));

                        return Expression.Block(
                            new ParameterExpression[] { se, te },
                            expList);

                    }

                }

                expList.Add(e);
                expList.Add(Expression.Assign(
                    Expression.MakeMemberAccess(target, tpi),
                    te));

                return Expression.Block(
                    new ParameterExpression[] { se, te },
                    expList);

            }

            return null;
        }


        public virtual void CopyToDictionay<TKey,TValue>(IDictionary<TKey,TValue> source, IDictionary<TKey,TValue> target)
        {
            if (source != null)
            {
                target?.Clear();
                foreach(var kv in source)
                {
                    target?.Add(kv.Key, kv.Value);
                }
            }
        }

        public virtual void CopyToList<TItem>(IEnumerable<TItem> source, IList<TItem> list)
        {
            if (source != null)
            {
                list?.Clear();
                var itor = source.GetEnumerator();
                while (itor.MoveNext())
                {
                    list?.Add(itor.Current);
                }
            }
        }
    }
}
