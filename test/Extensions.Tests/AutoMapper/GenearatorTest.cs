using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.AutoMapper;
using Xunit;

namespace Extensions.Tests.AutoMapper
{

    [Trait("", "Generator")]
    public class GenearatorTest
    {
        enum TestEnum
        {
            [MapperEnumName("hello")]
            A,
            B
        }

        [Fact(DisplayName = "ValueType")]
        public void Test1()
        {

            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            // int ==> int
            var intConv = GenerateConverter<int, int>(provider);
            Assert.Equal(10, intConv(10));

            var conv2 = GenerateConverter<int?, int>(provider);
            Assert.Equal(0, conv2(null));
            Assert.Equal(10, conv2(10));

            var conv3 = GenerateConverter<byte?, int>(provider);
            Assert.Equal(0, conv3(null));
            Assert.Equal(10, conv3(10));

            var conv4 = GenerateConverter<int?, byte?>(provider);
            Assert.Null(conv4(null));
            Assert.Equal((byte)10, conv4(10));
            Assert.Null(conv4(300));



            var conv5 = GenerateConverter<TestEnum, int>(provider);
            Assert.Equal(0, conv5(TestEnum.A));
            Assert.Equal(1, conv5(TestEnum.B));

            var conv6 = GenerateConverter<int, TestEnum>(provider);
            Assert.Equal(TestEnum.B, conv6(1));
            Assert.Equal((TestEnum)10, conv6(10));

            var conv7 = GenerateConverter<DateTime?, DateTime>(provider);
            Assert.Equal(new DateTime(2010, 1, 1), conv7(new DateTime(2010, 1, 1)));

            // enum --> string
            var conv8 = GenerateConverter<TestEnum, string>(provider);
            Assert.Equal("hello", conv8(TestEnum.A));
            Assert.Equal("b", conv8(TestEnum.B));
            Assert.Equal("10", conv8((TestEnum)10));

            // string --> enum
            var conv9 = GenerateConverter<string, TestEnum>(provider);
            Assert.Equal(TestEnum.A, conv9("hello"));
            Assert.Equal(TestEnum.B, conv9("b"));
            Assert.Equal((TestEnum)10, conv9("10"));
            Assert.Equal(TestEnum.A, conv9("11A"));

            var conv10 = GenerateConverter<string, TestEnum?>(provider);
            Assert.Equal(TestEnum.A, conv10("hello"));
            Assert.Equal(TestEnum.B, conv10("b"));
            Assert.Equal((TestEnum)10, conv10("10"));
            Assert.Null(conv10("11A"));

            var conv11 = GenerateConverter<List<string>, string>(provider);
            conv11(new List<string>() { "A" });
        }


        private Func<TSource, TTarget> GenerateConverter<TSource, TTarget>(IMapperProvider provider)
        {
            ParameterExpression s = Expression.Parameter(typeof(TSource));
            ParameterExpression t = Expression.Variable(typeof(TTarget));
            PropertyAssignGenerator g = new PropertyAssignGenerator(s,t,provider);
            var expression = g.GenerateExpression();

            Expression<Func<TSource, TTarget>> lam = Expression.Lambda<Func<TSource, TTarget>>(
                Expression.Block(new ParameterExpression[] { t }, expression, t), s);
            Func<TSource, TTarget> func = lam.Compile();
            return func;
        }
    }
}
