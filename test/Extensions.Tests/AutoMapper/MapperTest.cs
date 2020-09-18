using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Extensions.Tests.AutoMapper
{
    [Trait("", "AutoMapper")]
    public class MapperTest
    {
        [Fact(DisplayName = "Test")]
        public void Test1()
        {
            bool b = typeof(int).IsPrimitive;
            UInt16Converter c = new UInt16Converter();
            decimal x = -100;
        
            var mi = typeof(byte).GetField("MaxValue", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            Expression.MakeMemberAccess(null, mi);
        }
    }
}
