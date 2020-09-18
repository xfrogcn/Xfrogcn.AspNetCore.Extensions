using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xfrogcn.AspNetCore.Extensions.AutoMapper;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class DefaultMapper<TSource, TTarget> : IMapper<TSource, TTarget>
        where TSource : class
        where TTarget : new()
    {
        private object locker = new object();

        private static Type[] numbericTypes = new Type[]
        {
            typeof(Byte), typeof(SByte),
            typeof(Int16), typeof(UInt16),
            typeof(Int32),typeof(UInt32),
            typeof(Int64), typeof(UInt64),
            typeof(Double), typeof(Single)
        };

        private readonly IMapperProvider _provider;
        public DefaultMapper(IMapperProvider provider)
        {
            _provider = provider;
        }

        public TTarget Convert(TSource source)
        {
            throw new NotImplementedException();
        }

        protected virtual Func<TSource, TTarget> GenerirConvertDelegate()
        {
            Type sType = typeof(TSource);
            Type tType = typeof(TTarget);
            List<PropertyInfo> spis = sType.GetProperties(BindingFlags.Public).ToList();
            List<PropertyInfo> tpis = tType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            ParameterExpression sourcePar = Expression.Parameter(sType, "source");
            Expression targetVar = Expression.Variable(tType, "target");
            Expression newExpression = Expression.Assign(targetVar, Expression.New(tType));

            foreach (var pi in spis)
            {
                var property = tpis.FirstOrDefault(p => p.Name == pi.Name);
                property = property ?? tpis.FirstOrDefault(p => p.Name.Equals(pi.Name, StringComparison.OrdinalIgnoreCase));
                if (property == null)
                {
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
                }
                if (property == null)
                {
                    continue;
                }

                Type spType = pi.PropertyType;
                Type tpType = property.PropertyType;

                if (spType.IsClass && tpType.IsClass)
                {

                }

            }

            return null;
        }

        protected virtual List<Expression> ConvertProperty(Type sourceType, Type targetType)
        {
            return null;
        }

        protected virtual Expression ConvertPrimitiveType(ParameterExpression source, PropertyInfo spi, Expression target, PropertyInfo tpi)
        {
            if (spi.PropertyType == tpi.PropertyType)
            {
                return Expression.Assign(
                    Expression.MakeMemberAccess(target, tpi),
                    Expression.MakeMemberAccess(source, spi)
                    );
            }
            else if (numbericTypes.Contains(spi.PropertyType) && numbericTypes.Contains(tpi.PropertyType))
            {
                //数字类型互转
                var ttype = tpi.PropertyType;
                // 原值
                var svalVar = Expression.Variable(spi.PropertyType);
                var sval = Expression.Assign(svalVar, Expression.MakeMemberAccess(source, spi));

                // 最大值与最小值
                var tminVar = Expression.Variable(tpi.PropertyType);
                var tmaxVar = Expression.Variable(tpi.PropertyType);

                var minFieldInfo = tpi.PropertyType.GetField(nameof(byte.MinValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var maxFieldInfo = tpi.PropertyType.GetField(nameof(byte.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var minVal = Expression.Assign(tminVar, Expression.MakeMemberAccess(null, minFieldInfo));
                var maxVal = Expression.Assign(tmaxVar, Expression.MakeMemberAccess(null, maxFieldInfo));

                var assign = Expression.IfThen(
                Expression.AndAlso(
                    Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, sval, minVal),
                    Expression.MakeBinary(ExpressionType.LessThanOrEqual, sval, maxVal)),
                Expression.Assign(
                     Expression.MakeMemberAccess(target, tpi),
                     Expression.Convert( sval, tpi.PropertyType)));

               return Expression.Block(
                    new ParameterExpression[] { svalVar, tminVar, tmaxVar },
                    sval, minVal, maxVal, assign
                    );

            }

            return null;
        }
    }
}
