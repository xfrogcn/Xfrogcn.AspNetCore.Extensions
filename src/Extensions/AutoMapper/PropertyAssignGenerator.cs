using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions.AutoMapper
{
    public class PropertyAssignGenerator
    {
        private static Type[] numbericTypes = new Type[]
        {
            typeof(Byte), typeof(SByte),
            typeof(Int16), typeof(UInt16),
            typeof(Int32),typeof(UInt32),
            typeof(Int64), typeof(UInt64),
            typeof(Single),typeof(Double),
            typeof(Decimal)
        };

        ParameterExpression _sourcePar;
        ParameterExpression _targetPar;
        ParameterExpression _crCheckerPar;
        Type _sourceType;
        Type _targetType;
        IMapperProvider _mapper;

        public PropertyAssignGenerator(
            ParameterExpression sourcePar,
            ParameterExpression targetPar,
            ParameterExpression crCheckerPar,
            IMapperProvider mapper)
            : this(sourcePar, targetPar, crCheckerPar, sourcePar.Type, targetPar.Type, mapper)
        {

        }

        public PropertyAssignGenerator(
            ParameterExpression sourcePar, 
            ParameterExpression targetPar,
            ParameterExpression crCheckerPar,
            Type sourceType,
            Type targetType,
            IMapperProvider mapper)
        {
            _sourcePar = sourcePar;
            _targetPar = targetPar;
            _crCheckerPar = crCheckerPar;
            _sourceType = sourceType;
            _targetType = targetType;
            _mapper = mapper;
        }

        public virtual Expression GenerateExpression(bool genClass= false)
        {
            var exp = ConvertNullableType();
            if(exp == null)
            {
                exp = ConvertIListType();
            }
            if(exp == null)
            {
                exp = ConvertIDictionaryType();
            }
            if(exp == null)
            {
                if (_sourceType == typeof(string) && _targetType == typeof(string))
                {
                    return Expression.Assign(_targetPar, _sourcePar);
                }
                if (_sourceType.IsInterface && _targetType.IsInterface && _sourceType == _targetType)
                {
                    return Expression.Assign(_targetPar, _sourcePar);
                }
            }
            if(exp == null)
            {
                exp = ConvertEnumTypeToString();
            }
            if(exp == null)
            {
                exp = ConvertStringToEnumType();
            }
            if(exp == null && genClass)
            {
                exp = ConvertClassType();
            }
            return exp;
        }

        #region 枚举
        public virtual Expression ConvertEnumTypeToString()
        {
            if (_sourceType.IsEnum && _targetType == typeof(string))
            {
                var dic = getEnumNames(_sourceType);
                MethodInfo mi = this.GetType().GetMethod("convertEnumToString", BindingFlags.NonPublic | BindingFlags.Instance);

                return Expression.Assign(_targetPar,
                    Expression.Call(Expression.Constant(this), mi, Expression.Convert(_sourcePar, typeof(Enum)), Expression.Constant(dic)));
            }
            return null;
        }

        public virtual Expression ConvertStringToEnumType()
        {
            bool isNullableTarget = _targetType.IsGenericType && _targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type tType = _targetType;
            if (isNullableTarget)
            {
                tType = _targetType.GetGenericArguments()[0];
            }

            if (_sourceType == typeof(string) && tType.IsEnum)
            {
                var tmpDic = getEnumNames(tType);
                Dictionary<string, Enum> dic = new Dictionary<string, Enum>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in tmpDic)
                {
                    dic.Add(kv.Value, kv.Key);
                }

                MethodInfo mi = this.GetType().GetMethod("convertStringToEnum", BindingFlags.NonPublic | BindingFlags.Instance);
                ParameterExpression eVar = Expression.Variable(typeof(Enum));
                var exp = Expression.Assign(eVar, Expression.Call(Expression.Constant(this), mi, _sourcePar, Expression.Constant(tType), Expression.Constant(dic)));

                return Expression.Block(
                    new ParameterExpression[] { eVar },
                    exp,
                    Expression.IfThen(
                        Expression.NotEqual(eVar, Expression.Constant(null, typeof(Enum))),
                        Expression.Assign(_targetPar, Expression.Convert(exp, _targetType)
                    )));
            }
            return null;
        }


        private Dictionary<Enum, string> getEnumNames(Type enumType)
        {
            Dictionary<Enum, string> dic = new Dictionary<Enum, string>();
            var fis = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach(var f in fis)
            {
                string name = f.Name.ToLower();
                var attr = f.GetCustomAttribute<MapperEnumNameAttribute>();
                if(attr!=null && !string.IsNullOrEmpty(attr.Name))
                {
                    name = attr.Name;
                }
                dic.Add((Enum)f.GetValue(null), name);
            }
            return dic;
        }

        private string convertEnumToString(Enum val, Dictionary<Enum,string> names)
        {
            if (names.ContainsKey(val))
            {
                return names[val];
            }
            return val.ToString().ToLower();
        }

        private Enum convertStringToEnum(string val, Type enumType, Dictionary<string, Enum> names)
        {
            if(names.ContainsKey(val))
            {
                return names[val];
            }

            object t;
            if(Enum.TryParse(enumType, val, out t))
            {
                return (Enum)t;
            }
            return default;
        }

        #endregion

        #region 类
        public virtual Expression ConvertClassType()
        {
            if (_sourceType.IsClass && _targetType.IsClass && _targetType != typeof(string))
            {
                Expression provider = Expression.Constant(_mapper);
                MethodInfo mi = _mapper.GetType().GetMethod(nameof(IMapperProvider.GetMapper));

                mi = mi.MakeGenericMethod(_sourceType, _targetType);
                var mapperType = typeof(IMapper<,>).MakeGenericType(_sourceType, _targetType);
                ParameterExpression mapperVar = Expression.Variable(mapperType);
                Expression assign = Expression.Assign(mapperVar, Expression.Call(provider, mi));

                MethodInfo convertMethod = mapperType.GetMethod("Convert");

                var assign2 = Expression.Assign(
                    _targetPar,
                    Expression.Call(mapperVar, convertMethod, _sourcePar, _crCheckerPar));

                //ParameterExpression cacheValueVar = ParameterExpression.Variable(_targetType);
                //Expression cacheValueAssign = Expression.Assign(
                //    cacheValueVar,
                //    Expression.Convert(Expression.Call(_crCheckerPar, CircularRefChecker.GetValueMethod, Expression.Convert(_sourcePar, typeof(object)), Expression.Constant(_targetType)), _targetType));
                //Expression.IfThenElse(
                //    Expression.NotEqual(cacheValueAssign, Expression.Constant(null)),
                //    Expression.Assign(_targetPar, cacheValueVar),
                //    Expression.Block(
                //        new ParameterExpression[] { mapperVar },
                //        assign,
                //        assign2
                //    ));

                var ifExp = Expression.IfThen(
                    Expression.NotEqual(_sourcePar, Expression.Constant(null)),
                    Expression.Block(
                    new ParameterExpression[] { mapperVar },
                    assign,
                    assign2
                    ));

                return ifExp;
            }
            return null;
        }

        public virtual Expression GenerateCopyToExpression()
        {
            if (_sourceType.IsClass && _targetType.IsClass)
            {

                Expression provider = Expression.Constant(_mapper);
                MethodInfo mi = _mapper.GetType().GetMethod(nameof(IMapperProvider.GetMapper));

                mi = mi.MakeGenericMethod(_sourceType, _targetType);
                var mapperType = typeof(IMapper<,>).MakeGenericType(_sourceType, _targetType);
                ParameterExpression mapperVar = Expression.Variable(mapperType);
                Expression assign = Expression.Assign(mapperVar, Expression.Call(provider, mi));

                MethodInfo convertMethod = mapperType.GetMethod("CopyTo");

                var assign2 = Expression.Call(mapperVar, convertMethod, _sourcePar, _targetPar, _crCheckerPar);

                var ifExp = Expression.IfThen(
                    Expression.AndAlso(
                        Expression.NotEqual(_sourcePar, Expression.Constant(null)),
                        Expression.NotEqual(_targetPar, Expression.Constant(null))),
                    Expression.Block(
                        new ParameterExpression[] { mapperVar },
                        assign,
                        assign2
                        )
                    );

                return ifExp;
            }
            return null;
        }

        #endregion

        #region 值类型
        public virtual Expression ConvertNullableType()
        {
            bool isNullableSource = _sourceType.IsGenericType && _sourceType.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type sType = _sourceType;
            if (isNullableSource)
            {
                sType = _sourceType.GetGenericArguments()[0];
            }
   
            if(isNullableSource)
            {
                var g = new PropertyAssignGenerator(_sourcePar, _targetPar, _crCheckerPar, sType, _targetType, _mapper);
                var exp = g.GenerateExpression();
                if (exp != null)
                {
                    return Expression.IfThen(
                        Expression.NotEqual(_sourcePar, Expression.Constant(null, _sourceType)),
                        exp);
                }
            }

            return ConvertValueType();
        }

        public virtual Expression ConvertValueType()
        {
            if (_sourceType.IsValueType)
            {
                bool isNullableTarget = _targetType.IsGenericType && _targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
                Type tType = _targetType;
                if (isNullableTarget)
                {
                    tType = _targetType.GetGenericArguments()[0];
                }

                if(_sourceType == tType)
                {
                    return Expression.Assign(_targetPar, Expression.Convert(_sourcePar, _targetType));
                }


                return ConvertNumbericType();
            }

            return null;
        }

        public virtual Expression ConvertNumbericType()
        {
            bool isNullableTarget = _targetType.IsGenericType && _targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type tType = _targetType;
            if (isNullableTarget)
            {
                tType = _targetType.GetGenericArguments()[0];
            }

            if ((numbericTypes.Contains(_sourceType) || _sourceType.IsEnum) &&
                (numbericTypes.Contains(tType) || tType.IsEnum))
            {


                Type mType = tType;
                if (_targetType.IsEnum)
                {
                    mType = typeof(int);
                }

                // 最大值与最小值
                var tminVar = Expression.Variable(mType);
                var tmaxVar = Expression.Variable(mType);

                var minFieldInfo = mType.GetField(nameof(byte.MinValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var maxFieldInfo = mType.GetField(nameof(byte.MaxValue), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var minVal = Expression.Assign(tminVar, Expression.MakeMemberAccess(null, minFieldInfo));
                var maxVal = Expression.Assign(tmaxVar, Expression.MakeMemberAccess(null, maxFieldInfo));

                Type decimalType = typeof(Decimal);
                List<Expression> expList = new List<Expression>();
                var assign = Expression.IfThen(
                    Expression.AndAlso(
                        Expression.MakeBinary(
                            ExpressionType.GreaterThanOrEqual,
                            Expression.Convert(_sourceType.IsEnum ? Expression.Convert(_sourcePar, typeof(int)) : (Expression)_sourcePar, decimalType),
                            Expression.Convert(tminVar, decimalType)),
                        Expression.MakeBinary(
                            ExpressionType.LessThanOrEqual,
                            Expression.Convert(_sourceType.IsEnum ? Expression.Convert(_sourcePar, typeof(int)) : (Expression)_sourcePar, decimalType),
                            Expression.Convert(tmaxVar, decimalType))),
                    Expression.Assign(
                         _targetPar,
                         Expression.Convert(_sourcePar, _targetType)));

                expList.Add(minVal);
                expList.Add(maxVal);

      
                expList.Add(assign);

                return Expression.Block(
                     new ParameterExpression[] { tminVar, tmaxVar },
                     expList
                     );

            }

            return null;
        }
        #endregion

        #region 列表类型
        public virtual Expression ConvertIListType()
        {
            bool isSourceList = IsEnumerableType(_sourceType);
            bool isTargetList = IsListType(_targetType);
            if (isSourceList && (isTargetList || _targetType.IsArray))
            {
                var sType = typeof(object);
                if (_sourceType.IsArray)
                {
                    sType = _sourceType.GetElementType();
                }
                else if (_sourceType.IsGenericType)
                {
                    sType = _sourceType.GetGenericArguments()[0];
                }
                var tType = typeof(object);
                if (_targetType.IsGenericType)
                {
                    tType = _targetType.GetGenericArguments()[0];
                }
                else if (_targetType.IsArray)
                {
                    tType = _targetType.GetElementType();
                }
                if (tType == typeof(object))
                {
                    tType = sType;
                }

                MethodInfo mi = this.GetType().GetMethod(nameof(ListConvert), BindingFlags.Instance | BindingFlags.NonPublic);
                mi = mi.MakeGenericMethod(sType, _targetType, tType);

                Expression tlistInstance = Expression.Convert(Expression.Constant(null), _targetType);
                if (isTargetList)
                {
                    // 先New
                    tlistInstance = Expression.New(_targetType);
                }

                return Expression.Assign(
                    _targetPar,
                    Expression.Call(
                        Expression.Constant(this),
                        mi,
                        _sourcePar, tlistInstance));
            }
            return null;
        }

        public static bool IsListType(Type type)
        {
            var listType = typeof(IList<>);
            bool isList = false;
            if (type.IsGenericType)
            {
                isList = type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == listType);
            }
            return isList;
        }

        public static bool IsEnumerableType(Type type)
        {
            var listType = typeof(IEnumerable<>);
            bool isList = false;
            if (type.IsArray && type.GetElementType() != typeof(object))
            {
                return true;
            }
            if (type.IsGenericType)
            {
                isList = type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == listType);
            }
            return isList;
        }

        private TTargetList ListConvert<TItem, TTargetList, TTargetItem>(IEnumerable<TItem> source, TTargetList tlist)
        {
            if (source == null)
            {
                return default;
            }

            var mapper = _mapper.GetMapper<TItem, TTargetItem>();

            if (typeof(TTargetList).IsArray)
            {
                var list = new TTargetItem[source.Count()];
                var itor = source.GetEnumerator();
                int i = 0;
                while (itor.MoveNext())
                {
                    list[i] = mapper.Convert(itor.Current);
                    i++;
                }
                return (TTargetList)(object)list;
            }
            else if (IsListType(typeof(TTargetList)))
            {
                IList list = tlist as IList;
                if (list != null)
                {
                    var itor = source.GetEnumerator();
                    while (itor.MoveNext())
                    {
                        list.Add(mapper.Convert(itor.Current));
                    }
                }
                return tlist;
            }
            return default;
        }

        #endregion

        #region 字典
        public virtual Expression ConvertIDictionaryType()
        {
            bool isSourceDic = IsDictionaryType(_sourceType);
            bool isTargetDic = IsDictionaryType(_targetType);
            if (isSourceDic && isTargetDic)
            {
                var sTypes = _sourceType.GetGenericArguments();
                var tType = _targetType.GetGenericArguments();

                MethodInfo mi = this.GetType().GetMethod(nameof(DictionayConvert), BindingFlags.Instance | BindingFlags.NonPublic);
                mi = mi.MakeGenericMethod(sTypes[0], sTypes[1], _targetType, tType[0], tType[1]);

                return Expression.Assign(
                    _targetPar,
                    Expression.Call(
                        Expression.Constant(this),
                        mi,
                        _sourcePar));
            }
            return null;
        }

        public static bool IsDictionaryType(Type type)
        {
            var dicType = typeof(IDictionary<,>);
            bool isDic = false;
            if (type.IsGenericType)
            {
                isDic = type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == dicType);
            }

            return isDic;
        }


        private TTargetDicValue DictionayConvert<TKey, TValue, TTargetDicValue, TTargetKey, TTargetValue>(IDictionary<TKey, TValue> source)
             where TTargetDicValue : IDictionary<TTargetKey, TTargetValue>, new()
        {
            if (source == null)
            {
                return default;
            }
            TTargetDicValue dic = new TTargetDicValue();
            var keyMapper = _mapper.GetMapper<TKey, TTargetKey>();
            var valueMapper = _mapper.GetMapper<TValue, TTargetValue>();
            foreach (var kv in source)
            {
                var tKeyVal = keyMapper.Convert(kv.Key);
                if (tKeyVal == null)
                {
                    continue;
                }
                var tVal = valueMapper.Convert(kv.Value);
                dic.Add(tKeyVal, tVal);
            }
            return dic;
        }

        #endregion
    }
}
