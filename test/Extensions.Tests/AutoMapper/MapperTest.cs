using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xfrogcn.AspNetCore.Extensions;
using Xunit;

namespace Extensions.Tests.AutoMapper
{
    [Trait("", "AutoMapper")]
    public class MapperTest
    {
        public class S1
        {
            [MapperPropertyName(Name = "B", TargetType = typeof(T2))]
            public int A { get; set; }
        }

        public class T1
        {
            public int A { get; set; }
        }

        public class T1_2 : T1
        {
            public int B { get; set; }
        }

        [Fact(DisplayName = "基础")]
        public void Test1()
        {
            IServiceCollection sc = new ServiceCollection()
                .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            var mapper = provider.GetMapper<S1, T1>();

            var s1 = new S1() { A = 10 };
            var t2 = mapper.Convert(s1);
            Assert.IsType<T1>(t2);
            Assert.Equal(10, t2.A);

        }

        [Fact(DisplayName = "特性1对2")]
        public void Test2()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            var mapper = provider.GetMapper<S1, T1_2>();

            var s1 = new S1() { A = 10 };
            var t2 = mapper.Convert(s1);
            Assert.IsType<T1_2>(t2);
            Assert.Equal(10, t2.A);
            Assert.Equal(10, t2.B);
        }

        public class S2
        {
            public int? A { get; set; }
        }

        public class T2
        {
            public int? A { get; set; }
        }

        [Fact(DisplayName = "可空类型")]
        public void Test3()
        {

          
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            var mapper = provider.GetMapper<S2, T1>();

            var s2 = new S2() { A = null };
            var t2 = mapper.Convert(s2);
            Assert.IsType<T1>(t2);
            Assert.Equal(0, t2.A);

            s2 = new S2() { A = 10 };
            t2 = mapper.Convert(s2);
            Assert.IsType<T1>(t2);
            Assert.Equal(10, t2.A);

            var mapper2 = provider.GetMapper<T1, S2>();
            var t1 = new T1() { A = 10 };
            var s3 = mapper2.Convert(t1);
            Assert.Equal(10, s3.A);
        }

        public class S3
        {
            public decimal? A { get; set; }

            public decimal B { get; set; }

            public decimal? C { get; set; }
        }

        public class T3
        {
            public byte A { get; set; }

            public byte? B { get; set; }

            public byte? C { get; set; }
        }

        [Fact(DisplayName = "数字类型转换")]
        public void Test4()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            var mapper = provider.GetMapper<S3, T3>();

            var s3 = new S3()
            {
                A = 1,
                B = 1,
                C = 1
            };
            var t3 = mapper.Convert(s3);
            Assert.IsType<T3>(t3);
            Assert.Equal(1, t3.A);
            Assert.Equal(1, t3.B.Value);
            Assert.Equal(1, t3.C.Value);

            s3 = new S3()
            {
                A = null,
                B = 300,
                C = null
            };
            t3 = mapper.Convert(s3);
            Assert.Equal(0, t3.A);
            Assert.Null(t3.B);
            Assert.Null(t3.C);
        }

        public class S4
        {
            [MapperPropertyName(Name ="T1")]
            public S1 S1 { get; set; }
        }

        public class T4
        {
            public T1 T1 { get; set; }
        }

        [Fact(DisplayName = "子类型")]
        public void Test5()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            var mapper = provider.GetMapper<S4, T4>();

            var s4 = new S4();
            var t4 = mapper.Convert(s4);
            Assert.Null(t4.T1);

            s4 = new S4() { S1 = new S1() { A = 10 } };
            t4 = mapper.Convert(s4);
            Assert.NotNull(t4.T1);
            Assert.Equal(10, t4.T1.A);
        }


        

        [Fact(DisplayName = "字典")]
        public void Test6()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>()
            {
                { "A", 10 },
                {"B", 300 }
            };
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();
            var mapper = provider.GetMapper<Dictionary<string, int>, Dictionary<string, byte>>();
            var t = mapper.Convert(dic);
            Assert.Equal(10, t["A"]);
            Assert.Equal(0, t["B"]);

            Dictionary<S1, S1> dic2 = new Dictionary<S1, S1>()
            {
                { new S1{A =10}, new S1{A = 20} },
                { new S1{A= 15}, new S1{A=  15} }
            };

            var mapper2 = provider.GetMapper<Dictionary<S1, S1>, Dictionary<S2, T1_2>>();
            var t2 = mapper2.Convert(dic2);
            Assert.Equal(10, t2.Keys.ToList()[0].A);
            Assert.Equal(15, t2.Keys.ToList()[1].A);
            Assert.Equal(15, t2.Values.ToList()[1].B);
        }


        [Fact(DisplayName = "列表")]
        public void Test7()
        {
            IServiceCollection sc = new ServiceCollection()
               .AddLightweightMapper();

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            // array ==> list
            var m1 = provider.GetMapper<string[], List<string>>();
            var t1 = m1.Convert(new string[] { "A", "B" });
            Assert.Equal(2, t1.Count);
            Assert.Equal("A", t1[0]);
            Assert.Equal("B", t1[1]);

            // array ==> array
            var m2 = provider.GetMapper<ValueWrapper<decimal[]>, ValueWrapper<byte[]>>();
            var t2 = m2.Convert(new ValueWrapper<decimal[]>() { Value = new decimal[] { 1, 2, 300 } });
            Assert.Equal(3, t2.Value.Length);
            Assert.Equal(1, t2.Value[0]);
            Assert.Equal(2, t2.Value[1]);
            //越界
            Assert.Equal(0, t2.Value[2]);

            // 如果目标是Object，取原类型
            var m3 = provider.GetMapper<List<S1>, List<object>>();
            var t3 = m3.Convert(new List<S1>() { new S1 { A = 10 } });
            Assert.IsType<S1>(t3[0]);
            Assert.Equal(10, (t3[0] as S1).A);
           

        }

        [Fact(DisplayName = "注册自定义转换")]
        public void Test8()
        {
            IServiceCollection sc = new ServiceCollection()
              .AddLightweightMapper(options=>
              {
                  options.AddConvert<S1, T1_2>((s, t) =>
                  {
                      t.B = t.A + 1;
                  });
                  // 基类转换 ,在父类的转换会作用于子类
                  options.AddConvert<S1, T1>((s, t) =>
                  {
                      t.A = t.A + 2;
                  });
              });

            IServiceProvider sp = sc.BuildServiceProvider();
            IMapperProvider provider = sp.GetRequiredService<IMapperProvider>();

            var m = provider.GetMapper<S1, T1_2>();
            var t1 = m.Convert(new S1() { A = 10 });
            Assert.Equal(11, t1.B);
            // 注意先后关系
            Assert.Equal(12, t1.A);
        }
    }
}
