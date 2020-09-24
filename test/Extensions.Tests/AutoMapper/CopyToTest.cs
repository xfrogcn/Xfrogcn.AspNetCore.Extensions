using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xfrogcn.AspNetCore.Extensions;
using Xunit;

namespace Extensions.Tests.AutoMapper
{
    [Trait("", "AutoMapper")]
    public class CopyToTest
    {
        class S1
        {
            public string A { get; set; }
        }

        class S2
        {
            public string A { get; set; }
        }

        class S3 : S1
        {
            public S2 B { get; set; }
        }

       class S4 : S2
        {
            public S1 B { get; set; }
        }

        [Fact(DisplayName = "单值类型-无效果")]
        public void Test1()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            provider.CopyTo(1, 1);
            int? a = 0;
            provider.CopyTo<int?, int?>(null, a);
        }

        [Fact(DisplayName = "列表")]
        public void Test2()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            List<string> target = new List<string>();
            provider.CopyTo(new List<string> { "A", "B" }, target);
            Assert.Equal("A", target[0]);
            Assert.Equal("B", target[1]);

            string[] invalidList = new string[0];
            provider.CopyTo(new List<String> { "A" }, invalidList);

            List<S2> t2 = new List<S2>();
            provider.CopyTo(new List<S1> { new S1 { A = "10" } }, t2);
            Assert.Single(t2);
            Assert.Equal("10", t2[0].A);

            List<S3> t3 = new List<S3>()
            {
                new S3(){ A = "A", B = new S2(){ A = "B"}},
                new S3(){ A = "A1", B = new S2(){ A = "B1"}},
            };
            List<S4> t4 = new List<S4>();
            provider.CopyTo(t3, t4);
            Assert.Equal(2, t4.Count);
            Assert.Equal("B1", t4[1].B.A);

        }

        [Fact(DisplayName = "字典")]
        public void Test3()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            Dictionary<string,string> dic = new Dictionary<string, string>();
            provider.CopyTo(new Dictionary<string, string>
            {
                {"A","B" }
            }, dic);
            Assert.Single(dic);
            Assert.Equal("B", dic["A"]);

            Dictionary<S3, S4> s1 = new Dictionary<S3, S4>()
            {
                {
                    new S3{ A = "A", B = new S2{A="A1"}}, new S4{ A = "B", B = new S1{A="B1"}}
                }
            };
            Dictionary<S4, S3> t1 = new Dictionary<S4, S3>();
            provider.CopyTo(s1, t1);
            Assert.Single(t1);
            Assert.Equal("A1", t1.Keys.ToList()[0].B.A);
            Assert.Equal("B1", t1.Values.ToList()[0].B.A);
        }

        [Fact(DisplayName = "类")]
        public void Test4()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            S3 s1 = new S3() { A = "A", B = new S2 { A = "B" } };
            S4 t1 = new S4() { B = new S1() };
            var t1_b = t1.B;
            provider.CopyTo(s1, t1);
            Assert.Equal("A", t1.A);
            Assert.Equal("B", t1.B.A);
            Assert.Equal(t1_b, t1.B);

            t1.B = null;
            provider.CopyTo(s1, t1);
            Assert.Equal("B", t1.B.A);

            s1.B = null;
            t1.B = null;
            provider.CopyTo(s1, t1);
            Assert.Null(t1.B);

            s1.B = null;
            provider.CopyTo(s1, t1);
            Assert.Null(t1.B);
        }

        class S5 : S1
        {
            public int B { get; set; }

            public long C { get; set; }

            [MapperPropertyName(Name = "D1")]
            public byte? D { get; set; }

            public S5 E { get; set; }
        }

        class S6 : S2
        {
            public string B { get; set; }

            public int C { get; set; }

            
            public decimal D1 { get; set; }

            public S6 E { get; set; }
        }

        [Fact(DisplayName = "DefineCopyTo")]
        public void Test5()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            var a1 = provider.DefineCopyTo<S1, S2>(m => m.A);
            S2 t1 = new S2();
            S1 s1 = new S1() { A = "10" };
            a1(s1, t1);
            Assert.Null(t1.A);

            a1 = provider.DefineCopyTo<S1, S2>(null);
            a1(s1, t1);
            Assert.Equal("10", t1.A);

            // 去掉CD属性
            var a2 = provider.DefineCopyTo<S5, S6>(m => new S5
            {
                C = m.C,
                D = m.D
            });
            S5 s2 = new S5() { A = "A", B = 10, C = 2, D = 1, E = new S5() { C = 20, D = 10 } };
            S6 t2 = new S6();
            a2(s2, t2);
            Assert.Equal("A", t2.A);
            Assert.Null(t2.B);
            Assert.Equal(0, t2.C);
            Assert.Equal(0, t2.D1);
            Assert.Equal(20, t2.E.C);
            Assert.Equal(10, t2.E.D1);

            a2 = provider.DefineCopyTo<S5, S6>(m => new S5
            {
                E = new S5
                {
                    C = 0,
                    D  = null,
                }
            });
            t2.E = new S6();
            a2(s2, t2);
            Assert.Equal(2, t2.C);
            Assert.Equal(0, t2.E.C);
            Assert.Equal(0, t2.E.D1);
        }
    }
}
