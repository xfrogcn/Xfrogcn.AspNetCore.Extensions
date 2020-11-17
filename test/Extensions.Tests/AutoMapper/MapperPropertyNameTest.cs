using System;
using Microsoft.Extensions.DependencyInjection;
using Xfrogcn.AspNetCore.Extensions;
using Xunit;

namespace Extensions.Tests.AutoMapper
{
    public class MapperPropertyNameTest
    {
        public class StructA
        {
            public int X { get; set; }

            public int Y { get; set; }
        }

        public class SourceA
        {
            [MapperPropertyName(Name = "X1", TargetType = typeof(TargetA))]
            public string A { get; set; }

            [MapperPropertyName(Name = "D", TargetType =typeof(TargetBase))]
            public string B { get; set; }

            [MapperPropertyName(Name = "X2", TargetType = typeof(TargetA))]
            public string C { get; set; }

            [MapperPropertyName(Name = "TSA")]
            public StructA SA { get; set; }
        }

        public class TargetBase
        {
            public string A { get; set; }

            public string D { get; set; }

            [MapperPropertyName(Name = "C", SourceType = typeof(SourceA))]
            public string E { get; set; }

            public StructA TSA { get; set; }
        }

        public class TargetA : TargetBase
        {
            public string X1 { get; set; }

            public string X2 { get; set; }
        }

        [Fact(DisplayName = "Mapper Convert")]
        public void TestMapper()
        {
            // SourceA --> TargetA
            // A==>A B==>D C==>E  A==>X1 C==>X2
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            SourceA a = new SourceA()
            {
                A = "A",
                B = "B",
                C = "C",
                SA =new StructA()
                {
                    X = 1,
                    Y = 2
                }
            };
            var ta = provider.Convert<SourceA, TargetA>(a);
            Assert.Equal("A", ta.A);
            Assert.Equal("B", ta.D);
            Assert.Equal("C", ta.E);
            Assert.Equal("A", ta.X1);
            Assert.Equal("C", ta.X2);
            Assert.False(Object.ReferenceEquals(ta.TSA, a.SA));
            Assert.Equal(1, ta.TSA.X);
            Assert.Equal(2, ta.TSA.Y);
        }


        [Fact(DisplayName = "Mapper CopyTo")]
        public void TestCopyTo()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            SourceA a = new SourceA()
            {
                A = "A",
                B = "B",
                C = "C"
            };
            TargetA ta = new TargetA();
            provider.CopyTo<SourceA, TargetA>(a, ta);
            Assert.Equal("A", ta.A);
            Assert.Equal("B", ta.D);
            Assert.Equal("C", ta.E);
            Assert.Equal("A", ta.X1);
            Assert.Equal("C", ta.X2);
        }


        [Fact(DisplayName = "Mapper CopyDefine")]
        public void TestCopyDefine()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            SourceA a = new SourceA()
            {
                A = "A",
                B = "B",
                C = "C"
            };
            TargetA ta = new TargetA();
            var copyFunc = provider.DefineCopyTo<SourceA, TargetA>();
            copyFunc(a, ta);
            Assert.Equal("A", ta.A);
            Assert.Equal("B", ta.D);
            Assert.Equal("C", ta.E);
            Assert.Equal("A", ta.X1);
            Assert.Equal("C", ta.X2);

            var copyFunc2 = provider.DefineCopyTo<SourceA, TargetA>(t => new
            {
                A = t.A
            });

            ta = new TargetA();
            copyFunc2(a, ta);
            Assert.Null(ta.A);
            Assert.Equal("B", ta.D);
            Assert.Equal("C", ta.E);
            Assert.Null(ta.X1);
            Assert.Equal("C", ta.X2);

        }
    }
}
