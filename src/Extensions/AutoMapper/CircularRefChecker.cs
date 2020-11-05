using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions.AutoMapper
{
    public class CircularRefChecker
    {
        internal static MethodInfo GetValueMethod = typeof(CircularRefChecker).GetMethod(nameof(GetValue), BindingFlags.Public | BindingFlags.Instance);
        internal static MethodInfo SetInstanceMethod = typeof(CircularRefChecker).GetMethod(nameof(SetInstance), BindingFlags.Public | BindingFlags.Instance);

        class RefKey
        {
            public object Instance { get; set; }

            public Type TargetType { get; set; }

            public RefKey(object instance, Type targetType)
            {
                Instance = instance;
                TargetType = targetType;
            }

            public override int GetHashCode()
            {
                return (Instance == null ? 0 : Instance.GetHashCode()) ^ (TargetType == null ? 0 : TargetType.GetHashCode());
            }
        }
        readonly Hashtable _cache = new Hashtable();

        public object GetValue(object source, Type targetType)
        {
            RefKey key = new RefKey(source, targetType);
            return _cache[key];
        }

        public void SetInstance(object source, Type targetType, object targetInstance)
        {
            RefKey key = new RefKey(source, targetType);
            _cache[key] = targetInstance;
        }
    }
}
