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

        protected Func<TSource, CircularRefChecker, TTarget> _converter = null;

        protected Action<TSource, TTarget, CircularRefChecker> _copy = null;

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

        public TTarget Convert(TSource source, CircularRefChecker checker = null)
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

            return _converter(source, checker);
        }


        public void CopyTo(TSource source, TTarget target, CircularRefChecker checker = null)
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
            _copy.Invoke(source, target, checker);
        }

        public virtual Action<TSource, TTarget, CircularRefChecker> GenerateCopyToDelegate()
        {
            
            ParameterExpression sourcePar = Expression.Parameter(typeof(TSource));
            ParameterExpression targetPar = Expression.Parameter(typeof(TTarget));
            ParameterExpression crCheckerPar = Expression.Parameter(typeof(CircularRefChecker), "checker");

            Action<TSource, TTarget> converter = GenerateDefaultCopyToDelegate();

            List<Expression> expList = new List<Expression>();
            Expression checkAssign = Expression.Assign(
               crCheckerPar,
               Expression.Convert(Expression.Coalesce(crCheckerPar, Expression.New(typeof(CircularRefChecker))), typeof(CircularRefChecker)));

            expList.Add(checkAssign);

            expList.Add(Expression.Invoke(Expression.Constant(converter), sourcePar, targetPar, crCheckerPar));

            runConverter(sourcePar, targetPar, expList);


            return Expression.Lambda<Action<TSource, TTarget, CircularRefChecker>>(
                Expression.Block(
                expList
                ), sourcePar, targetPar, crCheckerPar).Compile();

        }

        public virtual Action<TSource, TTarget> GenerateDefaultCopyToDelegate()
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);

            ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            ParameterExpression targetPar = Expression.Parameter(tType, "target");
            ParameterExpression targetVar = Expression.Variable(tType);
            ParameterExpression crCheckerPar = Expression.Parameter(typeof(CircularRefChecker), "checker");
       

            var g = new PropertyAssignGenerator(sourcePar, targetVar, crCheckerPar, _provider);
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
                expList.AddRange(GeneratePropertyAssignExpression(sourcePar, targetPar, crCheckerPar, true));
            }
           

            return Expression.Lambda<Action<TSource, TTarget>>(
                Expression.Block(
                    new ParameterExpression[] { targetVar },
                    Expression.IfThen(
                        Expression.NotEqual(Expression.Constant(targetPar), Expression.Constant(null)),
                        Expression.Block( expList))
                ), sourcePar, targetPar, crCheckerPar).Compile();
        }

        public virtual Func<TSource, CircularRefChecker, TTarget> GenerateConvertDelegate()
        {
           
            ParameterExpression sourcePar = Expression.Parameter(typeof(TSource));
            ParameterExpression crCheckerPar = Expression.Parameter(typeof(CircularRefChecker), "checker");
            ParameterExpression targetVar = Expression.Variable(typeof(TTarget));

            Func<TSource, CircularRefChecker, TTarget> converter = GenerateDefaultConvertDelegate(crCheckerPar);

            List<Expression> expList = new List<Expression>();
            Expression checkAssign = Expression.Assign(
               crCheckerPar,
               Expression.Convert(Expression.Coalesce(crCheckerPar, Expression.New(typeof(CircularRefChecker))), typeof(CircularRefChecker)));

            expList.Add(checkAssign);

            expList.Add(Expression.Assign(targetVar, Expression.Invoke(Expression.Constant(converter), sourcePar, crCheckerPar)));

            runConverter(sourcePar, targetVar, expList);

            expList.Add(targetVar);

            return Expression.Lambda<Func<TSource, CircularRefChecker, TTarget>>(
                Expression.Block(
                new ParameterExpression[] { targetVar },
                expList
                ), sourcePar, crCheckerPar).Compile();

        }

        private void runConverter(ParameterExpression sourcePar, ParameterExpression targetVar,  List<Expression> expList)
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

        public virtual Func<TSource,CircularRefChecker, TTarget> GenerateDefaultConvertDelegate(ParameterExpression checkerPar)
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);

            ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            ParameterExpression targetVar = Expression.Variable(tType, "target");
            
            ParameterExpression cacheValueVar = Expression.Variable(tType, "cacheVal");

           

            var g = new PropertyAssignGenerator(sourcePar, targetVar, checkerPar, _provider);
            var exp = g.GenerateExpression();


            List<Expression> expList = new List<Expression>() { };

            if (exp != null)
            {
                // 非Class类型
                expList.Add(exp);
            }
            else
            {
               
                Expression cacheValueAssign = Expression.Assign(
                    cacheValueVar,
                    Expression.Convert(Expression.Call(checkerPar, CircularRefChecker.GetValueMethod, Expression.Convert(sourcePar, typeof(object)), Expression.Constant(tType)), tType));
                expList.Add(cacheValueAssign);

                List<Expression> subExpList = new List<Expression>();
                Expression newExpression = Expression.Assign(targetVar, Expression.New(tType));
                subExpList.Add(newExpression);
                subExpList.Add(Expression.Call(checkerPar, CircularRefChecker.SetInstanceMethod, sourcePar, Expression.Constant(tType), targetVar));
                subExpList.AddRange(GeneratePropertyAssignExpression(sourcePar, targetVar, checkerPar, false));

                expList.Add(
                    Expression.IfThenElse(
                        Expression.Equal(cacheValueVar, Expression.Constant(null)),
                        Expression.Block(subExpList),
                        Expression.Assign(targetVar, cacheValueVar)
                        ));
            }
            expList.Add(targetVar);

            Expression block = Expression.Block(
                    new ParameterExpression[] { targetVar, cacheValueVar },
                    expList
                );

            return Expression.Lambda<Func<TSource,CircularRefChecker, TTarget>>(block,sourcePar, checkerPar).Compile();
        }

        public virtual List<Expression> GeneratePropertyAssignExpression(ParameterExpression sourcePar, ParameterExpression targetPar, ParameterExpression checkerPar, bool isCopy= false, Dictionary<MemberInfo, Expression> excludeProperties = null)
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

                Expression excludeExpression = excludeProperties?.FirstOrDefault(p=>p.Key == pi || (p.Key.DeclaringType == pi.DeclaringType && p.Key.Name == pi.Name)).Value;
                if(excludeExpression?.NodeType == ExpressionType.MemberAccess ||
                    excludeExpression?.NodeType == ExpressionType.Constant)
                {
                    continue;
                }
                MemberInitExpression _dic = null;
                if(excludeExpression is MemberInitExpression mie)
                {
                    _dic = mie;
                }
                

                // 直接转换
                if (property != null && property.CanWrite)
                {
                    var directExp = ConvertProperty(sourcePar, checkerPar, pi, targetPar, property, isCopy, _dic);
                    if (directExp != null)
                    {
                        expList.Add(directExp);
                    }

                    property = null;
                }


                // 特性
                var sAttrs = pi.GetCustomAttributes<MapperPropertyNameAttribute>();
               // var tAttrs = pi.GetCustomAttributes<MapperPropertyNameAttribute>();
                var targetNames = sAttrs.Where(a => a.TargetType != null && a.TargetType.IsAssignableFrom(tType)).Select(a => a.Name).ToList();
                if (targetNames.Count > 0)
                {
                    property = tpis.FirstOrDefault(p => targetNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
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

                if (property == null || !property.CanWrite)
                {
                    continue;
                }


                var exp = ConvertProperty(sourcePar,checkerPar, pi, targetPar, property, isCopy, _dic);
                if (exp != null)
                {
                    expList.Add(exp);
                }


            }
            return expList;
        }

        
        public virtual Expression ConvertProperty(ParameterExpression source, ParameterExpression checkerPar, PropertyInfo spi, Expression target, PropertyInfo tpi, bool isCopy , MemberInitExpression excludeProperties)
        {
            ParameterExpression se = ParameterExpression.Variable(spi.PropertyType);
            ParameterExpression te = ParameterExpression.Variable(tpi.PropertyType);

            List<Expression> expList = new List<Expression>();
            

            PropertyAssignGenerator g = new PropertyAssignGenerator(se, te, checkerPar, _provider);
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
                            if (excludeProperties != null)
                            {
                                exp.Add(GenerateExcludeAction(spi.PropertyType, tType, se, te, excludeProperties));
                            }
                            else
                            {
                                
                                exp.Add(g.GenerateCopyToExpression());
                            }
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


        
        public Action<TSource, TTarget> DefineCopyTo(Expression<Func<TSource, object>> excludeProperties)
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);
            if(sType.IsClass && tType.IsClass && tType!=typeof(string) && sType != typeof(string))
            {
                Dictionary<MemberInfo, Expression> dic = new Dictionary<MemberInfo, Expression>();
                if (excludeProperties!=null)
                {

                    if(excludeProperties.Body is MemberExpression me)
                    {
                        dic.Add(me.Member, me);
                    }else if(excludeProperties.Body is NewExpression ne)
                    {
                        foreach(var a in ne.Arguments)
                        {
                            if(a is MemberExpression ame)
                            {
                                dic.Add(ame.Member, ame);
                            }
                        }
                    }else if(excludeProperties.Body is MemberInitExpression mie)
                    {
                        foreach(var a in mie.Bindings)
                        {
                            if(a is MemberAssignment mb)
                            {
                                dic.Add(a.Member, mb.Expression);
                            }
                            
                        }
                    }
                }

                return GenerateDefaultCopyToDelegateWithExclude(dic);
            }
            throw new InvalidOperationException("源与目标类型必须为类时，才可使用DefineCopyTo方法");
        }


        public virtual Expression GenerateExcludeAction(Type sourceType, Type targetType, ParameterExpression sourcePar, ParameterExpression targetPar, MemberInitExpression mie)
        {
            Dictionary<MemberInfo, Expression> dic = new Dictionary<MemberInfo, Expression>();
            foreach(var m in mie.Bindings)
            {
                if( m is MemberAssignment ma)
                {
                    dic.Add(ma.Member, ma.Expression);
                }
            }

            MethodInfo pmi = _provider.GetType().GetMethod("GetMapper");
            pmi = pmi.MakeGenericMethod(sourceType, targetType);

            var mapperType = typeof(IMapper<,>).MakeGenericType(sourceType, targetType);
            ParameterExpression mapperVar = ParameterExpression.Variable(mapperType);
            var mmaper = Expression.Assign(mapperVar, Expression.Call(
                Expression.Constant(_provider),
                pmi));

            MethodInfo mi = mapperType.GetMethod("GenerateDefaultCopyToDelegateWithExclude", BindingFlags.Public | BindingFlags.Instance);
            var actionType = typeof(Action<,>).MakeGenericType(sourceType, targetType);
            var actionVar = Expression.Variable(actionType);
            var action = Expression.Assign(
                actionVar,
                Expression.Call(mapperVar, mi, Expression.Constant(dic)));

            var invoke = Expression.Invoke(actionVar, sourcePar, targetPar);

            return Expression.Block(
                new ParameterExpression[] { mapperVar, actionVar },
                mmaper,
                action,
                invoke
                );

        }

        public virtual Action<TSource, TTarget> GenerateDefaultCopyToDelegateWithExclude(Dictionary<MemberInfo, Expression> excludeProperties)
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);

            ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            ParameterExpression targetPar = Expression.Parameter(tType, "target");
            ParameterExpression targetVar = Expression.Variable(tType);
            ParameterExpression checkerVar = Expression.Variable(typeof(CircularRefChecker));


            List<Expression> expList = new List<Expression>() { };
            expList.Add(Expression.Assign(checkerVar, Expression.New(typeof(CircularRefChecker))));

            // 类转换
            expList.AddRange(GeneratePropertyAssignExpression(sourcePar, targetPar, checkerVar, true, excludeProperties));


            return Expression.Lambda<Action<TSource, TTarget>>(
                Expression.Block(
                    new ParameterExpression[] { targetVar, checkerVar },
                    Expression.IfThen(
                        Expression.NotEqual(Expression.Constant(targetPar), Expression.Constant(null)),
                        Expression.Block(expList))
                ), sourcePar, targetPar).Compile();
        }
    }
}
